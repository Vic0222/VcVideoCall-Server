using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Vc.DAL.Mongo;
using Vc.DAL.Mongo.Repositories;
using Vc.DAL.Mongo.Transactions;
using Vc.Domain.DataHelper;
using Vc.Domain.RepositoryInterfaces;
using VcGrpcService.AppServices;
using VcGrpcService.Managers;
using VcGrpcService.Middlewares;
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
            services.AddGrpcReflection();

            //need to fix life times
            services.AddAutoMapper(typeof(AbstractRepository<>), typeof(ChatAppService));
            services.AddSingleton<OnlineUserManager>();
            services.AddScoped<ChatAppService>();
            services.AddScoped<UserAppService>();
            services.AddScoped<ClientManager>();
            services.AddScoped<ITransactionManager, MongoDatabaseSessionManager>();
            services.AddScoped(serivceProvider => serivceProvider.GetService<ITransactionManager>() as MongoDatabaseSessionManager); ;

            services.Scan(scan =>
                scan.FromAssembliesOf(typeof(AbstractRepository<>))
                .AddClasses(classes => classes.AssignableTo<IRepository>())
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            );

            services.AddAuthorization();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://securetoken.google.com/vcvideocall-c4a0b";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "https://securetoken.google.com/vcvideocall-c4a0b",
                        ValidateAudience = true,
                        ValidAudience = "vcvideocall-c4a0b",
                        ValidateLifetime = true
                    };
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<SyncUserMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ChatService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });

                if (env.IsDevelopment())
                {
                    endpoints.MapGrpcReflectionService();
                }

            });
        }
    }
}
