using DiabeteCheck.Models;
using DiabeteCheck.Models.DTOs;
using DiabeteCheck.Services.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DiabeteCheck.Services
{
    public class DiabeteCheckService(HttpClient httpClient, IConfiguration configuration) : IDiabeteCheckService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IConfiguration _configuration = configuration;
        private readonly string _baseUrl = configuration["GatewayUrl"];

        public async Task<RiskEvaluation> ControlRiskAsync(int patientId)
        {
            var patient = await GetPatientAsync(patientId);
            var notes = await GetNotesAsync(patientId);

            var triggerCount = CalculateRisk(notes);

            // Calcul de l'âge
            int age = DateTime.Today.Year - patient.DateNaissance.Year;

            // Valeur par défaut
            RiskEvaluation riskLevel = EvaluateRisk(triggerCount, age, patient);
            return riskLevel;
        }

        private int CalculateRisk(List<NoteDTO> notes)
        {
            int triggerCount = 0;
            if (notes.Count == 0)
                return triggerCount;

            List<Regex> facteursDeclencheurs =
[
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
];

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
            RiskEvaluation riskLevel = RiskEvaluation.None;

            if (triggerCount == 0)
            {
                riskLevel = RiskEvaluation.None;
            }
            else if (
                (patient.Genre == "M" && age < 30 && triggerCount >= 5) ||
                (patient.Genre == "F" && age < 30 && triggerCount >= 7) ||
                (age > 30 && triggerCount >= 8)
            )
            {
                riskLevel = RiskEvaluation.EarlyOnset;
            }
            else if (
                (patient.Genre == "M" && age < 30 && triggerCount >= 3 && triggerCount <= 4) ||
                (patient.Genre == "F" && age < 30 && triggerCount >= 4 && triggerCount <= 6) ||
                (age > 30 && (triggerCount == 6 || triggerCount == 7))
            )
            {
                riskLevel = RiskEvaluation.InDanger;
            }
            else if (triggerCount >= 2 && triggerCount <= 5 && age > 30)
            {
                riskLevel = RiskEvaluation.Borderline;
            }
            return riskLevel;
        }

        private async Task<PatientDTO> GetPatientAsync(int patientId)
        {
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
            var notesUrl = $"{_baseUrl}/api/notes/{patientId}";
            var notesResponse = await _httpClient.GetAsync(notesUrl);
            if (!notesResponse.IsSuccessStatusCode)
                throw new Exception("Aucune note trouvée pour ce patient.");

            var notesJson = await notesResponse.Content.ReadAsStringAsync();
            var notes = JsonSerializer.Deserialize<List<NoteDTO>>(notesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new Exception("Erreur lors de la désérialisation des notes.");

            return notes;
        }
    }
}