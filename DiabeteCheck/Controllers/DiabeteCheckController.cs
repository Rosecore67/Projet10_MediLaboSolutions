using DiabeteCheck.Models;
using DiabeteCheck.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DiabeteCheck.Controllers
{
    public class DiabeteCheckController : Controller
    {
        [ApiController]
        [Route("api/[controller]")]
        public class DiabetesAssessmentController : ControllerBase
        {
            private readonly IDiabeteCheckService _riskService;

            public DiabetesAssessmentController(IDiabeteCheckService riskService)
            {
                _riskService = riskService;
            }

            [HttpGet("{patientId}")]
            public async Task<ActionResult<RiskEvaluation>> GetRiskLevel(int patientId)
            {
                try
                {
                    var risk = await _riskService.AssessRiskAsync(patientId);
                    return Ok(risk);
                }
                catch (Exception ex)
                {
                    return NotFound(new { message = ex.Message });
                }
            }
        }
    }
}
