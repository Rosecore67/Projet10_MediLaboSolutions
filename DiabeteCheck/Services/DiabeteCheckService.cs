using DiabeteCheck.Models;
using DiabeteCheck.Models.DTOs;
using DiabeteCheck.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DiabeteCheck.Services
{
    public class DiabeteCheckService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : IDiabeteCheckService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IConfiguration _configuration = configuration;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly string _baseUrl = configuration["GatewayUrl"];

        public async Task<RiskEvaluation> ControlRiskAsync(int patientId)
        {
            var patient = await GetPatientAsync(patientId);
            var notes = await GetNotesAsync(patientId);

            var triggerCount = CalculateRisk(notes);

            // Calcul de l'âge
            int age = DateTime.Today.Year - patient.DateNaissance.Year;

            return EvaluateRisk(triggerCount, age, patient);
        }

        private void AttachToken()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                var token = authHeader.Replace("Bearer ", "");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private async Task<PatientDTO> GetPatientAsync(int patientId)
        {
            AttachToken();

            var patientUrl = $"{_baseUrl}/api/patients/{patientId}";
            var patientResponse = await _httpClient.GetAsync(patientUrl);
            if (!patientResponse.IsSuccessStatusCode)
                throw new Exception("Patient non trouvé.");

            var patientJson = await patientResponse.Content.ReadAsStringAsync();
            var patient = JsonSerializer.Deserialize<PatientDTO>(patientJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new Exception("Erreur lors de la désérialisation du patient.");

            return patient;
        }

        private async Task<List<NoteDTO>> GetNotesAsync(int patientId)
        {
            AttachToken();

            var notesUrl = $"{_baseUrl}/api/notes/{patientId}";
            var notesResponse = await _httpClient.GetAsync(notesUrl);
            if (!notesResponse.IsSuccessStatusCode)
                throw new Exception("Aucune note trouvée pour ce patient.");

            var notesJson = await notesResponse.Content.ReadAsStringAsync();
            var notes = JsonSerializer.Deserialize<List<NoteDTO>>(notesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new Exception("Erreur lors de la désérialisation des notes.");

            return notes;
        }

        private int CalculateRisk(List<NoteDTO> notes)
        {
            int triggerCount = 0;
            if (notes.Count == 0)
                return triggerCount;

            List<Regex> facteursDeclencheurs = new()
            {
                new Regex(@"H[éèe]moglobine\s*A1C", RegexOptions.IgnoreCase),
                new Regex(@"Microalbumine", RegexOptions.IgnoreCase),
                new Regex(@"Taille", RegexOptions.IgnoreCase),
                new Regex(@"Poids", RegexOptions.IgnoreCase),
                new Regex(@"Fumeur", RegexOptions.IgnoreCase),
                new Regex(@"Anormal", RegexOptions.IgnoreCase),
                new Regex(@"Cholest[ée]rol", RegexOptions.IgnoreCase),
                new Regex(@"Vertiges?", RegexOptions.IgnoreCase),
                new Regex(@"Rechute", RegexOptions.IgnoreCase),
                new Regex(@"R[ée]action", RegexOptions.IgnoreCase),
                new Regex(@"Anticorps", RegexOptions.IgnoreCase)
            };

            foreach (var reg in facteursDeclencheurs)
            {
                if (notes.Any(note => reg.IsMatch(note.Contenu)))
                {
                    triggerCount++;
                }
            }
            return triggerCount;
        }

        private RiskEvaluation EvaluateRisk(int triggerCount, int age, PatientDTO patient)
        {
            if (triggerCount == 0)
                return RiskEvaluation.None;

            if ((patient.Genre == "M" && age < 30 && triggerCount >= 5) ||
                (patient.Genre == "F" && age < 30 && triggerCount >= 7) ||
                (age > 30 && triggerCount >= 8))
                return RiskEvaluation.EarlyOnset;

            if ((patient.Genre == "M" && age < 30 && triggerCount is >= 3 and <= 4) ||
                (patient.Genre == "F" && age < 30 && triggerCount is >= 4 and <= 6) ||
                (age > 30 && triggerCount is 6 or 7))
                return RiskEvaluation.InDanger;

            if (triggerCount is >= 2 and <= 5 && age > 30)
                return RiskEvaluation.Borderline;

            return RiskEvaluation.None;
        }
    }
}