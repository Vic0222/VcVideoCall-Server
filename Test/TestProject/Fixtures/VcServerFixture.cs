using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VcGrpcService;

namespace TestProject.Fixtures
{
    public class VcServerFixture : IDisposable
    {
        private readonly VcWebApplicationFactory _factory;
        public VcServerFixture()
        {
            _factory = new VcWebApplicationFactory();
            var client = _factory.CreateDefaultClient(new ResponseVersionHandler());
            GrpcChannel = GrpcChannel.ForAddress(client.BaseAddress, new GrpcChannelOptions
            {
                HttpClient = client
            });

        }

        public GrpcChannel GrpcChannel { get; private set; }

        public void Dispose()
        {
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
