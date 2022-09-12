using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business.Dto 
{
    public class ReportViewDto
    {
        public string Content { get; set; }
        public int ContactCount { get; set; }
        public int RegisteredContactCount { get; set; }
    }
}
