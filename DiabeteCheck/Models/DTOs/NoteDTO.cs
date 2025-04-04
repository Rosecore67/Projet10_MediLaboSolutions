using System.Text.Json.Serialization;

namespace DiabeteCheck.Models.DTOs
{
    public class NoteDTO
    {
        public string Id { get; set; }
        public int PatientId { get; set; }

        [JsonPropertyName("content")]
        public string Contenu { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }
}
