namespace SampleMauiMvvmApp.Models
{
    [Table("User")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public bool Active { get; set; }
        //[OneToMany]
        //public List<Device> Devices { get; set; }
    }
}