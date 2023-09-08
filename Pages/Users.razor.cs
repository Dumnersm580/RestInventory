namespace RestInventorySystem.Pages;

public partial class Users
{
	public const string Route = "/users";

	private readonly bool Dense = true;
	private readonly bool Fixed_header = true;
	private readonly bool Fixed_footer = true;
	private readonly bool Hover = true;
	private readonly bool ReadOnly = false;
	private readonly bool VanCancelEdit = true;
	private readonly bool BlockSwitch = true;
	private string SearchString;
	private User ElementBeforeEdit;
	private readonly TableApplyButtonPosition ApplyButtonPosition = TableApplyButtonPosition.End;
	private readonly TableEditButtonPosition EditButtonPosition = TableEditButtonPosition.End;
	private readonly TableEditTrigger EditTrigger = TableEditTrigger.RowClick;
	private IEnumerable<User> Elements;
    private Guid? PartToDeleteId;
    [CascadingParameter]
	private Action<string> SetAppBarTitle { get; set; }

	protected override void OnInitialized()
	{
		SetAppBarTitle.Invoke("Manage Users");
		Elements = UserRepository.GetAll();
	}

	private void BackupItem(object element)
	{
		ElementBeforeEdit = ((User)element).Clone() as User;
	}

	private string GetUserName(Guid id)
	{
		var username = UserRepository.Get(x => x.Id, id)?.UserName;
		return username is null ? "N/A" : username;
	}

	private void ResetItemToOriginalValues(object element)
	{
		((User)element).UserName = ElementBeforeEdit.UserName;
		((User)element).Email = ElementBeforeEdit.Email;
		((User)element).FullName = ElementBeforeEdit.FullName;
		((User)element).Role = ElementBeforeEdit.Role;
	}

	private bool FilterFunc(User element)
	{
		return string.IsNullOrWhiteSpace(SearchString)
			   || element.Id.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
			   || element.UserName.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
			   || element.Email.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
			   || element.FullName.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
			   || element.HasInitialPassword.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
			   || element.CreatedAt.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase);
	}

	private async Task AddDialog()
	{
		DialogParameters parameters = new()
		{
			{ "ChangeParentState", new Action(StateHasChanged) }
		};
		await DialogService.ShowAsync<Shared.Dialogs.AddUserDialog>("Add User", parameters);
	}

	private void Reload()
	{
		StateHasChanged();
	}

    private async Task DeleteUser(Guid userId)
    {
        PartToDeleteId = userId; // You can reuse the existing PartToDeleteId property for this purpose

        var confirmationResult = await DialogService.ShowMessageBox(
            "Delete User",
            "Are you sure you want to delete this user?",
            yesText: "Yes",
            noText: "No"
        );

        if (confirmationResult == true)
        {
            try
            {
                var user = Elements.FirstOrDefault(x => x.Id == userId);
                if (user != null)
                {
                    UserRepository.Remove(user);
                    Snackbar.Add("User deleted successfully.", Severity.Success);
                }

                // Refresh the Elements collection with the updated list of users after deletion
                Elements = UserRepository.GetAll();

                // Notify Blazor that a state change has occurred and it should re-render the component
                StateHasChanged();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during deletion
                Snackbar.Add($"An error occurred while deleting the user: {ex.Message}", Severity.Error);
            }
        }
    }
}