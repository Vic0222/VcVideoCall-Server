using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestProject.Models.Request;
using TestProject.Models.Response;

namespace TestProject.Helpers
{
    public static class AuthenticationHelper
    {
        public static async Task AddIdToken(this Metadata headers, string userKey, IConfiguration configuration)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("https://identitytoolkit.googleapis.com/");

                IConfigurationSection configurationSection = configuration.GetSection($"loginRequest:{userKey}");
                LoginRequest loginRequest = configurationSection.Get<LoginRequest>();
                string apiKey = configuration.GetValue<string>("apiKey"); ;
                var response = await httpClient.PostAsJsonAsync($"v1/accounts:signInWithPassword?key={apiKey}", loginRequest);
                string responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
                if (!string.IsNullOrEmpty(loginResponse?.IdToken ?? string.Empty))
                {
                    headers.Add("Authorization", $"Bearer {loginResponse?.IdToken}");
                }
            }
        }
    }
}
