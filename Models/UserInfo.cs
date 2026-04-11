using System.ComponentModel.DataAnnotations.Schema;

namespace SampleMauiMvvmApp.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("UserInfo")]
    public class UserInfo
    {
        //Id is optional , i added just incase i want to use from the Json token
        [NotMapped]
        public string Id { get; set; }

        public string Username { get; set; }
        public string Role { get; set; }
    }
}