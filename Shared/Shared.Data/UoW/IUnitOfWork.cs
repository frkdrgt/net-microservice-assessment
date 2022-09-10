using Shared.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Data.UoW
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Contact> ContactRepository { get; }
        IGenericRepository<ContactInformation> ContactInformationRepository { get; }
        IGenericRepository<Report> ReportRepository { get; }
        Task<int> Commit();
    }
}
