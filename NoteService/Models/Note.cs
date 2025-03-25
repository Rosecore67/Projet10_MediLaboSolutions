using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoteService.Models
{
    public class Note
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("PatientId")]
        public int PatientId { get; set; }

        [BsonElement("Content")]
        public string Content { get; set; }

        [BsonElement("Date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
