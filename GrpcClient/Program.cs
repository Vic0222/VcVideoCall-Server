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
            string token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjNlNTQyN2NkMzUxMDhiNDc2NjUyMDhlYTA0YjhjYTZjODZkMDljOTMiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL3NlY3VyZXRva2VuLmdvb2dsZS5jb20vdmN2aWRlb2NhbGwtYzRhMGIiLCJhdWQiOiJ2Y3ZpZGVvY2FsbC1jNGEwYiIsImF1dGhfdGltZSI6MTYwNjE1MDkzOCwidXNlcl9pZCI6IndVblNkM1NQVWhXbmdqT3NYSzgzRWtQVkZ5VzIiLCJzdWIiOiJ3VW5TZDNTUFVoV25nak9zWEs4M0VrUFZGeVcyIiwiaWF0IjoxNjA2MTUwOTM4LCJleHAiOjE2MDYxNTQ1MzgsImVtYWlsIjoidi5nLmFsYm9uaWFuQGdtYWlsLmNvbSIsImVtYWlsX3ZlcmlmaWVkIjpmYWxzZSwiZmlyZWJhc2UiOnsiaWRlbnRpdGllcyI6eyJlbWFpbCI6WyJ2LmcuYWxib25pYW5AZ21haWwuY29tIl19LCJzaWduX2luX3Byb3ZpZGVyIjoicGFzc3dvcmQifX0.JnirYul0u36J8KX4-OT8GnYoZB8X179igNR_AtNreAMFOJuUQJDsYJJaB0AySIl8-hS5GVgtwJ8q91FD168wLx8-UpgXRZzxk2Bzjt5qrXxHkklRetR_ZFQwSWgUl7HE0ZHhOneF1zjNWoZiCS2c6qWNm0ZXi-zn3wby_mvGUff6WUdWXF1vOscVYmum4ETINA662iM1JKmQ3Vl_bmEOsupCxZiVFCtBu-6k4RG9iqvWiyCBdv0n-oAgtmaPFJg47M8dIpKok9GwhP_br7SO81dANkIJsvqndjExuO2BowzuJI-9IdNpQK_acB8Pt55tP-oO4Jiy_xX-Lnxcl1xz3g";
            headers.Add("Authorization", $"Bearer {token}");

            using (var chat = client.Join(headers))
            {
                _ = Task.Run(async () =>
                {
                    while (await chat.ResponseStream.MoveNext(cancellationToken: CancellationToken.None))
                    {
                        var response = chat.ResponseStream.Current;
                        Console.WriteLine($"{response.Sender}: {response.MessageBody}");
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
                    await chat.RequestStream.WriteAsync(message);
                }
                await chat.RequestStream.CompleteAsync();
            }

            Console.WriteLine("Disconnecting");
            await channel.ShutdownAsync();
        }
    }
}
