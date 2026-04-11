using SampleMauiMvvmApp.Interfaces;

namespace SampleMauiMvvmApp.Services
{
    // https://www.youtube.com/watch?v=XFP8Np-uRWc&ab_channel=JamesMontemagno
    public class CustomerService : BaseService, ICustomerService
    {
        private readonly ReadingService _readingService;
        private HttpClient _httpClient;

        public CustomerService(DbContext dbContext, ReadingService readingService) : base(dbContext)
        {
            _readingService = readingService;
            _httpClient = new HttpClient();
        }

        public async Task<List<Customer>> GetAllCustomers()
        {
            try
            {
                await SetAuthToken();
                //await GetListOfCustomerFromSql();

                var listOfCustomers = await dbContext.Database.Table<Customer>().ToListAsync();
                return listOfCustomers;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
            }
            return null;
        }

        public async Task<Customer> GetCustomerDetails(string customerId)
        {
            try
            {
                var theCustomer = await dbContext.Database.Table<Customer>().FirstOrDefaultAsync(x => x.CUSTNMBR == customerId);
                if (theCustomer != null)
                {
                    var readings = await _readingService.GetReadingsByCustomerId(theCustomer.CUSTNMBR);
                    theCustomer.Readings = readings;
                    return theCustomer;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to retrieve data. {ex.Message}";
            }
            return null;
        }

        public async Task<Customer> GetCustomerByReading(Reading reading)
        {
            try
            {
                return await dbContext.Database.Table<Customer>().Where(r => r.CUSTNMBR == reading.CUSTOMER_NUMBER).FirstOrDefaultAsync();
            }
            catch
            {
                StatusMessage = "No Customer found";
            }
            return null;
        }

        public async Task<Customer> GetCustomerByReadingId(string CustomerIdFromReading)
        {
            var customer = await dbContext.Database.Table<Customer>().FirstOrDefaultAsync(x => x.CUSTNMBR == CustomerIdFromReading);
            return customer;
        }

        //    List<Customer> CustomerList;
        //    public async Task<List<Customer>> GetListOfCustomerFromSql()
        //    {
        //    if (CustomerList == null)
        //    {
        //        try
        //    {
        //        var CustList = await dbContext.Database.Table<Customer>().Where(c => c.CUSTNMBR != null).ToListAsync();

        //        if (CustList.Count == 0)
        //        {
        //            var response = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.GetCustomer);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                // Read the response content as a string
        //                var responseContent = await response.Content.ReadAsStringAsync();

        //                // Deserialize the response content into the List<Customer>
        //                CustomerList = JsonConvert.DeserializeObject<List<Customer>>(responseContent);

        //                            // Insert data into the SQLite database
        //                            await dbContext.Database.InsertAllAsync(CustomerList);

        //                return CustomerList;
        //            }
        //            else
        //            {
        //                // Handle unsuccessful response, maybe throw an exception or log an error
        //                StatusMessage = $"Failed: {response.StatusCode}";
        //            }
        //        }
        //        else
        //        {
        //            // Data already exists in the SQLite database, retrieve it from there.
        //            CustomerList = CustList;
        //                    return CustomerList;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle any other exception that might occur during the API call or database operation
        //        StatusMessage = $"Error: {ex.Message}";
        //    }
        //    }

        //    // Return the CustomerList, even if it's null (client code should handle this)
        //    return CustomerList;
        //}

        private List<Customer> CustomerList;

        public async Task<List<Customer>> GetListOfCustomerFromSql()
        {
            string initialize = null;
            if (initialize == null)
            {
                try
                {
                    string userSite = Preferences.Default.Get("userSite", "");
                    // 1. Retrieve the list of customers & readings from SQL Server and compare to get a list of customers that have existing readings in SQL..
                    var response = await _httpClient.GetAsync(SampleMauiMvvmApp.API_URL_s.Constants.GetCustomer);

                    string baseUrl = SampleMauiMvvmApp.API_URL_s.Constants.GetReading; // e.g., "https://localhost:7231/api/Reading"
                    string requestUrl = $"{baseUrl}?billingSite={Uri.EscapeDataString(userSite)}";

                    var response2 = await _httpClient.GetAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var responseContent2 = await response2.Content.ReadAsStringAsync();

                        var sqlServerCustomerList = JsonConvert.DeserializeObject<List<Customer>>(responseContent);
                        var sqlServerReadingsList = JsonConvert.DeserializeObject<List<Reading>>(responseContent2);

                        var matchingCustomerIDs = sqlServerCustomerList
                            .Where(customer => sqlServerReadingsList.Any(reading => reading.CUSTOMER_NUMBER == customer.CUSTNMBR))
                            .Select(customer => customer.CUSTNMBR)
                            .ToList();

                        var newCustomersToInsert = sqlServerCustomerList
                            .Where(customer => matchingCustomerIDs.Contains(customer.CUSTNMBR))
                            .ToList();

                        var sqliteCustomerList = await dbContext.Database.Table<Customer>().Where(c => c.CUSTNMBR != null).ToListAsync();

                        var newCustomersNotInSQLite = newCustomersToInsert
                            .Where(customer => !sqliteCustomerList.Any(sqliteCustomer => sqliteCustomer.CUSTNMBR == customer.CUSTNMBR))
                            .ToList();

                        if (newCustomersNotInSQLite.Count > 0)
                        {
                            await dbContext.Database.InsertAllAsync(newCustomersNotInSQLite);
                            //await _readingService.GetListOfPrevMonthReadingFromSql();
                        }
                        else { return null; }

                        CustomerList = sqliteCustomerList.Concat(newCustomersNotInSQLite).ToList();

                        return CustomerList;
                    }
                    else
                    {
                        StatusMessage = $"Failed: {response.StatusCode}";
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                }
            }
            return CustomerList;
        }

        public async Task SetAuthToken()
        {
            var token = await SecureStorage.GetAsync("Token");
            _httpClient.DefaultRequestHeaders.Authorization = new
                System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}