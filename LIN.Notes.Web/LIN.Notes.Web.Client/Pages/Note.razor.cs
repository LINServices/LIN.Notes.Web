using LIN.Notes.Shared.Modals;

namespace LIN.Notes.Web.Client.Pages;

public partial class Note
{

    /// <summary>
    /// Id de la nota.
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery]
    public int Id { get; set; }


    /// <summary>
    /// Modelo de la nota.
    /// </summary>
    private NoteDataModel? NoteDataModel { get; set; }


    /// <summary>
    /// Titulo.
    /// </summary>
    private string Tittle { get; set; } = string.Empty;


    /// <summary>
    /// Contenido.
    /// </summary>
    private string Content { get; set; } = string.Empty;


    /// <summary>
    /// Modal de eliminar.
    /// </summary>
    private DeleteModal? DeleteModal { get; set; }


    /// <summary>
    /// Evento al establecer los parámetros.
    /// </summary>
    protected override void OnParametersSet()
    {

        // Obtener la nota.
        var note = Home.Notes?.Models?.Where(t => t.Id == Id).FirstOrDefault();

        // Si la nota no existe.
        NoteDataModel = note ?? new();

        // Valores
        Tittle = note?.Tittle ?? string.Empty;
        Content = note?.Content ?? string.Empty;

        base.OnParametersSet();
    }


    /// <summary>
    /// Volver a atrás.
    /// </summary>
    private async void Back() => await JSRuntime.InvokeVoidAsync("backLast");


    /// <summary>
    /// Establecer el nuevo color.
    /// </summary>
    private void SetColor()
    {
        GetClass();
    }


    /// <summary>
    /// Obtener las clases.
    /// </summary>
    string GetClass()
    {
        switch (NoteDataModel?.Color)
        {
            case 1:
                return "bg-salmon/50 dark:bg-salmon/20";
            case 2:
                return "bg-glass/50 dark:bg-glass/20";
            case 3:
                return "bg-cream-green/50 dark:bg-cream-green/20";
            case 4:
                return "bg-cream-purple/50 dark:bg-cream-purple/20";
            default:
                break;
        }

        return "bg-yell/50 dark:bg-yell/20";

    }


    /// <summary>
    /// Input.
    /// </summary>
    private async void Input(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {


        c += 1;
        int my = c;

        await Task.Delay(500);

        if (c != my)
            return;

        string value = e.Value?.ToString() ?? "";

        if (NoteDataModel == null)
            return;

        NoteDataModel.Content = value;

        await Save();

    }

    int c = 0;


    async void InputTittle(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {


        c += 1;
        int my = c;

        await Task.Delay(500);

        if (c != my)
            return;


        string value = e.Value?.ToString() ?? "";


        if (NoteDataModel == null)
            return;


        NoteDataModel.Tittle = value;

        await Save();
    }




    static async Task<int> Create(NoteDataModel model)
    {

        // Respuesta.
        CreateResponse response;


        // Solicitud a la API.
        response = await Access.Notes.Controllers.Notes.Create(new Types.Notes.Models.NoteDataModel()
        {
            Color = model.Color,
            Content = model.Content,
            Tittle = model.Tittle
        },
        Session.Instance.Token);

        // Respuesta.
        return response.Response == Responses.Success ? response.LastID : 0;


    }



    async void SetNewColor(int color)
    {

        if (NoteDataModel == null)
            return;

        NoteDataModel.Color = color;


        ResponseBase response = new();



        if (NoteDataModel.Id > 0)
            response = await Access.Notes.Controllers.Notes.Update(NoteDataModel.Id, color, Session.Instance.Token);



        StateHasChanged();


    }


    async void Delete()
    {
        if (NoteDataModel == null)
            return;


        // Respuesta.
        ResponseBase response = new()
        {
            Response = Responses.Undefined
        };

        // Respuesta de la API.
        if (NoteDataModel.Id > 0)
            response = await Access.Notes.Controllers.Notes.Delete(NoteDataModel.Id, Session.Instance.Token);

        Home.Notes?.Models.RemoveAll(t => t.Id == NoteDataModel.Id);
        NavigationManager.NavigateTo("Home");
        StateHasChanged();

    }



    bool IsSaving = false;

    /// <summary>
    /// Guardar los cambios.
    /// </summary>
    private async Task Save()
    {

        // Validar parámetros.
        if (NoteDataModel == null || IsSaving)
            return;

        // Establecer nuevo estado.
        IsSaving = true;

        // Respuesta.
        ResponseBase response = new()
        {
            Response = Responses.Undefined
        };

        // Respuesta de la API.
        if (NoteDataModel.Id > 0)
            _ = await Access.Notes.Controllers.Notes.Update(NoteDataModel, Session.Instance.Token);

        // Crear local.
        else if (NoteDataModel.Id == 0)
        {

            // Id.
            NoteDataModel.Id = new Random().Next(-10000, -2);

            // Es creado.
            int isCreated = await Create(NoteDataModel);

            // Esta confirmado
            bool isConfirmed = !(isCreated <= 0);


            // Si esta confirmado.
            if (isConfirmed)
                NoteDataModel.Id = isCreated;

            // Agregar al home.
            Home.Notes?.Models.Add(NoteDataModel);

            // Establecer.
            IsSaving = false;
            return;
        }


        // Nuevo estado.
        IsSaving = false;

    }


}