using NevisaConsoleApp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SocketIO.Serializer.NewtonsoftJson;
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
        string userName = "soroush.gooran";
        string password = "SrgSrg4343$";
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
            { "platform", "mobile" }
        }
        };


        var socketIOClient = new SocketIOClient.SocketIO(serverAddress, options);

        socketIOClient.Serializer = new NewtonsoftJsonSerializer(new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        });

        socketIOClient.OnConnected += (sender, e) =>
        {
            Console.WriteLine("Connected!");
            Console.WriteLine("R: Start Processing, S: Stop Processing, Esc: Exit");
        };

        socketIOClient.OnDisconnected += (sender, e) =>
        {
            Console.WriteLine("Disconnected!");
        };
        
        socketIOClient.On("message", data =>
        {
            // You can print the returned data first to decide what to do next.
            Console.WriteLine(data);
        });

        socketIOClient.On("queue-report", data =>
        {
            // You can print the returned data first to decide what to do next.
            Console.WriteLine(data);

            //var json = JToken.Parse(data.ToString());

            //string status = (string)json["status"];

            //string percent = (string)json["percent"];

            //Console.WriteLine("status: {0}, percent: {1}", status, percent);
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
        var response = addFileTask.GetAwaiter().GetResult();
        //Console.WriteLine(response);
        var json = JToken.Parse(response.ToString());
        var files = json["files"];
        Console.WriteLine(files);

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
                            Console.WriteLine($"{{\"files\": {files}}}");
                            await socketIOClient.EmitAsync("start-file-process", JToken.Parse($"{{\"files\": {files}}}"));
                            Console.WriteLine("Is Checking Lock ...");
                        }
                        break;
                    case ConsoleKey.S:
                        processing = false;
                        await socketIOClient.EmitAsync("stop-file-process", JToken.Parse($"{{\"files\": {files}}}"));
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
