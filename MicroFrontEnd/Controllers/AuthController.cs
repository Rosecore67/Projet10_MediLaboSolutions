using MicroFrontEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MicroFrontEnd.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _authUrl;

        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _authUrl = configuration["ApiUrls:Auth"] ?? throw new ArgumentNullException("ApiUrls:Auth is not configured");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(new { model.Username, model.Password }),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync($"{_authUrl}/login", jsonContent);

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
                    await HttpContext.Session.CommitAsync();
                }
                else
                {
                    Console.WriteLine("[AUTH] Token NULL ou vide !");
                    model.ErrorMessage = "Erreur lors de la récupération du token.";
                    return View(model);
                }

                return RedirectToAction("Index", "Patient");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AUTH ERROR] {ex.Message}");
                model.ErrorMessage = "Une erreur est survenue lors de la connexion.";
                return View(model);
            }
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
