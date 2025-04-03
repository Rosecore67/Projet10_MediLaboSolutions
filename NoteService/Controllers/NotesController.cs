using Microsoft.AspNetCore.Mvc;
using NoteService.Models.DTOs;
using NoteService.Services.Interface;

namespace NoteService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController(INoteService noteService) : ControllerBase
    {
        private readonly INoteService _noteService = noteService;

        // GET: api/notes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NoteReadDTO>>> GetAllNotes()
        {
            var notes = await _noteService.GetAllNotesAsync();
            return Ok(notes);
        }

        // GET: api/notes/{patientId}
        [HttpGet("{patientId}")]
        public async Task<ActionResult<IEnumerable<NoteReadDTO>>> GetNotesByPatient(int patientId)
        {
            var notes = await _noteService.GetNotesByPatientIdAsync(patientId);
            return Ok(notes);
        }

        // GET: api/notes/note/{id}
        [HttpGet("note/{id}")]
        public async Task<ActionResult<NoteReadDTO>> GetNoteById(string id)
        {
            var note = await _noteService.GetNoteByIdAsync(id);
            if (note == null) return NotFound();
            return Ok(note);
        }

        // POST: api/notes
        [HttpPost]
        public async Task<ActionResult<NoteReadDTO>> CreateNote([FromBody] NoteCreateDTO dto)
        {
            var newNote = await _noteService.CreateNoteAsync(dto);
            return CreatedAtAction(nameof(GetNotesByPatient), new { patientId = newNote.PatientId }, newNote);
        }

        // PUT: api/notes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(string id, [FromBody] NoteCreateDTO dto)
        {
            var updated = await _noteService.UpdateNoteAsync(id, dto);
            if (!updated) return NotFound();
            return NoContent();
        }
    }
}
