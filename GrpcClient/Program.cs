using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using VcGrpcService;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new Chat.ChatClient(channel);

            var headers = new Metadata();
            string token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjNlNTQyN2NkMzUxMDhiNDc2NjUyMDhlYTA0YjhjYTZjODZkMDljOTMiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL3NlY3VyZXRva2VuLmdvb2dsZS5jb20vdmN2aWRlb2NhbGwtYzRhMGIiLCJhdWQiOiJ2Y3ZpZGVvY2FsbC1jNGEwYiIsImF1dGhfdGltZSI6MTYwNjIyMDIxMSwidXNlcl9pZCI6IndVblNkM1NQVWhXbmdqT3NYSzgzRWtQVkZ5VzIiLCJzdWIiOiJ3VW5TZDNTUFVoV25nak9zWEs4M0VrUFZGeVcyIiwiaWF0IjoxNjA2MjIwMjExLCJleHAiOjE2MDYyMjM4MTEsImVtYWlsIjoidi5nLmFsYm9uaWFuQGdtYWlsLmNvbSIsImVtYWlsX3ZlcmlmaWVkIjpmYWxzZSwiZmlyZWJhc2UiOnsiaWRlbnRpdGllcyI6eyJlbWFpbCI6WyJ2LmcuYWxib25pYW5AZ21haWwuY29tIl19LCJzaWduX2luX3Byb3ZpZGVyIjoicGFzc3dvcmQifX0.sVd85pdw4BldjYCOVWNKwqa3QEdFSLq_I0ty8DC6G002ZlZG_uYRV3ILgQPedZGCYc5X5TS2FsyyC9DLG9cca2omgjfig1pZqcQhflBUZUS2oJSwyNNqU_mapyAEmir1z6N9rwVrIxJtCLASP8v6ORXd6iY_1l1hDb4Yc__3YrLjFpJpt8nXAR5QRwh-5giS2JcUJYKmOLA8lVxQ4pmnAELLffk8JqdExp4iFkdMNFzs8oM0dDED372Y0YXLuI4EKK-77qoK3481DAeGY6pKw2qO1zIHyhQYP44dJOG8mySOkdM2GndyWz_NzyHOpbxsiDExDAoCZrBT560fWncIfA";
            headers.Add("Authorization", $"Bearer {token}");

            using (var chat = client.Join(headers))
            {
                _ = Task.Run(async () =>
                {
                    while (await chat.ResponseStream.MoveNext(cancellationToken: CancellationToken.None))
                    {
                        var response = chat.ResponseStream.Current;
                        Console.WriteLine($"{response.Notification.Sender}: {response.Notification.MessageBody}");
                    }
                });

                string line;
                while ((line = Console.ReadLine()) != null)
                {
                    if (line.ToLower() == "bye")
                    {
                        break;
                    }

                    MessageRequest message = new MessageRequest() { MessageBody = line, Target = "88Ne5eX2C6ZMsG0wZCATJnEMFWH3", Type = RoomTypeReply.Private };
                    JoinRequest joinRequest = new JoinRequest() { MessageRequest = message };
                    await chat.RequestStream.WriteAsync(joinRequest);
                }
                await chat.RequestStream.CompleteAsync();
            }

            Console.WriteLine("Disconnecting");
            await channel.ShutdownAsync();
        }
    }
}
