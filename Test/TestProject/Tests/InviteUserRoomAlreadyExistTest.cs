using Grpc.Core;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject.Fixtures;
using TestProject.Helpers;
using VcGrpcService.Proto;

namespace TestProject.Tests
{
    public class InviteUserRoomAlreadyExistTest
    {
        private IConfigurationRoot _configuration;
        private VcServerFixture _vcServerFixture;

        [SetUp]
        public void Setup()
        {
            _configuration = TestConfigurationProvider.GetConfiguration();
            _vcServerFixture = new VcServerFixture();
            _vcServerFixture.Init(createRoom: true);
        }

        [TearDown]
        public void TearDown()
        {
            _vcServerFixture?.Dispose();
        }
        [Test]
        public async Task ShouldReturnServerError()
        {
            var client = new Chat.ChatClient(_vcServerFixture.GrpcChannel);
            var headers = new Metadata();
            await headers.AddIdTokenAsync("user1", _configuration);
            var exception = Assert.Throws<RpcException>(() => {
                var response = client.SendUserInvite(new UserInviteRequest() { UserId = "88Ne5eX2C6ZMsG0wZCATJnEMFWH3" }, headers);
                
            }, "No exception thrown");

        }
    }
}
