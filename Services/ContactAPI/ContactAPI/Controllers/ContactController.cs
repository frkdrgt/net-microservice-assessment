using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Business.Abstract;
using Shared.Business.Dto;
using System;
using System.Threading.Tasks;

namespace ContactAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : Controller
    {
        IContactRepository _contactRepository;
        private readonly IBus _bus;
        public ContactController(IContactRepository contactRepository, IBus bus)
        {
            _contactRepository = contactRepository;
            _bus = bus;
        }
        

        //TODO:
        [HttpPost("CreateTicket")]
        public async Task<IActionResult> CreateReport()
        {
            ContactAddRequestDto contactAddRequestDto = new ContactAddRequestDto();
            contactAddRequestDto.FirstName = "Omer";
            Uri uri = new Uri("rabbitmq://localhost/reportQueue");
            var endPoint = await _bus.GetSendEndpoint(uri);
            await endPoint.Send(contactAddRequestDto);
            return Ok();
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
