using ConsoleApp1.Models;
using JwtApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
namespace ConsoleApp1
{
    internal class Program
    {

        static string currRefreshToken = string.Empty;

        public static async Task<LoginResponse> Login(HttpClient client, LoginRequest request)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync( "/api/Auth/login", request);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            LoginResponse loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseBody);

            currRefreshToken=loginResponse.RefreshToken;

            return loginResponse;
        }

        public static async Task<LoginResponse> RefreshAccessToken(
    HttpClient client,
    string refreshToken)
        {
            var request = new RefreshTokenRequest
            {
                RefreshToken = refreshToken
            };

            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    "/api/Auth/refresh",
                    request);

            response.EnsureSuccessStatusCode();

            string responseBody =
                await response.Content.ReadAsStringAsync();

            LoginResponse loginResponse =
                JsonConvert.DeserializeObject<LoginResponse>(responseBody);

            currRefreshToken=loginResponse.RefreshToken;

            return loginResponse;
        }

        public static async Task<HttpResponseMessage> GetWithRefresh(
    HttpClient client,
    string endpoint,
    string refreshToken)
        {
            HttpResponseMessage response =
                await client.GetAsync(endpoint);

            if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            {
                return response;
            }

            Console.WriteLine("Access token expired. Refreshing...");

            LoginResponse refreshedResponse =
                await RefreshAccessToken(client, refreshToken);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    refreshedResponse.AccessToken);

            Console.WriteLine("Access token refreshed.");

            // Retry the original request
            response = await client.GetAsync(endpoint);

            return response;
        }

        public static async Task Main(string[] args)
        {

            var client = new HttpClient();

            client.BaseAddress = new Uri("https://localhost:44364/");

            var loginRequest = new LoginRequest
            {
                Username = "gaurav",
                Password = "12345"
            };

            try
            {

                LoginResponse loginResponse = await Login(client, loginRequest);

                Console.WriteLine($"Access Token: {loginResponse.AccessToken}");

                Console.WriteLine($"Refresh Token: {loginResponse.RefreshToken}");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AccessToken);

                while (true)
                {
                    Console.WriteLine(
    $"Time: {DateTime.Now:HH:mm:ss}");

                    HttpResponseMessage response =
                        await GetWithRefresh(
                            client,
                            "/api/Test/method2",
                            currRefreshToken);

                    Console.WriteLine(
                        $"Status: {(int)response.StatusCode}");

                    string responseBody =
                        await response.Content.ReadAsStringAsync();

                    Console.WriteLine(responseBody);

                    await Task.Delay(2000);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"JSON error: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e.Message}");
            }
        }
    }
}
