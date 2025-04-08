using MicroFrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MicroFrontEnd.Controllers
{
    public class AuthController(IHttpClientFactory httpClientFactory) : Controller
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(new { model.Username, model.Password }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("http://localhost:6000/api/auth/login", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                model.ErrorMessage = "Identifiants incorrects.";
                return View(model);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseString);
            var token = jsonDoc.RootElement.GetProperty("token").GetString();

            if (!string.IsNullOrEmpty(token))
            {
                HttpContext.Session.SetString("JwtToken", token);
                Console.WriteLine($"[AUTH] Token mis en session : {token}");
            }
            else
            {
                Console.WriteLine("[AUTH] Token NULL ou vide !");
            }

            return RedirectToAction("Index", "Patient");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("JwtToken");
            return RedirectToAction("Login", "Auth");
        }
    }
}
