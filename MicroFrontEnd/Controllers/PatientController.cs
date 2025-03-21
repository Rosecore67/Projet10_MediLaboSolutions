using MicroFrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
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
            var response = await _httpClient.GetAsync(_apiUrl);
            if (!response.IsSuccessStatusCode)
            {
                return View(new List<PatientViewModel>());
            }

            var data = await response.Content.ReadAsStringAsync();
            var patients = JsonSerializer.Deserialize<List<PatientViewModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(patients);
        }
    }
}
