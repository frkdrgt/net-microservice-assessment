using Shared.Business.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business.Abstract
{
    public interface IReportRepository
    {
        Task<ApiResult<SuccessResponseDto>> Add(ReportAddRequestDto requestDto);
        Task<ApiResult<SuccessResponseDto>> CreateReport(Guid id);
    }
}