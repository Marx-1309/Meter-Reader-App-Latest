namespace SampleMauiMvvmApp.Services
{
    public interface IAuthenticationService
    {
        Task<AuthResponseModel> Login(LoginModel loginModel);

        Task SetAuthToken();
    }

    public partial class AuthenticationService : BaseService, IAuthenticationService
    {
        //public static string HOST = ListOfUrl.TnWifi;

        private HttpClient _httpClient;
        public static string BaseAddress = Constants.HOST;

        public AuthenticationService(DbContext dbContext) : base(dbContext)
        {
            _httpClient = new() { BaseAddress = new Uri(BaseAddress) };
        }

        public async Task<AuthResponseModel> Login(LoginModel loginModel)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/login", loginModel);
                response.EnsureSuccessStatusCode();

                #region Save Data of the loggedIn User

                //Save Data of the loggedIn User
                //LoginHistory loggedInUser = new()
                //{
                //    Username = loginModel.Username,
                //    loginDate = DateTime.Now.ToLongDateString(),
                //};
                //await dbContext.Database.InsertAsync(loggedInUser);

                #endregion Save Data of the loggedIn User

                StatusMessage = "Login Successful";

                var responseModel = JsonConvert.DeserializeObject<AuthResponseModel>(await response.Content.ReadAsStringAsync());

                return responseModel;
            }
            catch (Exception ex)
            {
                StatusMessage = "Failed to login successfully.";
                return new AuthResponseModel();
            }
        }

        public async Task SetAuthToken()
        {
            var token = await SecureStorage.GetAsync("Token");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}