using NoteService.Models;

namespace NoteService.Repositories.Interface
{
    public interface INoteRepository
    {
        Task<IEnumerable<Note>> GetAllAsync();
        Task<Note?> GetByIdAsync(string id);
        Task<IEnumerable<Note>> GetByPatientIdAsync(int patientId);
        Task CreateAsync(Note note);
        Task<bool> UpdateAsync(string id, Note note);
        Task<bool> DeleteAsync(string id);
    }
}
