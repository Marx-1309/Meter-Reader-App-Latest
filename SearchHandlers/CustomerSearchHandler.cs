namespace SampleMauiMvvmApp.SearchHandlers
{
    public partial class CustomerSearchHandler : SearchHandler
    {
        public CustomerSearchHandler()
        {
        }

        public CustomerService customerService;
        public DbContext dbContext;

        public IList<Reading> Readings { get; set; }
        public string NavigationRoute { get; set; }
        public Type NavigationType { get; set; }

        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);

            if (string.IsNullOrWhiteSpace(newValue))
            {
                ItemsSource = null;
            }
            else
            {
                //ItemsSource = customerWrappers.Where(customer => customer.Custname.ToLower().Contains(newValue.ToLower()));
                ItemsSource = Readings.Where(reading => reading.ReadingInfo.ToLower().Contains(newValue.ToLower()));
            }
        }

        protected override async void OnItemSelected(object item)
        {
            if (item is Reading reading)
            {
                Customer customer = new Customer()
                {
                    CUSTNMBR = reading.CUSTOMER_NUMBER,
                };

                if (customer == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Failed getting customer details", "OK");
                    return;
                }

                if (customer == null)
                {
                    await Shell.Current.DisplayAlert("Error", "Failed getting customer details", "OK");
                    return;
                }

                await Shell.Current.GoToAsync($"{nameof(CustomerDetailPage)}", true,
                    new Dictionary<string, object>()
                    {
                { nameof(Customer), new CustomerWrapper(customer) }
                    });
            }
        }
    }
}