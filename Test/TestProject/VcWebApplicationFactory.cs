using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vc.DAL.Mongo;
using VcGrpcService;
using Microsoft.Extensions.DependencyInjection;
using TestProject.Fixtures;
using Microsoft.Extensions.Options;

namespace TestProject
{
    public class VcWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private MongoDatabaseFixture _mongoDatabaseFixture;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                //change databases
                string databaseName = "vcdb-test-" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
                services.Configure<MongoDatabaseSetting>(mdbsetting => {
                    mdbsetting.Database = databaseName;
                });

                var serviceProvider = services.BuildServiceProvider();

                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var logger = scopedServices
                        .GetRequiredService<ILogger<VcWebApplicationFactory>>();

                    try
                    {
                        _mongoDatabaseFixture = new MongoDatabaseFixture(scope.ServiceProvider.GetService<IOptions<MongoDatabaseSetting>>().Value);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding " +
                            "the database with test data. Error: {Message}",
                            ex.Message);
                    }
                }


            });

        }

        protected override void Dispose(bool disposing)
        {
            _mongoDatabaseFixture.Dispose();
            base.Dispose(disposing);
        }
    }
}
