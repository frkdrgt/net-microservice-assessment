using AutoMapper;
using MassTransit;
using MassTransit.MultiBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Shared.Business;
using Shared.Business.Abstract;
using Shared.Business.Dto;
using Shared.Business.Services;
using Shared.Data;
using Shared.Data.Models;
using Shared.Data.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(config =>
                {

                    config.Host(new Uri("rabbitmq://localhost"), h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                }));
            });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
            services.AddDbContext<FDbContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), y => y.MigrationsAssembly("Shared/Shared.Data")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));


            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Contact, ContactDetailResponseDto>().ReverseMap();
                cfg.CreateMap<Contact, ContactListResponseDto>().ReverseMap();
                cfg.CreateMap<Contact, ContactAddRequestDto>().ReverseMap();
                cfg.CreateMap<ContactInformation, ContactInformationAddRequestDto>().ReverseMap();
                cfg.CreateMap<ContactInformation, ContactInformationDto>().ReverseMap();
                cfg.CreateMap<Report, ReportAddRequestDto>().ReverseMap();

            });

            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper);

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IContactRepository, ContactRepository>();
            services.AddTransient<IContactInformationRepository, ContactInformationRepository>();
            services.AddTransient<IReportRepository, ReportRepository>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Contact Api", Version = "v1" });
            });


            services.AddSwaggerGenNewtonsoftSupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contact Api");
            });

            app.UseCors(x => x
             .AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader());
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
