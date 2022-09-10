using Shared.Data.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Data.Models
{
    public class ContactInformation
    {
        public Guid Id { get; set; }

        [ForeignKey("Contact")]
        public Guid ContactId { get; set; }
        public InformationEnum InformationType { get; set; }
        public string Content { get; set; }

        public Contact Contact { get; set; }
    }
}
