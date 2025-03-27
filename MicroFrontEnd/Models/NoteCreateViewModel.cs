namespace MicroFrontEnd.Models
{
    public class NoteCreateViewModel
    {
        public int PatientId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
