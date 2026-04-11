using System.ComponentModel.DataAnnotations.Schema;

namespace SampleMauiMvvmApp.Models
{
    public class Notes
    {
        [PrimaryKey, AutoIncrement]
        public int NoteID { get; set; }

        public string NoteTitle { get; set; }
        public string NoteContent { get; set; }
        public string Image { get; set; }
        public string Date { get; set; }
    }
}