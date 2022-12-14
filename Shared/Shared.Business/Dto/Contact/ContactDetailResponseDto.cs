using Shared.Business.Dto;
using Shared.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business
{
    public class ContactDetailResponseDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public  List<ContactInformationDto> ContactInformations { get; set; }
    }
}