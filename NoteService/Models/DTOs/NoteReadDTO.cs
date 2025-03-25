namespace NoteService.Models.DTOs
{
    public class NoteReadDTO
    {
        public string Id { get; set; }
        public int PatientId { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
    }
}
