using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VcGrpcService;
using Microsoft.Extensions.DependencyInjection;
using Vc.DAL.Mongo;
using Microsoft.Extensions.Options;

namespace TestProject.Fixtures
{
    public class VcServerFixture : IDisposable
    {
        private readonly VcWebApplicationFactory _factory;
        private MongoDatabaseFixture _mongoDatabaseFixture;

        public VcServerFixture()
        {
            _factory = new VcWebApplicationFactory();

        }

        public void Init(bool createRoom = true)
        {
            var client = _factory.WithWebHostBuilder(builder => {

                builder.ConfigureServices(services => 
                {
                    string databaseName = "vcdb-test-" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
                    services.Configure<MongoDatabaseSetting>(mdbsetting => {
                        mdbsetting.Database = databaseName;
                    });

                    var serviceProvider = services.BuildServiceProvider();
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var scopedServices = scope.ServiceProvider;
                        var logger = scopedServices
                            .GetRequiredService<ILogger<VcServerFixture>>();

                        try
                        {
                            _mongoDatabaseFixture = new MongoDatabaseFixture(scope.ServiceProvider.GetService<IOptions<MongoDatabaseSetting>>().Value);
                            _mongoDatabaseFixture.Seed(createRoom: createRoom);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An error occurred seeding " +
                                "the database with test data. Error: {Message}",
                                ex.Message);
                        }
                    }
                });
                
            }).CreateDefaultClient(new ResponseVersionHandler());

            GrpcChannel = GrpcChannel.ForAddress(client.BaseAddress, new GrpcChannelOptions
            {
                HttpClient = client
            });

        }

        public GrpcChannel GrpcChannel { get; private set; }

        public void Dispose()
        {
            _mongoDatabaseFixture?.Dispose();
            _factory.Dispose();
        }
        private class ResponseVersionHandler : DelegatingHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var response = await base.SendAsync(request, cancellationToken);
                response.Version = request.Version;
                return response;
            }
        }

    }
}
