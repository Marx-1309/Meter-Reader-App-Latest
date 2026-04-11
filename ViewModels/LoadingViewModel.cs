using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;

//using SampleMauiMvvmApp.Helpers;
using SampleMauiMvvmApp.Models;
using SampleMauiMvvmApp.Services;
using SampleMauiMvvmApp.Views;
using SampleMauiMvvmApp.Views.SecurityPages;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SampleMauiMvvmApp.ViewModels
{
    [QueryProperty("loggedin", "loggedin")]
    public partial class LoadingViewModel : BaseViewModel
    {
        [ObservableProperty]
        private string loggedin;

        public ReadingService readingService;
        public ReadingExportService readingExportService;

        public LoadingViewModel(ReadingService _readingService, ReadingExportService _readingExportService)
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

        [RelayCommand]
        public Task GetInitializationDataAsync(string? loggedin)
        {
            return Task.CompletedTask;
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