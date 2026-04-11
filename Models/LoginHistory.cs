namespace SampleMauiMvvmApp.Models
{
    [Table("LoginHistory")]
    public class LoginHistory
    {
        [PrimaryKey, AutoIncrement]
        public int? LoginId { get; set; }

        public string? Username { get; set; }
        public string? loginDate { get; set; }
    }
}