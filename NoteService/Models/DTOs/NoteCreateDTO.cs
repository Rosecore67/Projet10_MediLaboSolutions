namespace NoteService.Models.DTOs
{
    public class NoteCreateDTO
    {
        public int PatientId { get; set; }
        public string Content { get; set; }
    }
}
