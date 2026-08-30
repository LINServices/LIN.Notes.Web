using LIN.Types.Notes.Enumerations;

namespace LIN.Notes.Web.Client.Pages;

public partial class Movements
{

    [Inject]
    private LIN.Access.Notes.Sessions.Abstractions.ISession Session { get; set; } = null!;

    private ReadAllResponse<MovementDataModel>? MovementsList { get; set; }

    private bool ShowForm { get; set; }
    private bool IsSaving { get; set; }

    private decimal NewAmount { get; set; }
    private MovementType NewType { get; set; } = MovementType.Expense;
    private DateTime NewDate { get; set; } = DateTime.Today;

    private decimal TotalIncome => MovementsList?.Models
        .Where(m => m.Type == MovementType.Income)
        .Sum(m => m.Amount) ?? 0;

    private decimal TotalExpense => MovementsList?.Models
        .Where(m => m.Type == MovementType.Expense)
        .Sum(m => m.Amount) ?? 0;

    private decimal Balance => TotalIncome - TotalExpense;

    protected override void OnInitialized() => Load();

    private async void Load()
    {
        try
        {
            MovementsList = await Access.Notes.Controllers.Movements.ReadAll(Session.Token);
            await InvokeAsync(StateHasChanged);
        }
        catch { }
    }

    private static string FormatAmount(decimal amount) => $"${amount:N2}";

    private async Task Create()
    {
        if (NewAmount <= 0 || IsSaving)
            return;

        IsSaving = true;

        var model = new MovementDataModel
        {
            Amount = NewAmount,
            Type = NewType,
            Date = NewDate
        };

        var response = await Access.Notes.Controllers.Movements.Create(model, Session.Token);

        if (response.Response == Responses.Success)
        {
            model.Id = response.LastId;
            MovementsList?.Models.Add(model);
            NewAmount = 0;
            NewDate = DateTime.Today;
            ShowForm = false;
        }

        IsSaving = false;
        StateHasChanged();
    }

    private async Task DeleteMovement(MovementDataModel movement)
    {
        MovementsList?.Models.Remove(movement);
        StateHasChanged();

        if (movement.Id > 0)
            await Access.Notes.Controllers.Movements.Delete(movement.Id, Session.Token);
    }

}
