using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NoteService.Models;
using NoteService.Repositories.Interface;
using NoteService.Settings;

namespace NoteService.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly IMongoCollection<Note> _notesCollection;

        public NoteRepository(IOptions<NoteDatabaseSettings> noteDbSettings)
        {
            var mongoClient = new MongoClient(noteDbSettings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(noteDbSettings.Value.DatabaseName);
            _notesCollection = database.GetCollection<Note>(noteDbSettings.Value.NotesCollectionName);
        }

        public async Task<IEnumerable<Note>> GetAllAsync()
            => await _notesCollection.Find(_ => true).ToListAsync();

        public async Task<Note?> GetByIdAsync(string id)
            => await _notesCollection.Find(n => n.Id == id).FirstOrDefaultAsync();

        public async Task<IEnumerable<Note>> GetByPatientIdAsync(int patientId)
            => await _notesCollection.Find(n => n.PatientId == patientId).ToListAsync();

        public async Task CreateAsync(Note note)
            => await _notesCollection.InsertOneAsync(note);

        public async Task<bool> UpdateAsync(string id, Note note)
        {
            var result = await _notesCollection.ReplaceOneAsync(n => n.Id == id, note);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _notesCollection.DeleteOneAsync(n => n.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
