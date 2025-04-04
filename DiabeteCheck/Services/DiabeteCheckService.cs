using DiabeteCheck.Models;
using DiabeteCheck.Models.DTOs;
using DiabeteCheck.Services.Interfaces;
using DiabeteCheck.Utils;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DiabeteCheck.Services
{
    public class DiabeteCheckService(HttpClient httpClient, IConfiguration configuration) : IDiabeteCheckService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IConfiguration _configuration = configuration;

        public async Task<RiskEvaluation> ControlRiskAsync(int patientId)
        {
            // Valeur par défaut
            RiskEvaluation riskLevel = RiskEvaluation.None;

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

            // Fusionner tout le contenu des notes
            string allContent = string.Join(" ", notes
                .Where(n => !string.IsNullOrWhiteSpace(n.Contenu))
                .Select(n => n.Contenu.ToLowerInvariant()));

            // Liste des déclencheurs uniques détectés
            var detectedTriggers = new HashSet<string>();

            // Détection des termes (chaque racine ne compte qu'une seule fois)
            foreach (var kvp in TriggerTerms.TermsWithVariants)
            {
                foreach (var variant in kvp.Value)
                {
                    var pattern = $@"\b{Regex.Escape(variant)}\b";
                    if (Regex.IsMatch(allContent, pattern))
                    {
                        detectedTriggers.Add(kvp.Key);
                        break; // On arrête dès qu'une variante a matché
                    }
                }
            }

            int triggerCount = detectedTriggers.Count;

            // Calcul de l'âge
            int age = DateTime.Today.Year - patient.DateNaissance.Year;
            if (patient.DateNaissance > DateTime.Today.AddYears(-age)) age--;

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
    }
}
