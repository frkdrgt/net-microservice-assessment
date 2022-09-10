using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shared.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FDbContext>
    {
        public FDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<FDbContext>();
            IConfiguration config = new ConfigurationBuilder().SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../APIGateway")).AddJsonFile("appsettings.json").Build();

            builder.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            return new FDbContext(builder.Options);
        }
    }
}
