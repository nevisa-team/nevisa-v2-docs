using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;

namespace NevisaConsoleApp
{
    internal class NevisaAPI
    {
        private const string SERVER_ADDRESS = "https://ent.persianspeech.com";
        private const string LOGIN_API = "/api/auth/login";
        private const string ADD_FILE_API = "/api/files/add";
        public async Task<(string, string)> Login(string userName, string password)
        {
            string request = $"{{\"username\": \"{userName}\", \"password\": \"{password}\" }}";

            StringContent content = new StringContent(request, Encoding.UTF8, "application/json");

            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var httpClient = new HttpClient(handler);

            var response = await httpClient.PostAsync($"{SERVER_ADDRESS}{LOGIN_API}", content);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string responseString = await response.Content.ReadAsStringAsync();

                var json = JToken.Parse(responseString);

                string token = (string)json["token"];

                string code = (string)json["code"];

                return (token, code);
            }
            else
            {
                throw new Exception("login Failed! " + response.ReasonPhrase);
            }

        }

        public async Task<string?> AddFile(string token, string filePath, bool usePunctuations = false, bool useTextToNumber = false, string userPlatform = "browser")
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("authorization", token);
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                MultipartFormDataContent formData = new MultipartFormDataContent();
                formData.Add(new StringContent(usePunctuations ? "true" : "false"), "usePunctuations");
                formData.Add(new StringContent(useTextToNumber ? "true" : "false"), "useTextToNumber");
                formData.Add(new StringContent(userPlatform), "userPlatform");

                using (FileStream fileStream = System.IO.File.OpenRead(filePath))
                {
                    formData.Add(new StreamContent(fileStream), "files", Path.GetFileName(filePath));
                    HttpResponseMessage response = await client.PostAsync(SERVER_ADDRESS + ADD_FILE_API, formData);

                    if (response.IsSuccessStatusCode)
                    {
                        string files = await response.Content.ReadAsStringAsync();
                        //string code = response.Headers.GetValues("code").ToString();
                        return files;
                    }
                    else
                    {
                        throw new Exception("add_file Failed! " + response.ReasonPhrase);
                    }
                }
            }
        }
    }
}
