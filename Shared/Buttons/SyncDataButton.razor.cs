namespace RestInventorySystem.Shared.Buttons;

public partial class SyncDataButton : IDisposable
{
	private bool IsSaving = false;
	private Timer timer;

	public SyncDataButton()
	{
		// Initialize the timer with a callback function and a state object (null in this case)
		timer = new Timer(SyncDataCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
	}

	private async void SyncDataCallback(object state)
	{
		if (IsSaving)
		{
			return;
		}

		IsSaving = true;
		await UserRepository.FlushAsync();
		await SpareRepository.FlushAsync();
		await ActivityLogRepository.FlushAsync();
		IsSaving = false;
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			timer?.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
