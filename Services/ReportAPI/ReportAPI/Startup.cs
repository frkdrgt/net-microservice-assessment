using AutoMapper;
using MassTransit;
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
using ReportAPI.Consumers;
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

namespace ReportAPI
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
                x.AddConsumer<ReportConsumer>();
                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(new Uri(Configuration.GetSection("RabbitMqConnection").GetSection("HostName").Value), h =>
                    {
                        h.Username(Configuration.GetSection("RabbitMqConnection").GetSection("Username").Value);
                        h.Password(Configuration.GetSection("RabbitMqConnection").GetSection("Password").Value);
                    });
                    cfg.ReceiveEndpoint("reportQueue", ep =>
                    {
                        ep.PrefetchCount = 16;
                        ep.UseMessageRetry(r => r.Interval(2, 100));
                        ep.ConfigureConsumer<ReportConsumer>(provider);
                    });
                }));
            });
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.AddDbContext<FDbContext>(options =>
             options.UseNpgsql(
                   Configuration.GetConnectionString("DefaultConnection"), y => y.MigrationsAssembly("Shared/Shared.Data")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Report, ReportAddRequestDto>().ReverseMap();
                cfg.CreateMap<Report, ReportListDto>().ReverseMap();
                cfg.CreateMap<Report, ReportDetailDto>().ReverseMap();
            });

            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper);

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IContactRepository, ContactRepository>();
            services.AddTransient<IReportRepository, ReportRepository>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Report Api", Version = "v1" });
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Report Api");
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
