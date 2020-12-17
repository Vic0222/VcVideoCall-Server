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
    public class UserAcceptTest
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
        public async Task ShouldAcceptInviteSuccessfully()
        {
            var client = new Chat.ChatClient(_vcServerFixture.GrpcChannel);
            var headers = new Metadata();
            await headers.AddIdTokenAsync("user2", _configuration);

            var response = await client.GetRoomAsync(new GetRoomRequest() { RoomId = "5fbcfc82231676fa807c5d3e", Type = GetRoomType.FromRoomId }, headers);

            Assert.IsNotNull(response.Room, "Room was null");

            var userAcceptResponse = await client.SendUserAcceptAsync(new UserAcceptRequest() { RoomId = response.Room?.Id }, headers);

            Assert.AreEqual(RoomStatus.RoomAccepted, userAcceptResponse.Room.Status);

        }
    }
}
