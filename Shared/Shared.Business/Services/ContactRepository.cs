using AutoMapper;
using Shared.Business.Abstract;
using Shared.Business.Dto;
using Shared.Data.Models;
using Shared.Data.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business.Services
{
    public class ContactRepository : IContactRepository
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public ContactRepository(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<ApiResult<SuccessResponseDto>> Add(ContactAddRequestDto requestDto)
        {
            var result = new ApiResult<SuccessResponseDto>();

            if (requestDto == null)
            {
                result.IsSucceed = false;
                return result;
            }

            var entity = _mapper.Map<Contact>(requestDto);
            entity.Id = Guid.NewGuid();
            await _uow.ContactRepository.AddAsync(entity);

            var affectedRow = await _uow.Commit();

            if (affectedRow > 0)
            {
                result.ResultObject = new SuccessResponseDto
                {
                    Id = entity.Id,
                    Message = "Contact Created"
                };
                result.IsSucceed = true;
            }

            return result;
        }
        public async Task<ApiResult<SuccessResponseDto>> Delete(Guid id)
        {
            var result = new ApiResult<SuccessResponseDto>();

            var contact = await _uow.ContactRepository.FindByAsync(x => x.Id == id);

            if (contact == null)
            {
                result.IsSucceed = false;
                return result;
            }

            await _uow.ContactRepository.DeleteAsync(contact);

            var affectedRow = await _uow.Commit();

            if (affectedRow > 0)
            {
                result.ResultObject = new SuccessResponseDto
                {
                    Id = "",
                    Message = "Contact deleted"
                };
                result.IsSucceed = true;
            }
            return result;
        }
        public async Task<ApiResult<ContactDetailResponseDto>> Get(Guid id)
        {
            var result = new ApiResult<ContactDetailResponseDto>();
            var contact = await _uow.ContactRepository.FindByAsync(x => x.Id == id, x => x.ContactInformations);

            if (contact == null)
            {
                result.IsSucceed = false;
                return result;
            }

            var dto = _mapper.Map<ContactDetailResponseDto>(contact);

            result.ResultObject = dto;
            result.IsSucceed = true;

            return result;
        }
        public async Task<ApiResult<List<ContactListResponseDto>>> GetAll()
        {
            var result = new ApiResult<List<ContactListResponseDto>>();

            var contacts = await _uow.ContactRepository.GetAllAsync();

            var dto = _mapper.Map<List<ContactListResponseDto>>(contacts);
            
            result.ResultObject = dto;
            result.IsSucceed = true;
            return result;
        }
    }
}