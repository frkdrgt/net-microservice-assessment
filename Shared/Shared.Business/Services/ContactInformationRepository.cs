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
    public class ContactInformationRepository : IContactInformationRepository
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ContactInformationRepository(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<ApiResult<SuccessResponseDto>> Add(ContactInformationAddRequestDto requestDto)
        {
            var result = new ApiResult<SuccessResponseDto>();

            if (requestDto == null)
            {
                result.IsSucceed = false;
                return result;
            }

            var entity = _mapper.Map<ContactInformation>(requestDto);
            entity.Id = Guid.NewGuid();
            await _uow.ContactInformationRepository.AddAsync(entity);

            var affectedRow = await _uow.Commit();

            if (affectedRow > 0)
            {
                result.ResultObject = new SuccessResponseDto
                {
                    Id = entity.Id,
                    Message = "Contact Information Added"
                };
                result.IsSucceed = true;
            }

            return result;
        }

        public async Task<ApiResult<SuccessResponseDto>> Delete(Guid id)
        {
            var result = new ApiResult<SuccessResponseDto>();

            var contactInformation = await _uow.ContactInformationRepository.FindByAsync(x => x.Id == id);

            if (contactInformation == null)
            {
                result.IsSucceed = false;
                return result;
            }

            await _uow.ContactInformationRepository.DeleteAsync(contactInformation);

            var affectedRow = await _uow.Commit();

            if (affectedRow > 0)
            {
                result.ResultObject = new SuccessResponseDto
                {
                    Id = "",
                    Message = "Contact Information deleted"
                };
                result.IsSucceed = true;
            }
            return result;
        }
    }
}