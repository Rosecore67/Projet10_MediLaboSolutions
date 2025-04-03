using DiabeteCheck.Models;
using DiabeteCheck.Models.DTOs;
using DiabeteCheck.Services.Interfaces;
using DiabeteCheck.Utils;
using System.Text.Json;

namespace DiabeteCheck.Services
{
    public class DiabeteCheckService : IDiabeteCheckService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public DiabeteCheckService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<RiskEvaluation> ControlRiskAsync(int patientId)
        {
            // URLs des autres microservices
            var baseUrl = _configuration["GatewayUrl"];
            var patientUrl = $"{baseUrl}/api/patients/{patientId}";
            var notesUrl = $"{baseUrl}/api/notes/{patientId}";

            // Récupérer le patient
            var patientResponse = await _httpClient.GetAsync(patientUrl);
            if (!patientResponse.IsSuccessStatusCode)
                throw new Exception("Patient non trouvé.");

            var patientJson = await patientResponse.Content.ReadAsStringAsync();
            var patient = JsonSerializer.Deserialize<PatientDTO>(patientJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Récupérer les notes
            var notesResponse = await _httpClient.GetAsync(notesUrl);
            if (!notesResponse.IsSuccessStatusCode)
                throw new Exception("Aucune note trouvée pour ce patient.");

            var notesJson = await notesResponse.Content.ReadAsStringAsync();
            var notes = JsonSerializer.Deserialize<List<NoteDTO>>(notesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Compter les déclencheurs
            int triggerCount = 0;
            foreach (var note in notes)
            {
                foreach (var trigger in TriggerTerms.Terms)
                {
                    if (note.Contenu != null && note.Contenu.Contains(trigger, StringComparison.OrdinalIgnoreCase))
                    {
                        triggerCount++;
                    }
                }
            }

            // Calcul de l'âge
            int age = DateTime.Today.Year - patient.DateNaissance.Year;
            if (patient.DateNaissance > DateTime.Today.AddYears(-age)) age--;

            // Règles d’évaluation
            if (triggerCount == 0)
                return RiskEvaluation.None;

            if (triggerCount == 2 && age > 30)
                return RiskEvaluation.Borderline;

            if ((patient.Genre == "M" && age < 30 && triggerCount >= 3) ||
                (patient.Genre == "F" && age < 30 && triggerCount >= 4))
                return RiskEvaluation.InDanger;

            if ((age > 30 && triggerCount >= 6) ||
                (patient.Genre == "M" && age < 30 && triggerCount >= 5) ||
                (patient.Genre == "F" && age < 30 && triggerCount >= 7))
                return RiskEvaluation.EarlyOnset;

            return RiskEvaluation.None;
        }
    }
}
