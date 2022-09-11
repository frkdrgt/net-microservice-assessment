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
    public class ContactInformationController : Controller
    {
        IContactInformationRepository _contactInformationRepository;
        public ContactInformationController(IContactInformationRepository contactInformationRepository)
        {
            _contactInformationRepository = contactInformationRepository;
        }


        [AllowAnonymous]
        [HttpPost("Add")]
        public async Task<IActionResult> Add(ContactInformationAddRequestDto requestDto)
        {
            var result = await _contactInformationRepository.Add(requestDto);
            if (!result.IsSucceed)
            {
                return NotFound(result.Message);
            }
            return Ok(result.ResultObject);
        }

        [AllowAnonymous]
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _contactInformationRepository.Delete(id);
            if (!result.IsSucceed)
            {
                return NotFound(result.Message);
            }
            return Ok(result.ResultObject);
        }
    }
}
