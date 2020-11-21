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
            var client = new ChatRoom.ChatRoomClient(channel);

            var headers = new Metadata();
            string token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjNlNTQyN2NkMzUxMDhiNDc2NjUyMDhlYTA0YjhjYTZjODZkMDljOTMiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL3NlY3VyZXRva2VuLmdvb2dsZS5jb20vdmN2aWRlb2NhbGwtYzRhMGIiLCJhdWQiOiJ2Y3ZpZGVvY2FsbC1jNGEwYiIsImF1dGhfdGltZSI6MTYwNTkzODU1OSwidXNlcl9pZCI6Ijg4TmU1ZVgyQzZaTXNHMHdaQ0FUSm5FTUZXSDMiLCJzdWIiOiI4OE5lNWVYMkM2Wk1zRzB3WkNBVEpuRU1GV0gzIiwiaWF0IjoxNjA1OTM4NTU5LCJleHAiOjE2MDU5NDIxNTksImVtYWlsIjoidi5nLmFsYm9uaWFuLmRldkBnbWFpbC5jb20iLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsImZpcmViYXNlIjp7ImlkZW50aXRpZXMiOnsiZW1haWwiOlsidi5nLmFsYm9uaWFuLmRldkBnbWFpbC5jb20iXX0sInNpZ25faW5fcHJvdmlkZXIiOiJwYXNzd29yZCJ9fQ.mvw7XofWKg8AgT8PtlWBx1ngmJZ3NKSQdCko0jDz5qcc4O6J5BDGYdUy4dcPSuCkrToBvOCjyQqfjXdXOBnWa9LtEPavQWCRBSO17bvBOkuBvXZIc_z02XErNC0o4AKF72DOGCB9JMIlqopFUmN8obpLyIQthjBBVuxX4bBR0ZTNstKaefe6hoTZWs2yYDijQOyre5mmWQIQQ7cel3EbmY7k2EklV-RVfNsVHug_6B9o5uHiBzH6Wy27WeylJiUbmfXkcFrTx0oEV4mCGeth1mKhPYVK5NAk_9bfd0bbsH9yWCS4nPCQT-b--AaG4nakkM5jIu72Qdbf598OaZqqTg";
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

                    MessageRequest message = new MessageRequest() { Sender = "wUnSd3SPUhWngjOsXK83EkPVFyW2", MessageBody = line, Target = "88Ne5eX2C6ZMsG0wZCATJnEMFWH3", Type = 1 };
                    await chat.RequestStream.WriteAsync(message);
                }
                await chat.RequestStream.CompleteAsync();
            }

            Console.WriteLine("Disconnecting");
            await channel.ShutdownAsync();
        }
    }
}
