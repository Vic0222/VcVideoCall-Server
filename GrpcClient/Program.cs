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
            string token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjNlNTQyN2NkMzUxMDhiNDc2NjUyMDhlYTA0YjhjYTZjODZkMDljOTMiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL3NlY3VyZXRva2VuLmdvb2dsZS5jb20vdmN2aWRlb2NhbGwtYzRhMGIiLCJhdWQiOiJ2Y3ZpZGVvY2FsbC1jNGEwYiIsImF1dGhfdGltZSI6MTYwNTk0MjUyMCwidXNlcl9pZCI6Ijg4TmU1ZVgyQzZaTXNHMHdaQ0FUSm5FTUZXSDMiLCJzdWIiOiI4OE5lNWVYMkM2Wk1zRzB3WkNBVEpuRU1GV0gzIiwiaWF0IjoxNjA1OTQyNTIwLCJleHAiOjE2MDU5NDYxMjAsImVtYWlsIjoidi5nLmFsYm9uaWFuLmRldkBnbWFpbC5jb20iLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsImZpcmViYXNlIjp7ImlkZW50aXRpZXMiOnsiZW1haWwiOlsidi5nLmFsYm9uaWFuLmRldkBnbWFpbC5jb20iXX0sInNpZ25faW5fcHJvdmlkZXIiOiJwYXNzd29yZCJ9fQ.BWS-3FJemqUK6YAhFPhuaZKCKwu3LjvIWoCSccwp7m1hONsLZb6alE15QKKTxOUZSAB0vrhFpcIoKN_cUBP4fgqGzjJrzs7rb9aOeN5HKzHbBIhPSpB_j-o9fUmS4-SOTm8fi9R9oGOOeocQGeAIrnYmJdbGZOYSXMZPEbWeBjSjcbjAdUGVYiOD6b0_mun751-3aZgDUpBZDkTLGgJfi2GrvH-RNH-Fo0PJXwAYZmdeA1LTWGf_VOOVWnFBVs4wzuJYP8r85LZ87oIgJ-WmUc8WkotFlrzDVxtpWO1dbTbgDeCzJUry8urru_jso89llkORRWn7mk_l-DH5BfHgEg";
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

                    MessageRequest message = new MessageRequest() { MessageBody = line, Target = "wUnSd3SPUhWngjOsXK83EkPVFyW2", Type = 1 };
                    await chat.RequestStream.WriteAsync(message);
                }
                await chat.RequestStream.CompleteAsync();
            }

            Console.WriteLine("Disconnecting");
            await channel.ShutdownAsync();
        }
    }
}
