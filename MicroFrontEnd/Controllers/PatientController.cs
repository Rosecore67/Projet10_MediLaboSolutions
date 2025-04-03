using MicroFrontEnd.Models;
using MicroFrontEnd.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MicroFrontEnd.Controllers
{
    public class PatientController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "http://localhost:6000/api/patients"; // a modifier pour OCELOT

        public PatientController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            Console.WriteLine($"Appel API: {_apiUrl}");

            var response = await _httpClient.GetAsync(_apiUrl);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erreur API: {response.StatusCode}");
                return View(new List<PatientViewModel>());
            }

            var data = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Données reçues: {data}");

            var patients = JsonSerializer.Deserialize<List<PatientViewModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(patients);
        }

        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var data = await response.Content.ReadAsStringAsync();
            var patient = JsonSerializer.Deserialize<PatientDetailsViewModel>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var notesResponse = await _httpClient.GetAsync($"http://localhost:6000/api/notes/{id}");

            if (notesResponse.IsSuccessStatusCode)
            {
                var notesContent = await notesResponse.Content.ReadAsStringAsync();
                var notes = JsonSerializer.Deserialize<List<NoteDTO>>(notesContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                patient.Notes = notes;
            }
            else
            {
                patient.Notes = new List<NoteDTO>(); // Pour éviter un null
            }

            var riskResponse = await _httpClient.GetAsync($"http://localhost:6000/api/DiabetesCheck/{id}");
            if (riskResponse.IsSuccessStatusCode)
            {
                var riskData = await riskResponse.Content.ReadAsStringAsync();
                patient.NiveauRisque = JsonSerializer.Deserialize<string>(riskData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                patient.NiveauRisque = "Non évalué";
            }

            return View(patient);
        }

        // Permet de récupérer les patients pour une MAJ
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var data = await response.Content.ReadAsStringAsync();
            var patient = JsonSerializer.Deserialize<PatientEditViewModel>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PatientEditViewModel model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_apiUrl}/{model.Id}", content);
            if (!response.IsSuccessStatusCode) return View(model);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiUrl}/{id}");
            return RedirectToAction("Index");
        }
    }
}
