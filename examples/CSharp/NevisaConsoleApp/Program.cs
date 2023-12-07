using Newtonsoft.Json.Linq;
using SocketIOClient;
using System.Text;

static class Program
{
    class LockStatus
    {
        public bool lockChecked { get; set; }
    }

    class Result
    {
        public string partial { get; set; }
        public string text { get; set; }
    }

    //----------------------------------------------------------------------

    private static async Task Main()
    {
        string serverAddress = "https://ent.persianspeech.com";
        string streamAddress = "your-stream-address";
        string userName = "your-user-name";
        string password = "your-password";
        string token = "";
        bool finished = false;
        bool recording = false;
        Console.OutputEncoding = Encoding.Unicode;

        string request = $"{{\"username\": \"{userName}\", \"password\": \"{password}\" }}";

        StringContent content = new StringContent(request, Encoding.UTF8, "application/json");

        var handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var httpClient = new HttpClient(handler);

        var response = await httpClient.PostAsync($"{serverAddress}/api/auth/login", content);

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            string responseString = await response.Content.ReadAsStringAsync();

            var json = JToken.Parse(responseString);

            token = (string)json["token"];
        }
        else
        {
            Console.WriteLine("Login Error!");
            Console.ReadKey();
            return;
        }

        //----------------------------------------------------------------------

        SocketIOOptions options = new()
        {
            ExtraHeaders = new()
        {
            { "token", token },
            { "platform", "browser" }
        }
        };

        var socketIOClient = new SocketIOClient.SocketIO(serverAddress, options);

        socketIOClient.OnConnected += (sender, e) =>
        {
            Console.WriteLine("Connected!");
            Console.WriteLine("R: Start Recording, S: Stop Recording, Esc: Exit");
        };

        socketIOClient.OnDisconnected += (sender, e) =>
        {
            Console.WriteLine("Disconnected!");
        };

        socketIOClient.On("result", response =>
        {
            // You can print the returned data first to decide what to do next.
            //Console.WriteLine(response);
            if (response.GetValue<Result>(0).partial is not null)
            {
                Console.WriteLine("partial: " + response.GetValue<Result>(0).partial);
            }
            if (response.GetValue<Result>(0).text is not null)
            {
                Console.WriteLine("text: " + response.GetValue<Result>(0).text);
            }
        });

        socketIOClient.OnError += (sender, e) =>
        {
            Console.WriteLine(e);
        };

        socketIOClient.On("start-streaming", data =>
            {
                if (data.GetValue<LockStatus>(0).lockChecked)
                {
                    try
                    {
                        Console.WriteLine("Lock Checked. Ok.");
                        recording = true;
                        Console.WriteLine("Is Recording Now ... \nPress S to Stop and Esc to Exit");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            });

        //----------------------------------------------------------------------

        await socketIOClient.ConnectAsync();

        while (!finished)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        finished = true;
                        break;
                    case ConsoleKey.R:
                        if (!recording)
                        {
                            await socketIOClient.EmitAsync("start-streaming", streamAddress);
                            Console.WriteLine("Is Checking Lock ...");
                        }
                        break;
                    case ConsoleKey.S:
                        recording = false;
                        await socketIOClient.EmitAsync("stop-streaming");
                        Console.WriteLine("Recording Stoped!");
                        Console.WriteLine("R: Start Recording, S: Stop Recording, Esc: Exit");
                        break;
                    default:
                        Console.WriteLine("R: Start Recording, S: Stop Recording, Esc: Exit");
                        break;
                }

            }
        }
    }
}
