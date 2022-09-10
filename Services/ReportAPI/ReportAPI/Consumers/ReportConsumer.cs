using MassTransit;
using Shared.Business.Dto;
using System.Threading.Tasks;

namespace ReportAPI.Consumers
{
    public class ReportConsumer : IConsumer<ContactAddRequestDto>
    {
        public async Task Consume(ConsumeContext<ContactAddRequestDto> context)
        {
            var data = context.Message;
            //Create report

            await Task.FromResult<bool>(true);
        }
    }
}