using Shared.Data.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business.Dto 
{
    public class ReportDetailDto
    {
        public Guid Id { get; set; }
        public DateTime RequestDate { get; set; }
        public string FileUrl { get; set; }
        public ReportStatus Status { get; set; }
        public List<ReportViewDto> ReportDatas { get; set; }
    }
}
