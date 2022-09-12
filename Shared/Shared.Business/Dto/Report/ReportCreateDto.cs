using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business.Dto
{
    public class ReportCreateDto
    {
        public Guid ReportId { get; set; }
        public string Path { get; set; }
    }
}
