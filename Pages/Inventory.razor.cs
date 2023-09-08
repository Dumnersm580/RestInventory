namespace RestInventorySystem.Pages;

public partial class Inventory
{
	public const string Route = "/inventory";

	private readonly bool Dense = true;
	private readonly bool Fixed_header = true;
	private readonly bool Fixed_footer = true;
	private readonly bool Hover = true;
	private bool ReadOnly = false;
	private readonly bool CanCancelEdit = true;
	private readonly bool BlockSwitch = true;
	private string SearchString;
	private Spare SelectedItem;
	private Spare ElementBeforeEdit;
	private readonly TableApplyButtonPosition ApplyButtonPosition = TableApplyButtonPosition.End;
	private readonly TableEditButtonPosition EditButtonPosition = TableEditButtonPosition.End;
	private readonly TableEditTrigger EditTrigger = TableEditTrigger.RowClick;
	private IEnumerable<Spare> Elements;
	private readonly Dictionary<Guid, bool> SpareDescTracks = new();
    private Guid? PartToDeleteId; // Add this line

    [CascadingParameter]
	private Action<string> SetAppBarTitle { get; set; }

	protected sealed override void OnInitialized()
	{

		SetAppBarTitle.Invoke("Manage Robot Parts");
		Elements = SpareRepository.GetAll();
		if (!AuthService.IsUserAdmin())
		{
			ReadOnly = true;
		}
		foreach (Spare s in Elements)
		{
			SpareDescTracks.Add(s.Id, false);
		}
	}

	private void BackupItem(object element)
	{
		ElementBeforeEdit = ((Spare)element).Clone() as Spare;
	}

	private void ResetItemToOriginalValues(object element)
	{
		((Spare)element).Name = ElementBeforeEdit.Name;
		((Spare)element).Description = ElementBeforeEdit.Description;
		((Spare)element).Company = ElementBeforeEdit.Company;
		((Spare)element).Price = ElementBeforeEdit.Price;
		((Spare)element).AvailableQuantity = ElementBeforeEdit.AvailableQuantity;
	}

	private bool FilterFunc(Spare element)
	{
		return string.IsNullOrWhiteSpace(SearchString)
			   || element.Id.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
			   || element.Name.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
			   || element.Description.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
			   || element.Company.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
			   || element.Price.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
			   || element.AvailableQuantity.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase);
	}

	private void ShowBtnPress(Guid id)
	{
		SpareDescTracks[id] = !SpareDescTracks[id];
	}

	private bool GetShow(Guid id)
	{
		return SpareDescTracks.ContainsKey(id) ? SpareDescTracks[id] : (SpareDescTracks[id] = false);
	}

	private string GetLastTakenOut(Guid id)
	{
		List<ActivityLog> log = ActivityLogRepository.GetAll().Where(x => x.SpareID == id && x.Action == StockAction.Deduct && x.ApprovalStatus == ApprovalStatus.Approve).ToList();
		return log.Count == 0 ? "N/A" : log.Max(x => x.ApprovalStatusOn).ToString();
	}

	private async Task AddDialog()
	{
		DialogParameters parameters = new()
		{
			{ "ChangeParentState", new Action(StateHasChanged) }
		};
		await DialogService.ShowAsync<AddSpareDialog>("Add Spare", parameters);
	}

	private async Task ActOnStock(Spare spare, StockAction action)
	{
		if (action == StockAction.Deduct)
		{
			if (!ApproveButton.ValidateWeekAndTime(Snackbar))
			{
				return;
			}

			if (spare.AvailableQuantity == 0)
			{
				Snackbar.Add("Out of Stock!", Severity.Error);
				return;
			}
		}
		DialogParameters parameters = new()
		{
			{ "StockAction", action },
			{ "Spare",  spare},
			{ "ChangeParentState", new Action(StateHasChanged) }
		};
		await DialogService.ShowAsync<StockActionDialog>($"{Enum.GetName(action)} Stock", parameters);
	}


    private async Task DeletePart(Guid partId)
    {
        PartToDeleteId = partId;

        var confirmationResult = await DialogService.ShowMessageBox(
            "Delete Part",
            "Are you sure you want to delete this part?",
            yesText: "Yes",
            noText: "No"
        );

        if (confirmationResult == true)
        {
            try
            {
                var item = Elements.FirstOrDefault(x => x.Id == partId);
                if (item != null)
                {
                    SpareRepository.Remove(item);
                    Snackbar.Add("Part deleted successfully.", Severity.Success);
                }

                // Refresh the Elements collection with the updated list of spares after deletion
                Elements = SpareRepository.GetAll();

                // Notify Blazor that a state change has occurred and it should re-render the component
                StateHasChanged();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during deletion
                Snackbar.Add($"An error occurred while deleting the part: {ex.Message}", Severity.Error);
            }
        }
    }



}