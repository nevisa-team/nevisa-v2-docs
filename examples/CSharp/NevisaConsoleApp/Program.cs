using NAudio.Wave;
using Newtonsoft.Json.Linq;
using SocketIOClient;

static class Program
{
    private static async Task Main()
    {
        string serverAddress = "https://ent.persianspeech.com";
        string userName = "super.admin";
        string password = "123";
        string token = "";
        bool finished = false;
        bool recording = false;
        WaveInEvent waveSource = new();

        string request = $"{{\"username\": \"{userName}\", \"password\": \"{password}\" }}";

        StringContent content = new StringContent(request, System.Text.Encoding.UTF8, "application/json");

        var httpClient = new HttpClient();

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
            return;
        }

        SocketIOOptions options = new()
        {
            ExtraHeaders = new()
        {
            { "token", token },
            { "platform", "browser" }
        }
        };

        var socketIOClient = new SocketIOClient.SocketIO("https://ent.persianspeech.com/", options);

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
            Console.WriteLine(response);
        });

        socketIOClient.On("start-microphone", data =>
        {
            // You can print the returned data first to decide what to do next.
            if ((bool)JToken.Parse(JToken.Parse(data.ToString())[0].ToString())["lockChecked"])
            {
                Console.WriteLine("Lock Checked. Ok.");
                //waveSource.DeviceNumber = 0;
                waveSource.WaveFormat = new WaveFormat(16000, 1);

                waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailableAsync);

                waveSource.StartRecording();

                recording = true;

                Console.WriteLine("Is Recording Now ... \nPress S to Stop and Esc to Exit");
            }
        });


        async void waveSource_DataAvailableAsync(object sender, WaveInEventArgs e)
        {
            if (recording)
            {
                await socketIOClient.EmitAsync("microphone-blob", e.Buffer);
            }
        }

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
                            await socketIOClient.EmitAsync("start-microphone");                            
                        }
                        break;
                    case ConsoleKey.S:
                        recording = false;
                        waveSource.StopRecording();
                        await socketIOClient.EmitAsync("stop-microphone");
                        Console.WriteLine("Recording Stoped!");
                        break;
                    default:
                        Console.WriteLine("R: Start Recording, S: Stop Recording, Esc: Exit");
                        break;
                }

            }
        }
    }
}