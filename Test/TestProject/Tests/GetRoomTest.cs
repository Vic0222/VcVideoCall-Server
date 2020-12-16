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
    public class GetRoomTest
    {
        private IConfigurationRoot _configuration;
        private VcServerFixture _vcServerFixture;

        [SetUp]
        public void Setup()
        {
            _configuration = TestConfigurationProvider.GetConfiguration();
            _vcServerFixture = new VcServerFixture();
            _vcServerFixture.Init();
        }

        [TearDown]
        public void TearDown()
        {
            _vcServerFixture?.Dispose();
        }

        [Test]
        public async Task ShouldGetRoomByUserId_StatusInvitePending()
        {
            var client = new Chat.ChatClient(_vcServerFixture.GrpcChannel);
            var headers = new Metadata();
            await headers.AddIdTokenAsync("user1", _configuration);
            var response = await client.GetRoomAsync(new GetRoomRequest() { Type = GetRoomType.FromUserIdPrivate, UserId = "88Ne5eX2C6ZMsG0wZCATJnEMFWH3" }, headers);

            Assert.AreEqual(response.Room.Id, "5fbcfc82231676fa807c5d3e", "Invalid room id");
            Assert.AreEqual(RoomStatus.RoomInvitePending, response.RoomStatus, "Invalid room status");
        }


        [Test]
        public async Task ShouldGetRoomByUserId_StatusAcceptPending()
        {
            var client = new Chat.ChatClient(_vcServerFixture.GrpcChannel);
            var headers = new Metadata();
            await headers.AddIdTokenAsync("user2", _configuration);
            var response = await client.GetRoomAsync(new GetRoomRequest() { Type = GetRoomType.FromUserIdPrivate, UserId = "wUnSd3SPUhWngjOsXK83EkPVFyW2" }, headers);

            Assert.AreEqual(response.Room.Id, "5fbcfc82231676fa807c5d3e", "Invalid room id");
            Assert.AreEqual(RoomStatus.RoomAcceptPending, response.RoomStatus, "Invalid room status");
        }

        [Test]
        public async Task ShouldGetRoomByRoomId_StatusInvitePending()
        {
            var client = new Chat.ChatClient(_vcServerFixture.GrpcChannel);
            var headers = new Metadata();
            await headers.AddIdTokenAsync("user1", _configuration);
            var response = await client.GetRoomAsync(new GetRoomRequest() { Type = GetRoomType.FromRoomId, RoomId = "5fbcfc82231676fa807c5d3e" }, headers);

            Assert.AreEqual(response.Room.Id, "5fbcfc82231676fa807c5d3e", "Invalid room id");
            Assert.AreEqual(RoomStatus.RoomInvitePending, response.RoomStatus, "Invalid room status");
        }

        [Test]
        public async Task ShouldGetRoomByRoomId_StatusAcceptPending()
        {
            var client = new Chat.ChatClient(_vcServerFixture.GrpcChannel);
            var headers = new Metadata();
            await headers.AddIdTokenAsync("user2", _configuration);
            var response = await client.GetRoomAsync(new GetRoomRequest() { Type = GetRoomType.FromRoomId, RoomId = "5fbcfc82231676fa807c5d3e" }, headers);

            Assert.AreEqual(response.Room.Id, "5fbcfc82231676fa807c5d3e", "Invalid room id");
            Assert.AreEqual(RoomStatus.RoomAcceptPending, response.RoomStatus, "Invalid room status");
        }
    }
}
