namespace MicroFrontEnd.Models.DTOs
{
    public class NoteDTO
    {
        public string Id { get; set; }
        public int PatientId { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
    }
}
