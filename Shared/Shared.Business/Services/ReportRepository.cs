using AutoMapper;
using Shared.Business.Abstract;
using Shared.Business.Dto;
using Shared.Data.Enum;
using Shared.Data.Models;
using Shared.Data.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business.Services
{
    public class ReportRepository : IReportRepository
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public ReportRepository(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<ApiResult<SuccessResponseDto>> Add(ReportAddRequestDto requestDto)
        {
            var result = new ApiResult<SuccessResponseDto>();

            if (requestDto == null)
            {
                result.IsSucceed = false;
                return result;
            }

            var entity = _mapper.Map<Report>(requestDto);
            entity.Id = Guid.NewGuid();
            entity.Status = Data.Enum.ReportStatus.PROCESSING;
            await _uow.ReportRepository.AddAsync(entity);

            var affectedRow = await _uow.Commit();

            if (affectedRow > 0)
            {
                result.ResultObject = new SuccessResponseDto
                {
                    Id = entity.Id,
                    Message = "Report is processing now"
                };
                result.IsSucceed = true;
            }

            return result;
        }

        public async Task<ApiResult<SuccessResponseDto>> CreateReport(Guid reportId)
        {
            var result = new ApiResult<SuccessResponseDto>();

            var getReport = await _uow.ReportRepository.FindByAsync(x => x.Id == reportId);

            if (getReport == null)
            {
                result.IsSucceed = false;
                return result;
            }

            return result;
        }

    }

    class ReportLocationDto
    {
        public Guid ContactId { get; set; }
        public InformationEnum InformationType { get; set; }
        public string Content { get; set; }
    }
}