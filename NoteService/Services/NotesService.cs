using NoteService.Models;
using NoteService.Models.DTOs;
using NoteService.Repositories.Interface;
using NoteService.Services.Interface;

namespace NoteService.Services
{
    public class NotesService : INoteService
    {
        private readonly INoteRepository _repository;

        public NotesService(INoteRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<NoteReadDTO>> GetAllNotesAsync()
        {
            var notes = await _repository.GetAllAsync();
            return notes.Select(n => new NoteReadDTO
            {
                Id = n.Id,
                PatientId = n.PatientId,
                Content = n.Content,
                Date = n.Date
            });
        }

        public async Task<NoteReadDTO?> GetNoteByIdAsync(string id)
        {
            var note = await _repository.GetByIdAsync(id);
            if (note == null) return null;

            return new NoteReadDTO
            {
                Id = note.Id,
                PatientId = note.PatientId,
                Content = note.Content,
                Date = note.Date
            };
        }

        public async Task<IEnumerable<NoteReadDTO>> GetNotesByPatientIdAsync(int patientId)
        {
            var notes = await _repository.GetByPatientIdAsync(patientId);
            return notes.Select(n => new NoteReadDTO
            {
                Id = n.Id,
                PatientId = n.PatientId,
                Content = n.Content,
                Date = n.Date
            });
        }

        public async Task<NoteReadDTO> CreateNoteAsync(NoteCreateDTO noteDto)
        {
            var newNote = new Note
            {
                PatientId = noteDto.PatientId,
                Content = noteDto.Content,
                Date = DateTime.UtcNow
            };

            await _repository.CreateAsync(newNote);

            return new NoteReadDTO
            {
                Id = newNote.Id,
                PatientId = newNote.PatientId,
                Content = newNote.Content,
                Date = newNote.Date
            };
        }

        public async Task<bool> UpdateNoteAsync(string id, NoteCreateDTO noteDto)
        {
            var updatedNote = new Note
            {
                Id = id,
                PatientId = noteDto.PatientId,
                Content = noteDto.Content,
                Date = DateTime.UtcNow
            };

            return await _repository.UpdateAsync(id, updatedNote);
        }
    }
}
