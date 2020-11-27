using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using VcGrpcService.Proto;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://localhost:80");
            var client = new Chat.ChatClient(channel);

            var headers = new Metadata();
            string token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjlhZDBjYjdjMGY1NTkwMmY5N2RjNTI0NWE4ZTc5NzFmMThkOWM3NjYiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL3NlY3VyZXRva2VuLmdvb2dsZS5jb20vdmN2aWRlb2NhbGwtYzRhMGIiLCJhdWQiOiJ2Y3ZpZGVvY2FsbC1jNGEwYiIsImF1dGhfdGltZSI6MTYwNjQ0ODg2MywidXNlcl9pZCI6IndVblNkM1NQVWhXbmdqT3NYSzgzRWtQVkZ5VzIiLCJzdWIiOiJ3VW5TZDNTUFVoV25nak9zWEs4M0VrUFZGeVcyIiwiaWF0IjoxNjA2NDQ4ODYzLCJleHAiOjE2MDY0NTI0NjMsImVtYWlsIjoidi5nLmFsYm9uaWFuQGdtYWlsLmNvbSIsImVtYWlsX3ZlcmlmaWVkIjpmYWxzZSwiZmlyZWJhc2UiOnsiaWRlbnRpdGllcyI6eyJlbWFpbCI6WyJ2LmcuYWxib25pYW5AZ21haWwuY29tIl19LCJzaWduX2luX3Byb3ZpZGVyIjoicGFzc3dvcmQifX0.lJqd3ZpEqcQz9jXNwCTHxPnyMR87J66Jwsb-5xUfyuoJJXqppNbw9AX-Zes7TJEv2U0xyvHetk55m-DgFRBLNoCCM6Hl8i1Oxgq4BKdbjpVf1bpnvEcLilT65CzHIsLqB4pEDn9PdRcqonbQ2ONW8NEyqPd5MxjuxW_zxJMTciibMIHs-7DMk7GgO2ukN7xAYNIcZsoq_MUI8giP6RjEaJueaFXhU1Xpi6Yyq1_2G29Svu1kd1JuAOKVDAHwsiqqC8gvxoDNue8i9UvycdQx3nRzgNBMzHk5ecn0CEFn5F1U3Ju-j0VFTODL79hXliJgsvCK9HX76PEAhdqPhwThkQ";
            headers.Add("Authorization", $"Bearer {token}");

            using (var chat = client.Join(new JoinRequest() ,headers))
            {
                _ = Task.Run(async () =>
                {
                    while (await chat.ResponseStream.MoveNext(cancellationToken: CancellationToken.None))
                    {
                        var response = chat.ResponseStream.Current;
                        Console.WriteLine($"{response.MessageNotification.SenderId}: {response.MessageNotification.RoomId}");
                    }
                });

                string line;
                while ((line = Console.ReadLine()) != null)
                {
                    if (line.ToLower() == "bye")
                    {
                        break;
                    }

                    MessageRequest message = new MessageRequest() { MessageBody = line, RoomId = "5fbcfc82231676fa807c5d3e" };

                    client.SendMessageRequest(message, headers);
                }

            }

            Console.WriteLine("Disconnecting");
            await channel.ShutdownAsync();
        }
    }
}
