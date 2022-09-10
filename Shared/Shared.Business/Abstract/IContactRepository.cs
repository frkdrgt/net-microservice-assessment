using Shared.Business.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business.Abstract
{
    public interface IContactRepository
    {
        Task<ApiResult<SuccessResponseDto>> Add(ContactAddRequestDto requestDto);
        Task<ApiResult<SuccessResponseDto>> Delete(Guid id);
        Task<ApiResult<List<ContactListResponseDto>>> GetAll();
        Task<ApiResult<ContactDetailResponseDto>> Get(Guid id);
    }
}