using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Business.Abstract;
using Shared.Business.Dto;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ContactAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : Controller
    {
        IContactRepository _contactRepository;
        IReportRepository _reportRepository;
        private readonly IBus _bus;
        public ContactController(IContactRepository contactRepository, IBus bus, IReportRepository reportRepository)
        {
            _contactRepository = contactRepository;
            _bus = bus;
            _reportRepository = reportRepository;
        }


        [HttpPost("CreateReport")]
        public async Task<IActionResult> CreateReport()
        {
            ReportAddRequestDto contactAddRequestDto = new ReportAddRequestDto();
            contactAddRequestDto.RequestDate = DateTime.UtcNow;

            var result = await _reportRepository.Add(contactAddRequestDto);
            if (!result.IsSucceed)
            {
                return NotFound(result.Message);
            }

            ReportCreateDto reportCreateDto = new ReportCreateDto();
            reportCreateDto.ReportId = Guid.Parse(result.ResultObject.Id.ToString()); 
            Uri uri = new Uri("rabbitmq://localhost/reportQueue");
            var endPoint = await _bus.GetSendEndpoint(uri);
            await endPoint.Send(reportCreateDto);
            
            return Ok(result.ResultObject); 
        }

        [AllowAnonymous]
        [HttpPost("Add")]
        public async Task<IActionResult> Add(ContactAddRequestDto requestDto)
        {
            var result = await _contactRepository.Add(requestDto);
            if (!result.IsSucceed)
            {
                return NotFound(result.Message);
            }
            return Ok(result.ResultObject);
        }

        [AllowAnonymous]
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _contactRepository.GetAll();
            if (!result.IsSucceed)
            {
                return NotFound(result.Message);
            }
            return Ok(result.ResultObject);
        }

        [AllowAnonymous]
        [HttpGet("Get")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _contactRepository.Get(id);
            if (!result.IsSucceed)
            {
                return NotFound(result.Message);
            }
            return Ok(result.ResultObject);
        }
    }
}
