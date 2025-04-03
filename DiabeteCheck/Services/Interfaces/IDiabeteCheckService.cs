using DiabeteCheck.Models;

namespace DiabeteCheck.Services.Interfaces
{
    public interface IDiabeteCheckService
    {
        Task<RiskEvaluation> ControlRiskAsync(int patientId);
    }
}
