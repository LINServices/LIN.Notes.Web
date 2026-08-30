using LIN.Access.Identity.Platform;
using LIN.Access.Identity.Platform.Controllers.Identities;
using LIN.Access.Identity.Platform.Hubs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LIN.Notes.Web.Client.Pages;

public partial class Login : IDisposable
{
    /// <summary>
    /// Gestor de navegación.
    /// </summary>
    [Inject]
    private NavigationManager? NavigationManager { get; set; }

    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    /// <summary>
    /// Indica si se está iniciando sesión con una llave de acceso (PassKey).
    /// </summary>
    private bool _isWithKey = false;

    /// <summary>
    /// Nombre de usuario ingresado.
    /// </summary>
    private string _username = string.Empty;

    /// <summary>
    /// Contraseña ingresada.
    /// </summary>
    private string _password = string.Empty;

    /// <summary>
    /// Mensaje mostrado durante procesos de carga.
    /// </summary>
    private string _loadingMessage = "Iniciando Sesión";

    /// <summary>
    /// Mensaje de error a mostrar en la UI.
    /// </summary>
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Indica si hay un proceso de inicio de sesión activo.
    /// </summary>
    private bool _isLoggingIn = false;

    /// <summary>
    /// Indica si se debe mostrar una animación.
    /// </summary>
    private bool _isAnimating = false;

    /// <summary>
    /// Visibilidad del mensaje de error.
    /// </summary>
    private string _errorVisibility = "hidden";

    /// <summary>
    /// Organizaciones candidatas cuando el login es ambiguo (la identidad pertenece a 2+ organizaciones).
    /// </summary>
    private List<OrgAlternative> _orgAlternatives = [];

    /// <summary>
    /// Indica si se debe mostrar el botón para cancelar una espera de aprobación de PassKey.
    /// </summary>
    private bool _isCancelVisible = false;

    /// <summary>
    /// Conexión activa al hub de PassKey mientras se espera la aprobación desde otro dispositivo.
    /// </summary>
    private PassKeyHub? _passKeyHub;

    /// <summary>
    /// Key del intent de PassKey en curso, devuelto por <see cref="PassKeys.CreateIntent"/>.
    /// </summary>
    private string? _passKeyIntentKey;

    /// <summary>
    /// Inicialización del componente.
    /// </summary>
    protected override void OnInitialized()
    {
        if (SessionAuth.IsOpen)
        {
            NavigationManager?.NavigateTo("/");
            return;
        }
        base.OnInitialized();
    }

    /// <summary>
    /// Hace visibles los controles de entrada.
    /// </summary>
    private void ShowControls()
    {
        _isLoggingIn = false;
        StateHasChanged();
    }

    /// <summary>
    /// Oculta los controles de entrada.
    /// </summary>
    private void HideControls()
    {
        _isLoggingIn = true;
        StateHasChanged();
    }

    /// <summary>
    /// Alterna entre el modo de inicio de sesión normal y PassKey.
    /// </summary>
    private void TogglePassKeyMode()
    {
        _isWithKey = !_isWithKey;
        HideError();
        StateHasChanged();
    }

    /// <summary>
    /// Oculta el mensaje de error.
    /// </summary>
    private void HideError()
    {
        _errorVisibility = "hidden";
        StateHasChanged();
    }

    /// <summary>
    /// Muestra un mensaje de error en la UI.
    /// </summary>
    /// <param name="message">Mensaje a mostrar.</param>
    private void ShowError(string message)
    {
        _errorVisibility = "visible";
        _errorMessage = message;
        StateHasChanged();
    }

    /// <summary>
    /// Inicia el proceso de autenticación.
    /// </summary>
    private async void StartAuthentication()
    {
        if (_isWithKey)
        {
            await StartPassKeyLogin();
            return;
        }

        _loadingMessage = "Iniciando Sesión";
        HideControls();
        HideError();

        // Validar información de entrada.
        if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password))
        {
            ShowControls();
            ShowError("Completa todos los campos");
            return;
        }

        await AttemptLogin(null);
    }

    /// <summary>
    /// Inicia el proceso de autenticación mediante PassKey: crea el intent de login y espera,
    /// por el hub en tiempo real, la aprobación desde un dispositivo ya autenticado.
    /// </summary>
    private async Task StartPassKeyLogin()
    {
        HideError();

        if (string.IsNullOrWhiteSpace(_username))
        {
            ShowError("Usuario requerido");
            return;
        }

        var appKey = Configuration["lin:key"];

        if (!Guid.TryParse(appKey, out var applicationKey))
        {
            ShowError("Inténtalo más tarde");
            return;
        }

        _loadingMessage = "Esperando aprobación desde tu otro dispositivo...";
        _isCancelVisible = true;
        HideControls();

        var intent = await PassKeys.CreateIntent(_username, applicationKey);

        if (intent.Response != Responses.Success)
        {
            _isCancelVisible = false;
            ShowControls();

            if (intent.Response == Responses.InvalidUser)
                ShowError($"No existe el usuario {_username}");
            else if (intent.Response == Responses.UnauthorizedByApp)
                ShowError("Esta aplicación no está autorizada");
            else
                ShowError("Inténtalo más tarde");

            return;
        }

        _passKeyIntentKey = intent.Model.Key;

        _passKeyHub = new PassKeyHub();
        _passKeyHub.OnApprovalResult += OnPassKeyApprovalResult;
        _passKeyHub.OnError += OnPassKeyError;

        try
        {
            await _passKeyHub.ConnectAsync();
            await _passKeyHub.WaitApprovalAsync(_passKeyIntentKey);
        }
        catch
        {
            await DisconnectPassKeyHub();
            _isCancelVisible = false;
            ShowControls();
            ShowError("Inténtalo más tarde");
        }
    }

    /// <summary>
    /// Se ejecuta cuando el hub reporta el resultado de la aprobación del intent de PassKey.
    /// </summary>
    private async void OnPassKeyApprovalResult(object? sender, PassKeyApprovalResult e)
    {
        if (e.Key != _passKeyIntentKey)
            return;

        await DisconnectPassKeyHub();

        if (e.Status == "Success")
        {
            var login = await SessionAuth.LoginWith(e.Token);

            if (login.Response == Responses.Success)
            {
                await CompleteLogin();
                return;
            }

            _isCancelVisible = false;
            ShowControls();
            ShowError("Inténtalo más tarde");
            return;
        }

        _isCancelVisible = false;
        ShowControls();

        switch (e.Status)
        {
            case "Rejected":
                ShowError("La solicitud fue rechazada desde tu otro dispositivo");
                break;
            case "Expired":
                ShowError("La solicitud expiró, inténtalo de nuevo");
                break;
            case "BlockedByOrg":
                ShowError("Tu organización no permite este inicio de sesión");
                break;
            default:
                ShowError(e.Message ?? "Inténtalo más tarde");
                break;
        }
    }

    /// <summary>
    /// Se ejecuta cuando el hub reporta un error de negocio (no una excepción de conexión).
    /// </summary>
    private void OnPassKeyError(object? sender, string message)
    {
        _ = DisconnectPassKeyHub();
        _isCancelVisible = false;
        ShowControls();
        ShowError(message);
    }

    /// <summary>
    /// Cancela la espera de aprobación de PassKey en curso.
    /// </summary>
    private async void CancelPassKey()
    {
        await DisconnectPassKeyHub();
        _isCancelVisible = false;
        ShowControls();
    }

    /// <summary>
    /// Desconecta y limpia la conexión activa al hub de PassKey, si existe.
    /// </summary>
    private async Task DisconnectPassKeyHub()
    {
        if (_passKeyHub is null)
            return;

        _passKeyHub.OnApprovalResult -= OnPassKeyApprovalResult;
        _passKeyHub.OnError -= OnPassKeyError;

        try
        {
            await _passKeyHub.DisconnectAsync();
        }
        catch
        {
        }

        _passKeyHub = null;
        _passKeyIntentKey = null;
    }

    /// <summary>
    /// Libera la conexión al hub de PassKey si el componente se destruye mientras hay una espera activa.
    /// </summary>
    public void Dispose()
    {
        if (_passKeyHub is not null)
        {
            _passKeyHub.OnApprovalResult -= OnPassKeyApprovalResult;
            _passKeyHub.OnError -= OnPassKeyError;
            _ = _passKeyHub.DisconnectAsync();
        }
    }

    /// <summary>
    /// Intenta iniciar sesión, opcionalmente para una organización específica (caso de login ambiguo).
    /// </summary>
    /// <param name="organizationId">Id de la organización elegida, si aplica.</param>
    private async Task AttemptLogin(int? organizationId)
    {
        var (login, response) = await SessionManager.Instance.StarSession(_username, _password, true);

        if (response == Responses.Success)
        {
            await CompleteLogin();
            return;
        }

        // LoginWith no expone las Alternatives cuando falla (devuelve Sesion null); se reconsulta
        // directamente contra Authentication.Login para poder leerlas y armar el selector.
        if (response == Responses.InvalidParam)
        {
            var appKey = Configuration["lin:key"];

            if (Guid.TryParse(appKey, out var applicationKey))
            {
                var raw = await Authentication.Login(_username, _password, applicationKey, organizationId);

                if (raw.Response == Responses.InvalidParam && raw.Alternatives.Count > 0)
                {
                    _orgAlternatives = ParseAlternatives(raw.Alternatives);

                    if (_orgAlternatives.Count > 0)
                    {
                        ShowControls();
                        StateHasChanged();
                        return;
                    }
                }
            }

            ShowControls();
            ShowError("No fue posible determinar tu organización");
            return;
        }
        else if (response == Responses.InvalidPassword)
        {
            ShowControls();
            ShowError("La contraseña es incorrecta");
        }
        else if (response == Responses.NotExistAccount)
        {
            ShowControls();
            ShowError($"No existe el usuario {_username}");
        }
        else if (response == Responses.UnauthorizedByOrg)
        {
            ShowControls();
            ShowError("Tu organización no permite que accedas a esta app");
        }
        else
        {
            ShowControls();
            ShowError("Inténtalo más tarde");
        }
    }

    /// <summary>
    /// Finaliza un login exitoso (con contraseña o PassKey): fija el contexto de organización y navega al home.
    /// </summary>
    private async Task CompleteLogin()
    {
        //    _orgAlternatives = [];
        //    OrganizationContext.SetOrganization(SessionAuth.Instance.Account.Identity.OwnerOrganizationId);

        //    var orgMe = await Organizations.ReadMe(SessionAuth.Instance.AccountToken);
        //    if (orgMe.Response == Responses.Success)
        //        OrganizationContext.SetOrganization(orgMe.Model);

        NavigationManager?.NavigateTo("/");
    }

    /// <summary>
    /// Reintenta el login con la organización seleccionada por el usuario.
    /// </summary>
    /// <param name="alternative">Organización elegida.</param>
    private async void SelectOrganization(OrgAlternative alternative)
    {
        HideControls();
        await AttemptLogin(alternative.Id);
    }

    /// <summary>
    /// Deserializa la lista de alternativas (cada elemento es un string JSON anidado con { Id, Name }).
    /// </summary>
    private static List<OrgAlternative> ParseAlternatives(List<object> alternatives)
    {
        var result = new List<OrgAlternative>();

        foreach (var item in alternatives)
        {
            try
            {
                var json = item?.ToString();
                if (string.IsNullOrWhiteSpace(json))
                    continue;

                var alternative = JsonSerializer.Deserialize<OrgAlternative>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (alternative is not null)
                    result.Add(alternative);
            }
            catch (JsonException)
            {
                // Elemento no deserializable, se omite.
            }
        }

        return result;
    }

    /// <summary>
    /// Organización candidata en un login ambiguo.
    /// </summary>
    public sealed class OrgAlternative
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
