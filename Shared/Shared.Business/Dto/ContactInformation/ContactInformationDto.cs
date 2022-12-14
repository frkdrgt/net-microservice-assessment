using Shared.Data.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business.Dto
{
    public class ContactInformationDto
    {
        public Guid Id { get; set; } 
        public InformationEnum InformationType { get; set; }
        public string Content { get; set; }
    }
}
