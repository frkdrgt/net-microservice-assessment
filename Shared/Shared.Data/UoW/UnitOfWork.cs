using Shared.Data.Models;
using Shared.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Data.UoW
{
    internal class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly FDbContext _context;

        private IGenericRepository<Contact> _contactRepository;
        private IGenericRepository<ContactInformation> _contactInformationRepository;
        private IGenericRepository<Report> _reportRepository;
        
        public UnitOfWork(FDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<Contact> ContactRepository
        {
            get { return _contactRepository ?? (_contactRepository = new GenericRepository<Contact>(_context)); }
        }

        public IGenericRepository<ContactInformation> ContactInformationRepository
        {
            get { return _contactInformationRepository ?? (_contactInformationRepository = new GenericRepository<ContactInformation>(_context)); }
        }

        public IGenericRepository<Report> ReportRepository
        {
            get { return _reportRepository ?? (_reportRepository = new GenericRepository<Report>(_context)); }
        }

        public async Task<int> Commit()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var result = await _context.SaveChangesAsync();
                    transaction.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    _context.Dispose();
                    transaction.Rollback();

                    return -1;
                }
            }
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }
    }
}
