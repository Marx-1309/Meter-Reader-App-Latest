namespace SampleMauiMvvmApp.Models
{
    [Table("LoginModel")]
    public class LoginModel
    {
        public LoginModel(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; set; }
        public string Password { get; set; }
    }
}

[JsonSerializable(typeof(List<LoginModel>))]
internal sealed partial class LoginModelContext : JsonSerializerContext
{
}