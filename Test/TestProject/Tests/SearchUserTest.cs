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
    public class SearchUserTest
    {
        private IConfigurationRoot _configuration;
        private VcServerFixture _vcServerFixture;

        [SetUp]
        public void Setup()
        {
            _configuration = TestConfigurationProvider.GetConfiguration();
            _vcServerFixture = new VcServerFixture();
        }

        [TearDown]
        public void TearDown()
        {
            _vcServerFixture?.Dispose();
        }

        [Test]
        public async Task ShouldRetreiveTwoUsers()
        {
            var client = new Chat.ChatClient(_vcServerFixture.GrpcChannel);
            var headers = new Metadata();
            await headers.AddIdTokenAsync("user1", _configuration);
            var response = await client.SearchUserAsync(new SearchUserRequest() { Keyword = "v.g.a" }, headers);


            Assert.GreaterOrEqual(response.Users.Count, 2, "User count should be greater than 2.");
            Assert.NotNull(response.Users.FirstOrDefault(u => u.UserId == "wUnSd3SPUhWngjOsXK83EkPVFyW2"), "User with id wUnSd3SPUhWngjOsXK83EkPVFyW2 not found.");
            Assert.NotNull(response.Users.FirstOrDefault(u => u.UserId == "88Ne5eX2C6ZMsG0wZCATJnEMFWH3"), "User with id 88Ne5eX2C6ZMsG0wZCATJnEMFWH3 not found.");
        }
    }
}
