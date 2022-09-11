using Shared.Business.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business.Abstract
{
    public interface IContactInformationRepository
    {
        Task<ApiResult<SuccessResponseDto>> Add(ContactInformationAddRequestDto requestDto);
        Task<ApiResult<SuccessResponseDto>> Delete(Guid id);
    }
}