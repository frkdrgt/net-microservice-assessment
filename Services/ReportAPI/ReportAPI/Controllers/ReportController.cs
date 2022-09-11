using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Business.Abstract;
using System;
using System.Threading.Tasks;

namespace ReportAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : Controller
    {
     
        IReportRepository _reportRepository;
        public ReportController(IReportRepository reportRepository)
        {
         
            _reportRepository = reportRepository;
        }

        //FOR TEST
        [AllowAnonymous]
        [HttpGet("Get")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _reportRepository.CreateReport(id);
            if (!result.IsSucceed)
            {
                return NotFound(result.Message);
            }
            return Ok(result.ResultObject);
        }
    }
}
