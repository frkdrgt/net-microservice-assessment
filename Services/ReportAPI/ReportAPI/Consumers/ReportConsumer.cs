using MassTransit;
using Shared.Business.Abstract;
using Shared.Business.Dto;
using System.IO;
using System.Threading.Tasks;

namespace ReportAPI.Consumers
{
    public class ReportConsumer : IConsumer<ReportCreateDto>
    {
        IReportRepository _reportRepository;
        public ReportConsumer(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task Consume(ConsumeContext<ReportCreateDto> context)
        {
            var data = context.Message;
            string path = Path.Combine(Directory.GetCurrentDirectory(), "ReportFiles");
            data.Path = path;
            await _reportRepository.CreateReport(data);

            await Task.FromResult<bool>(true);
        }
    }
}