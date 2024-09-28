namespace LIN.Notes.Web.Client.Pages;

public partial class Home : IDisposable
{

    /// <summary>
    /// Instancia.
    /// </summary>
    public static Home? Instance { get; private set; }


    /// <summary>
    /// Notas actuales.
    /// </summary>
    public static ReadAllResponse<NoteDataModel>? Notes { get; set; }


    /// <summary>
    /// Filtro de color.
    /// </summary>
    private int Color = -1;


    /// <summary>
    /// Constructor.
    /// </summary>
    public Home()
    {
        Instance = this;
    }


    /// <summary>
    /// Evento al inicializar.
    /// </summary>
    protected override void OnInitialized()
    {
        // Cargar datos.
        Load();

        // Evento al actualizar la sesión.
        Access.Notes.Observers.SessionObserver.OnUpdate += OnUpdateSession;

        base.OnInitialized();
    }


    /// <summary>
    /// Al actualizar la sesión.
    /// </summary>
    /// <param name="e">Session.</param>
    private void OnUpdateSession(object? sender, Session e)
    {
        if (e.Type == SessionType.Connected)
            InvokeAsync(() => RefreshData(true));

        InvokeAsync(StateHasChanged);
    }


    /// <summary>
    /// Obtener datos desde el servicio.
    /// </summary>
    private async void Load()
    {
        try
        {
            // Rellena los datos
            await RefreshData();
            _ = InvokeAsync(StateHasChanged);
        }
        catch (Exception)
        {
        }
    }


    /// <summary>
    /// Refrescar los datos desde el servicio.
    /// </summary>
    private async Task RefreshData(bool force = false)
    {

        // Si no es forzado y los datos están cargados.
        if (!force && Notes != null)
            return;

        // Estado cambio.
        _ = InvokeAsync(StateHasChanged);

        // Respuestas.
        Notes = await Access.Notes.Controllers.Notes.ReadAll(Session.Instance.Token);

        // Actualizar pantalla.
        _ = InvokeAsync(StateHasChanged);
        return;
    }


    /// <summary>
    /// Cerrar la sesión.
    /// </summary>
    private async void CloseSession()
    {
        await InvokeAsync(() =>
        {
            Access.Notes.Observers.SessionObserver.Dispose();
            Access.Auth.SessionAuth.CloseSession();
            NavigationManager?.NavigateTo("/");
        });
    }


    /// <summary>
    /// Navegar a una nota.
    /// </summary>
    /// <param name="note">Modelo.</param>
    private void Go(NoteDataModel note)
    {

        // Url.
        var url = NavigationManager.BaseUri + "note";

        // Agregar parámetros.
        url = Global.Utilities.Network.Web.AddParameters(url, new Dictionary<string, string>()
        {
            {"Id", note.Id.ToString() }
        });

        // Navegar.
        NavigationManager.NavigateTo(url);

    }


    /// <summary>
    /// Ir a crear nueva nota.
    /// </summary>
    private void Create() => Go(new());


    /// <summary>
    /// Limpiar.
    /// </summary>
    /// <param name="notas">Lista de notas.</param>
    public async void RemoveOne(int id)
    {
        await InvokeAsync(() =>
         {
             Notes?.Models.RemoveAll(t => t.Id == id);
             StateHasChanged();
         });
    }


    /// <summary>
    /// Limpiar.
    /// </summary>
    public async void UpdateColor(int id, int color)
    {
        await InvokeAsync(() =>
        {
            // Color.
            foreach (var note in Notes?.Models.Where(t => t.Id == id) ?? [])
            {
                note.Color = color;
            }
            StateHasChanged();
        });
    }


    /// <summary>
    /// Establecer el color.
    /// </summary>
    /// <param name="color">Color.</param>
    private void SetColor(int color)
    {
        Color = color;
        InvokeAsync(StateHasChanged);
    }


    /// <summary>
    /// Dispose.
    /// </summary>
    public void Dispose()
    {
        // Eliminar evento al actualizar la sesión.
        Access.Notes.Observers.SessionObserver.OnUpdate -= OnUpdateSession;
    }

}