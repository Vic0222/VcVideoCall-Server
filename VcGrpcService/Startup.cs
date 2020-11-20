using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Vc.DAL.Mongo;
using Vc.DAL.Mongo.Repositories;
using Vc.Domain.RepositoryInterfaces;
using VcGrpcService.AppServices;
using VcGrpcService.Services;

namespace VcGrpcService
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            var config = Configuration.GetSection("MongoDatabaseSetting");
            services.Configure<MongoDatabaseSetting>(config);


            services.AddGrpc();
            services.AddAutoMapper(typeof(AbstractRepository<>));
            services.AddSingleton<ChatAppService>();
            services.AddTransient<ClientManager>();
            services.Scan(scan => 
                scan.FromAssembliesOf(typeof(AbstractRepository<>))
                .AddClasses(classes=>classes.AssignableTo<IRepository>())
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            );


            //configurations
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ChatRoomService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
