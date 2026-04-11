namespace SampleMauiMvvmApp.Interfaces
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllCustomers();

        Task<Customer> GetCustomerByReading(Reading reading);

        Task<Customer> GetCustomerByReadingId(string CustomerIdFromReading);

        Task<Customer> GetCustomerDetails(string customerId);

        Task<List<Customer>> GetListOfCustomerFromSql();

        Task SetAuthToken();
    }
}