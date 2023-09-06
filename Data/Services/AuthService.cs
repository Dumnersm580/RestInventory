namespace RestInventorySystem.Data.Services
{
	internal class AuthService
	{
		private readonly Repository<User> _userRepository;
		private readonly SessionService _sessionService;

		public User CurrentUser { get; private set; }

		public AuthService(Repository<User> userRepository, SessionService sessionService)
		{
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
		}

		public async Task<string> SeedInitialUser()
		{
			if (_userRepository.GetAll().Any() || _userRepository.Contains(x => x.Role, UserRole.Admin))
				return null;

			string username = "admin";
			User user = new()
			{
				UserName = username,
				Email = "Please Change!",
				FullName = "Please Change!",
				PasswordHash = Hasher.HashSecret(username),
				Role = UserRole.Admin,
				CreatedBy = Guid.Empty,
			};
			_userRepository.Add(user);
			await _userRepository.FlushAsync();
			return username;
		}

		public void Register(string username, string email, string fullname, UserRole role)
		{
			if (_userRepository.HasUserName(username))
				throw new Exception("Username already exists!");

			User user = new()
			{
				UserName = username,
				Email = email,
				FullName = fullname,
				PasswordHash = Hasher.HashSecret(username),
				Role = role,
				CreatedBy = CurrentUser.Id,
			};
			_userRepository.Add(user);
		}

		public async Task<bool> Login(string userName, string password, bool stayLoggedIn)
		{
			CurrentUser = _userRepository.Get(x => x.UserName, userName);
			if (CurrentUser == null || !Hasher.VerifyHash(password, CurrentUser.PasswordHash))
				return false;

			Session session = Session.Generate(CurrentUser.Id, stayLoggedIn);
			await _sessionService.SaveSession(session);
			return true;
		}

		public bool IsUserAdmin()
		{
			return CurrentUser?.Role == UserRole.Admin;
		}

		public void ChangePassword(string oldPassword, string newPassword)
		{
			if (oldPassword == newPassword)
				throw new Exception("New password must be different from the current password.");

			CurrentUser.PasswordHash = Hasher.HashSecret(newPassword);
			CurrentUser.HasInitialPassword = false;
		}

		public void LogOut()
		{
			_sessionService.DeleteSession();
			CurrentUser = null;
		}

		public async Task CheckSession()
		{
			Session session = await _sessionService.LoadSession();
			if (session == null)
				return;

			User user = _userRepository.Get(x => x.Id, session.UserId);
			if (user == null || !session.IsValid())
				throw new Exception("Session expired!");

			CurrentUser = user;
		}
	}
}
