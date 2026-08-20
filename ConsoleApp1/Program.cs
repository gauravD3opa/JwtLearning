using ConsoleApp1.Models;
using JwtApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
namespace ConsoleApp1
{
    internal class Program
    {

        public static async Task<string> GetToken(HttpClient client, LoginRequest request)
        {

            // 1. Send the POST request
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/Auth/login", request);

            // 2. Ensure the API returned a successful status code (e.g., 200 OK)
            response.EnsureSuccessStatusCode();

            // 3. Read the actual JSON body content as a string
            string responseBody = await response.Content.ReadAsStringAsync();

            // 4. Deserialize the string into your object
            LoginResponse loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseBody);

            return loginResponse.Token;
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
                string token = await GetToken(client, loginRequest);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // get an HttpResponseMessage object
                HttpResponseMessage response = await client.GetAsync("/api/Test/method2");

                // This will now work perfectly
                response.EnsureSuccessStatusCode();

                // This will now work perfectly
                string responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response from /api/Test/method2: {responseBody}");


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
