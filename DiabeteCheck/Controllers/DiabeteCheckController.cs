using DiabeteCheck.Models;
using DiabeteCheck.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiabeteCheck.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DiabetesCheckController : ControllerBase
    {
        private readonly IDiabeteCheckService _riskService;

        public DiabetesCheckController(IDiabeteCheckService riskService)
        {
            _riskService = riskService;
        }

        [HttpGet("{patientId}")]
        public async Task<ActionResult<RiskEvaluation>> GetRiskLevel(int patientId)
        {
            try
            {
                var risk = await _riskService.ControlRiskAsync(patientId);
                return Ok(risk);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
