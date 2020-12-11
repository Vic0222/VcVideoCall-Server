using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TestProject.Fixtures;
using TestProject.Helpers;
using TestProject.Models.Request;
using TestProject.Models.Response;
using VcGrpcService.Proto;

namespace TestProject.Tests
{
    public class JoinTest
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



        private async Task<Metadata> generateMetadata(string userkey)
        {
            var headers = new Metadata();
            await headers.AddIdTokenAsync(userkey, _configuration);
            return headers;
        }


        [Test]
        public async Task ShouldReceiveMessage()
        {
            var client = new Chat.ChatClient(_vcServerFixture.GrpcChannel);

            string expectedRoomId = "5fbcfc82231676fa807c5d3e";
            string actualRoomId = string.Empty;

            Metadata senderHeaders = await generateMetadata("user1");
            Metadata receiverHeaders = await generateMetadata("user2");
            using (var recieverJoinResponse = client.Join(new JoinRequest(), receiverHeaders))
            {


                //receiver task added discard to remove warning
                var receiverTask = Task.Run(async () =>
                 {
                     while (await recieverJoinResponse.ResponseStream.MoveNext(cancellationToken: CancellationToken.None) && string.IsNullOrEmpty(actualRoomId))
                     {
                         var response = recieverJoinResponse.ResponseStream.Current;
                         if (response.Type == JoinResponseType.Notification)
                         {
                             actualRoomId = response.MessageNotification.RoomId;
                         }
                     }
                 });

                //refactor to dynamic room id

                MessageRequest message = new MessageRequest() { MessageBody = "test message", RoomId = expectedRoomId };

                client.SendMessageRequest(message, senderHeaders);

                var start = DateTime.Now;
                var timeout = TimeSpan.FromSeconds(10);
                while (string.IsNullOrEmpty(actualRoomId) && DateTime.Now.Subtract(start) < timeout)
                {
                    await Task.Delay(500);
                }

                Assert.IsNotEmpty(actualRoomId, "Room id was empty");
                Assert.AreEqual(expectedRoomId, actualRoomId, "Wrong room Id");
            }
        }


    }
}