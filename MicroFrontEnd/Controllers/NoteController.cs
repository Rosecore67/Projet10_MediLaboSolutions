using MicroFrontEnd.Models;
using MicroFrontEnd.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MicroFrontEnd.Controllers
{
    public class NoteController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _accessor;
        private readonly string _noteApiUrl;
        private readonly string _patientApiUrl;

        public NoteController(IHttpClientFactory httpClientFactory, IHttpContextAccessor accessor, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _accessor = accessor;
            _noteApiUrl = config["ApiUrls:Notes"]!;
            _patientApiUrl = config["ApiUrls:Patients"]!;
        }

        private HttpClient CreateClientWithJwt()
        {
            var client = _httpClientFactory.CreateClient("LoggedClient");
            var token = _accessor.HttpContext?.Session.GetString("JwtToken");

            Console.WriteLine($"[NOTE] Token récupéré : {(token?.Substring(0, 15) ?? "AUCUN")}...");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<IActionResult> Index()
        {
            var client = CreateClientWithJwt();

            var noteResponse = await client.GetAsync(_noteApiUrl);
            if (!noteResponse.IsSuccessStatusCode)
                return View(new List<(NoteDTO, string)>());

            var noteData = await noteResponse.Content.ReadAsStringAsync();
            var notes = JsonSerializer.Deserialize<List<NoteDTO>>(noteData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var result = new List<(NoteDTO, string)>();

            foreach (var note in notes)
            {
                var patientClient = CreateClientWithJwt();
                var patientResponse = await patientClient.GetAsync($"{_patientApiUrl}/{note.PatientId}");
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

        [HttpPost]
        public async Task<IActionResult> Create(NoteCreateViewModel model)
        {
            var client = CreateClientWithJwt();

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_noteApiUrl, content);
            if (!response.IsSuccessStatusCode)
                return View(model);

            return RedirectToAction("Index");
        }
    }
}
