using BackPatient.Models;
using BackPatient.Models.DTOs;
using BackPatient.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BackPatient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _service;

        public PatientsController(IPatientService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patient>>> GetPatients()
        {
            return Ok(await _service.GetAllPatients());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Patient>> GetPatient(int id)
        {
            var patient = await _service.GetPatientById(id);
            if (patient == null) return NotFound();
            return Ok(patient);
        }

        [HttpPost("add")]
        public async Task<ActionResult<Patient>> PostPatient(PatientCreateDto patientDto)
        {
            var patient = new Patient
            {
                Nom = patientDto.Nom,
                Prenom = patientDto.Prenom,
                DateNaissance = patientDto.DateNaissance,
                Genre = patientDto.Genre,
                Adresse = patientDto.Adresse,
                Telephone = patientDto.Telephone
            };

            await _service.AddPatient(patient);

            return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> PutPatient(int id, PatientUpdateDto patientDto)
        {
            var existingPatient = await _service.GetPatientById(id);
            if (existingPatient == null)
            {
                return NotFound("Le patient n'existe pas.");
            }

            existingPatient.Nom = patientDto.Nom;
            existingPatient.Prenom = patientDto.Prenom;
            existingPatient.DateNaissance = patientDto.DateNaissance;
            existingPatient.Genre = patientDto.Genre;
            existingPatient.Adresse = patientDto.Adresse;
            existingPatient.Telephone = patientDto.Telephone;

            await _service.UpdatePatient(existingPatient);

            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            await _service.DeletePatient(id);
            return NoContent();
        }
    }
}
