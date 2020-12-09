using Grpc.Core;
using Grpc.Net.Client;
using GrpcClient.Models;
using GrpcClient.Response;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using VcGrpcService.Proto;

namespace GrpcClient
{
    /// <summary>
    /// Cli for testing the grpc server.
    /// !Important: create a settings.json base from settings - sample.json and ignore from source con
    /// </summary>
    class Program
    {
        private static IConfigurationRoot _configuration;

        static async Task Main(string[] args)
        {
            SetupConfiguration();
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://localhost:80");
            var client = new Chat.ChatClient(channel);

            var headers = new Metadata();
            string token = await LoginAsync();
            headers.Add("Authorization", $"Bearer {token}");
            if (args[0] == "searchusers")
            {
                string keyword = args[1];
                await SearchUserAsync(client, keyword, headers);
            }
            else
            {
                await TestJoinAndChat(channel, client, headers);
            }
            
        }

        private static async Task SearchUserAsync(Chat.ChatClient client, string keyword, Metadata headers)
        {

            SearchUserRequest request = new SearchUserRequest() { Keyword = keyword };
            var response = await client.SearchUserAsync(request, headers);
            Console.WriteLine(JsonConvert.SerializeObject(response));
        }

        static async Task<string> LoginAsync()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("https://identitytoolkit.googleapis.com/");

                IConfigurationSection configurationSection = _configuration.GetSection("loginRequest");
                LoginRequest loginRequest = configurationSection.Get<LoginRequest>();
                string apiKey = _configuration.GetValue<string>("apiKey"); ;
                var response = await httpClient.PostAsJsonAsync($"v1/accounts:signInWithPassword?key={apiKey}", loginRequest);
                string responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
                return loginResponse?.IdToken ?? string.Empty;
            }
             
        }

        static void SetupConfiguration()
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            configurationBuilder.AddJsonFile("appsettings." + Environment.GetEnvironmentVariable("Environment") + ".json");

            _configuration = configurationBuilder.Build();

        }

        private static async Task TestJoinAndChat(GrpcChannel channel, Chat.ChatClient client, Metadata headers)
        {
            using (var chat = client.Join(new JoinRequest(), headers))
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
