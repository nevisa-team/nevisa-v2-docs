using NevisaConsoleApp;
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

    static NevisaAPI nevisaAPI = new();
    //----------------------------------------------------------------------
    private static async Task Main()
    {
        string serverAddress = "https://ent.persianspeech.com";
        string fileAddress = "C:\\Users\\diatell\\Music\\test.wav";
        string userName = "super.admin";
        string password = "123";
        string token = "";
        bool finished = false;
        bool processing = false;
        Console.OutputEncoding = Encoding.Unicode;
        string code="";
        var loginTask = nevisaAPI.Login(userName, password);
        (token,code) = loginTask.GetAwaiter().GetResult();

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
            Console.WriteLine("R: Start Processing, S: Stop Processing, Esc: Exit");
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

        socketIOClient.On("start-file-process", data =>
            {
                if (data.GetValue<LockStatus>(0).lockChecked)
                {
                    try
                    {
                        Console.WriteLine("Lock Checked. Ok.");
                        processing = true;
                        Console.WriteLine("Is Processing Now ... \nPress S to Stop and Esc to Exit");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            });

        //----------------------------------------------------------------------

        await socketIOClient.ConnectAsync();

        // Add File for Recognition --------------------------------------
        var addFileTask = nevisaAPI.AddFile(token, fileAddress);
        string files;
        files = addFileTask.GetAwaiter().GetResult();

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
                        if (!processing)
                        {
                            await socketIOClient.EmitAsync("start-file-process", $"{{\"files\": \"{files}\"}}");
                            Console.WriteLine("Is Checking Lock ...");
                        }
                        break;
                    case ConsoleKey.S:
                        processing = false;
                        await socketIOClient.EmitAsync("stop-file-process", $"{{\"files\": \"{files}\"}}");
                        Console.WriteLine("Processing Stoped!");
                        Console.WriteLine("R: Start Processing, S: Stop Processing, Esc: Exit");
                        break;
                    default:
                        Console.WriteLine("R: Start Processing, S: Stop Processing, Esc: Exit");
                        break;
                }

            }
        }
    }
}
