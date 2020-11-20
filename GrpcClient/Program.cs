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

            using (var chat = client.Join())
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
                    await chat.RequestStream.WriteAsync(new MessageRequest() { Sender = "5fb7d5629c3b8e7f336dcb3c", MessageBody = line, Target = "5fb7d5629c3b8e7f336dcb3d", Type = 1 });
                }
                await chat.RequestStream.CompleteAsync();
            }

            Console.WriteLine("Disconnecting");
            await channel.ShutdownAsync();
        }
    }
}
