using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeterReaderApp.Models;
using MeterReaderApp.Services;
using System.Collections.ObjectModel;

namespace MeterReaderApp.ViewModels
{
    public partial class CustomerMapViewModel : BaseViewModel
    {
        private readonly CustomerMapService _customerMapService;
        private readonly ReadingService _readingService;

        [ObservableProperty]
        private ObservableCollection<ReadingDto> customers = new();

        public CustomerMapViewModel(CustomerMapService customerMapService, ReadingService readingService)
        {
            Title = "Customer Map";
            _customerMapService = customerMapService;
            _readingService = readingService;
        }

        [RelayCommand]
        public async Task LoadCustomersAsync(string readingsStatus)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var response = await _customerMapService.GetCustomersWithCoordinatesAsync(readingsStatus);
                Customers = new ObservableCollection<ReadingDto>(response ?? new List<ReadingDto>());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadCustomersAsync error: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task SaveCustomerLocationAsync(string customerNo, decimal latitude, decimal longitude)
        {
            if (string.IsNullOrEmpty(customerNo)) return;

            try
            {
                bool isSuccess = await _customerMapService.UpdateCustomerLocationAsync(customerNo, latitude, longitude);

                if (isSuccess)
                    await Shell.Current.DisplayAlert("Success", "Customer location updated.", "OK");
                else
                    await Shell.Current.DisplayAlert("Error", "Failed to update location.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveCustomerLocationAsync error: {ex}");
            }
        }
    }
}
