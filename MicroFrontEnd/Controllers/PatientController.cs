using MicroFrontEnd.Models;
using MicroFrontEnd.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MicroFrontEnd.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _accessor;
        private readonly string _apiUrl = "http://localhost:6000/api/patients";

        public PatientController(IHttpClientFactory httpClientFactory, IHttpContextAccessor accessor)
        {
            _httpClientFactory = httpClientFactory;
            _accessor = accessor;
        }

        // 👇 Injecte automatiquement le token depuis la session dans le header
        private HttpClient CreateClientWithJwt()
        {
            var client = _httpClientFactory.CreateClient("LoggedClient");
            var token = _accessor.HttpContext?.Session.GetString("JwtToken");

            Console.WriteLine($"[CONTROLLER] Token récupéré : {(token?.Substring(0, 15) ?? "AUCUN")}...");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<IActionResult> Index()
        {
            Console.WriteLine("🔍 [PatientController] Index appelé !");
            var client = CreateClientWithJwt();

            var response = await client.GetAsync(_apiUrl);
            if (!response.IsSuccessStatusCode)
                return View(new List<PatientViewModel>());

            var data = await response.Content.ReadAsStringAsync();
            var patients = JsonSerializer.Deserialize<List<PatientViewModel>>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(patients);
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = CreateClientWithJwt();

            var response = await client.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var data = await response.Content.ReadAsStringAsync();
            var patient = JsonSerializer.Deserialize<PatientDetailsViewModel>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Notes
            var notesResponse = await client.GetAsync($"http://localhost:6000/api/notes/{id}");
            if (notesResponse.IsSuccessStatusCode)
            {
                var notesContent = await notesResponse.Content.ReadAsStringAsync();
                patient.Notes = JsonSerializer.Deserialize<List<NoteDTO>>(notesContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                patient.Notes = new List<NoteDTO>();
            }

            // Risque
            var riskResponse = await client.GetAsync($"http://localhost:6000/api/diabetescheck/{id}");
            if (riskResponse.IsSuccessStatusCode)
            {
                var riskData = await riskResponse.Content.ReadAsStringAsync();
                patient.NiveauRisque = JsonSerializer.Deserialize<string>(riskData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                patient.NiveauRisque = "Non évalué";
            }

            return View(patient);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateClientWithJwt();

            var response = await client.GetAsync($"{_apiUrl}/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var data = await response.Content.ReadAsStringAsync();
            var patient = JsonSerializer.Deserialize<PatientEditViewModel>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(patient);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PatientEditViewModel model)
        {
            var client = CreateClientWithJwt();

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{_apiUrl}/update/{model.Id}", content);
            if (!response.IsSuccessStatusCode) return View(model);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = CreateClientWithJwt();

            await client.DeleteAsync($"{_apiUrl}/delete/{id}");
            return RedirectToAction("Index");
        }
    }
}
