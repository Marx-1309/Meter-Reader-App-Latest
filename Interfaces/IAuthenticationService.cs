namespace SampleMauiMvvmApp.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthResponseModel> Login(LoginModel loginModel);

        Task SetAuthToken();
    }
}