using MicroFrontEnd.Models;
using MicroFrontEnd.Models.DTOs;
using MicroFrontEnd.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MicroFrontEnd.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _accessor;
        private readonly string _apiUrl = "https://localhost:6000/api/patients";

        public PatientController(IHttpClientFactory httpClientFactory, IHttpContextAccessor accessor)
        {
            _httpClientFactory = httpClientFactory;
            _accessor = accessor;
        }

        public async Task<IActionResult> Index()
        {


            var client = _httpClientFactory.CreateClient();
            client.AddJwtFromSession(_accessor);

            var response = await client.GetAsync(_apiUrl);
            if (!response.IsSuccessStatusCode)
            {
                return View(new List<PatientViewModel>());
            }

            var data = await response.Content.ReadAsStringAsync();

            var patients = JsonSerializer.Deserialize<List<PatientViewModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Console.WriteLine("🎯 Appel de Index");
            Console.WriteLine("Session JWT dans controller : " + (_accessor.HttpContext?.Session.GetString("JwtToken") ?? "vide"));
            return View(patients);
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient();
            client.AddJwtFromSession(_accessor);

            var response = await client.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var data = await response.Content.ReadAsStringAsync();
            var patient = JsonSerializer.Deserialize<PatientDetailsViewModel>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Get notes
            var notesUrl = $"https://localhost:6000/api/notes/{id}";
            var notesResponse = await client.GetAsync(notesUrl);

            if (notesResponse.IsSuccessStatusCode)
            {
                var notesContent = await notesResponse.Content.ReadAsStringAsync();
                var notes = JsonSerializer.Deserialize<List<NoteDTO>>(notesContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                patient.Notes = notes;
            }
            else
            {
                patient.Notes = new List<NoteDTO>();
            }

            // Get diabetes risk
            var riskUrl = $"https://localhost:6000/api/diabetescheck/{id}";
            var riskResponse = await client.GetAsync(riskUrl);
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

        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient();
            client.AddJwtFromSession(_accessor);

            var response = await client.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var data = await response.Content.ReadAsStringAsync();
            var patient = JsonSerializer.Deserialize<PatientEditViewModel>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PatientEditViewModel model)
        {
            var client = _httpClientFactory.CreateClient();
            client.AddJwtFromSession(_accessor);

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{_apiUrl}/update/{model.Id}", content);
            if (!response.IsSuccessStatusCode) return View(model);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient();
            client.AddJwtFromSession(_accessor);

            var response = await client.DeleteAsync($"{_apiUrl}/delete/{id}");
            return RedirectToAction("Index");
        }
    }
}
