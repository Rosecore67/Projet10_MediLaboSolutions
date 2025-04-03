using DiabeteCheck.Models;

namespace DiabeteCheck.Services.Interfaces
{
    public interface IDiabeteCheckService
    {
        Task<RiskEvaluation> AssessRiskAsync(int patientId);
    }
}
