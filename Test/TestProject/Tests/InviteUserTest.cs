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
    public class InviteUserTest
    {
        private IConfigurationRoot _configuration;
        private VcServerFixture _vcServerFixture;

        [SetUp]
        public void Setup()
        {
            _configuration = TestConfigurationProvider.GetConfiguration();
            _vcServerFixture = new VcServerFixture();
            _vcServerFixture.Init(createRoom: false);
        }

        [TearDown]
        public void TearDown()
        {
            _vcServerFixture?.Dispose();
        }
        [Test]
        public async Task ShouldReturnARoom()
        {
            var client = new Chat.ChatClient(_vcServerFixture.GrpcChannel);
            var headers = new Metadata();
            await headers.AddIdTokenAsync("user1", _configuration);
            var response = await client.SendInviteToUserAsync(new InviteUserRequest() { UserId = "88Ne5eX2C6ZMsG0wZCATJnEMFWH3" }, headers);

            Assert.IsNotNull(response.Room, "Room was null");

            //check room status. should be invite pending
            Assert.AreEqual(RoomStatus.RoomInvitePending, response.Room.Status, "Room was null");
        }
    }
}
