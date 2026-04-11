namespace SampleMauiMvvmApp.ViewModels
{
    public partial class OnboardingViewModel : BaseViewModel
    {
        public ReadingService readingService;
        public ReadingExportService readingExportService;

        public OnboardingViewModel(ReadingService _readingService, ReadingExportService _readingExportService)
        {
            this.readingService = _readingService;
            CheckUserLoginDetails();
            this.readingExportService = _readingExportService;
        }

        private async void CheckUserLoginDetails()
        {
            await Task.Delay(1000);
            IsBusy = true;

            var token = await SecureStorage.GetAsync("Token");
            if (string.IsNullOrEmpty(token))
            {
                IsBusy = false;
                await GoToLoginPage();
            }
            else
            {
                var jsonToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;

                if (jsonToken.ValidTo < DateTime.UtcNow)
                {
                    SecureStorage.Remove("Token");
                    await GoToLoginPage();
                }
                else
                {
                    var role = jsonToken.Claims.FirstOrDefault(q => q.Type.Equals(ClaimTypes.Role))?.Value;
                    App.UserInfo = new UserInfo()
                    {
                        Username = jsonToken.Claims.FirstOrDefault(q => q.Type.Equals(ClaimTypes.Email))?.Value,
                        Role = role,
                    };

                    IsBusy = false;
                    await GoToMainPage();
                }
            }
        }

        public async Task CheckIfValidToken()
        {
            await Task.Delay(1000);
            IsBusy = true;

            var token = await SecureStorage.GetAsync("Token");

            SecureStorage.Remove("Token");
            Preferences.Default.Clear();

            if (string.IsNullOrEmpty(token))
            {
                IsBusy = false;
                await GoToLoginPage();
            }
            else
            {
                var jsonToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;

                if (jsonToken.ValidTo < DateTime.UtcNow)
                {
                    SecureStorage.Remove("Token");
                    Preferences.Default.Clear();
                    await GoToLoginPage();
                }
                else
                {
                    string userId = jsonToken.Claims.First(claim => claim.Type == "sub").Value;
                    string email = jsonToken.Claims.First(claim => claim.Type == "email").Value;

                    App.UserInfo = new UserInfo()
                    {
                        Id = jsonToken.Claims.First(claim => claim.Type == "sub").Value,
                        Username = jsonToken.Claims.First(claim => claim.Type == "email").Value
                    };

                    IsBusy = false;
                    await GoToMainPage();
                }
            }
        }

        [RelayCommand]
        public async Task GetInitializationData()
        {
            await CheckIfValidToken();
            return;

            //await readingService.GetListOfReadingExportFromSql();
        }

        [RelayCommand]
        public async Task GetNewExportData()
        {
            IsBusy = true;
            await readingExportService.CheckForNewExportInSql();
            IsBusy = false;
        }

        private async Task GoToLoginPage()
        {
            await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
        }

        private async Task GoToMainPage()
        {
            await Shell.Current.GoToAsync($"//{nameof(MonthCustomerTabPage)}"); ;
        }
    }
}