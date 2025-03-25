using NoteService.Models.DTOs;

namespace NoteService.Services.Interface
{
    public interface INoteService
    {
        Task<IEnumerable<NoteReadDTO>> GetAllNotesAsync();
        Task<NoteReadDTO?> GetNoteByIdAsync(string id);
        Task<IEnumerable<NoteReadDTO>> GetNotesByPatientIdAsync(int patientId);
        Task<NoteReadDTO> CreateNoteAsync(NoteCreateDTO noteDto);
        Task<bool> UpdateNoteAsync(string id, NoteCreateDTO noteDto);
    }
}
