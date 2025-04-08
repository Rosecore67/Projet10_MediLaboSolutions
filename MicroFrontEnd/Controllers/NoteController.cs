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
    public class NoteController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _accessor;
        private readonly string _noteApiUrl = "http://localhost:6000/api/notes";
        private readonly string _patientApiUrl = "http://localhost:6000/api/patients";

        public NoteController(IHttpClientFactory httpClientFactory, IHttpContextAccessor accessor)
        {
            _httpClientFactory = httpClientFactory;
            _accessor = accessor;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            client.AddJwtFromSession(_accessor);

            var noteResponse = await client.GetAsync(_noteApiUrl);
            if (!noteResponse.IsSuccessStatusCode)
                return View(new List<(NoteDTO, string)>());

            var noteData = await noteResponse.Content.ReadAsStringAsync();
            var notes = JsonSerializer.Deserialize<List<NoteDTO>>(noteData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var result = new List<(NoteDTO, string)>();

            foreach (var note in notes)
            {
                var patientClient = _httpClientFactory.CreateClient();
                patientClient.AddJwtFromSession(_accessor);

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

        public IActionResult Create(int patientId)
        {
            return View(new NoteCreateViewModel { PatientId = patientId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(NoteCreateViewModel model)
        {
            var client = _httpClientFactory.CreateClient();
            client.AddJwtFromSession(_accessor);

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_noteApiUrl, content);
            if (!response.IsSuccessStatusCode)
                return View(model);

            return RedirectToAction("Index");
        }
    }
}
