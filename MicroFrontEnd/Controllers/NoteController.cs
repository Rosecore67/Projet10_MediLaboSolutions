using MicroFrontEnd.Models;
using MicroFrontEnd.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MicroFrontEnd.Controllers
{
    public class NoteController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _noteApiUrl = "http://localhost:6000/api/notes";
        private readonly string _patientApiUrl = "http://localhost:6000/api/patients";

        public NoteController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Affiche toutes les notes avec les noms des patients
        public async Task<IActionResult> Index()
        {
            var noteResponse = await _httpClient.GetAsync($"{_noteApiUrl}");
            if (!noteResponse.IsSuccessStatusCode) return View(new List<(NoteDTO, string)>());

            var noteData = await noteResponse.Content.ReadAsStringAsync();
            var notes = JsonSerializer.Deserialize<List<NoteDTO>>(noteData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var result = new List<(NoteDTO, string)>();

            foreach (var note in notes)
            {
                var patientResponse = await _httpClient.GetAsync($"{_patientApiUrl}/{note.PatientId}");
                var patientName = "Inconnu";

                if (patientResponse.IsSuccessStatusCode)
                {
                    var patientJson = await patientResponse.Content.ReadAsStringAsync();
                    var patient = JsonSerializer.Deserialize<PatientViewModel>(patientJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    patientName = $"{patient.Prenom} {patient.Nom}";
                }

                result.Add((note, patientName));
            }

            return View(result);
        }

        // Formulaire de création
        public IActionResult Create(int patientId)
        {
            return View(new NoteCreateViewModel { PatientId = patientId });
        }

        // Traitement du POST
        [HttpPost]
        public async Task<IActionResult> Create(NoteCreateViewModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_noteApiUrl}", content);
            if (!response.IsSuccessStatusCode)
                return View(model);

            return RedirectToAction("Index");
        }
    }
}
