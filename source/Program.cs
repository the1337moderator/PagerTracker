using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

int port = GetPort(args);
List<string> buttons = LoadButtons();
object lockObject = new object();
Dictionary<string, DateTime> clickedTimes = new Dictionary<string, DateTime>();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "Pager Tracker";
});
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, port);
});

WebApplication app = builder.Build();

app.MapGet("/", () => Results.Content(PagerTrackerPage.Html, "text/html"));
app.MapGet("/api/state", () =>
{
    lock (lockObject)
    {
        return Results.Content(GetStateJson(clickedTimes), "application/json");
    }
});

app.MapPost("/api/toggle/{button}", (string button) =>
{
    if (!buttons.Contains(button))
    {
        return Results.BadRequest();
    }

    lock (lockObject)
    {
        if (clickedTimes.ContainsKey(button))
        {
            clickedTimes.Remove(button);
        }
        else
        {
            clickedTimes[button] = DateTime.Now;
        }

        return Results.Content(GetStateJson(clickedTimes), "application/json");
    }
});

Console.WriteLine($"PagerTracker is running at http://localhost:{port}");
app.Run();

string GetStateJson(Dictionary<string, DateTime> clickedTimes)
{
    StringBuilder json = new StringBuilder();
    bool first = true;

    json.Append("{\"available\":[");
    foreach (string button in buttons)
    {
        if (!clickedTimes.ContainsKey(button))
        {
            if (!first)
            {
                json.Append(',');
            }

            json.Append('"');
            json.Append(EscapeJson(button));
            json.Append('"');
            first = false;
        }
    }

    json.Append("],\"active\":[");
    first = true;

    foreach (KeyValuePair<string, DateTime> entry in clickedTimes.OrderBy(entry => entry.Value))
    {
        if (buttons.Contains(entry.Key))
        {
            if (!first)
            {
                json.Append(',');
            }

            json.Append("{\"number\":");
            json.Append('"');
            json.Append(EscapeJson(entry.Key));
            json.Append('"');
            json.Append(",\"time\":\"");
            json.Append(EscapeJson(entry.Value.ToString("M/d/yyyy h:mm:ss tt")));
            json.Append("\"}");
            first = false;
        }
    }

    json.Append("]}");
    return json.ToString();
}

static List<string> LoadButtons()
{
    string configFile = Path.Combine(AppContext.BaseDirectory, "PagerTracker.json");

    if (!File.Exists(configFile))
    {
        return new List<string> { "1", "2", "3", "5", "7", "8", "9", "10", "11" };
    }

    List<string> buttons = ParseStringArray(File.ReadAllText(configFile));

    if (buttons.Count == 0)
    {
        Console.Error.WriteLine("PagerTracker.json did not contain any button labels.");
        Environment.Exit(1);
    }

    return buttons;
}

static List<string> ParseStringArray(string json)
{
    List<string> buttons = new List<string>();
    string text = json.Trim();

    if (!text.StartsWith('[') || !text.EndsWith(']'))
    {
        Console.Error.WriteLine("PagerTracker.json must be a JSON array of strings, like [\"1\", \"2\", \"3\"].");
        Environment.Exit(1);
    }

    int i = 1;
    while (i < text.Length - 1)
    {
        while (i < text.Length - 1 && (char.IsWhiteSpace(text[i]) || text[i] == ','))
        {
            i++;
        }

        if (i >= text.Length - 1)
        {
            break;
        }

        if (text[i] != '"')
        {
            Console.Error.WriteLine("PagerTracker.json must contain only strings.");
            Environment.Exit(1);
        }

        i++;
        StringBuilder value = new StringBuilder();

        while (i < text.Length - 1)
        {
            if (text[i] == '\\' && i + 1 < text.Length - 1)
            {
                i++;
                value.Append(text[i]);
            }
            else if (text[i] == '"')
            {
                break;
            }
            else
            {
                value.Append(text[i]);
            }

            i++;
        }

        if (i >= text.Length - 1 || text[i] != '"')
        {
            Console.Error.WriteLine("PagerTracker.json has an unfinished string.");
            Environment.Exit(1);
        }

        string button = value.ToString().Trim();
        if (button.Length > 0 && !buttons.Contains(button))
        {
            buttons.Add(button);
        }

        i++;
    }

    return buttons;
}

static string EscapeJson(string value)
{
    return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}

static int GetPort(string[] args)
{
    if (args.Length == 0)
    {
        return GetDefaultPort();
    }

    string portText = args[0];

    if (args.Length >= 2 && args[0].Equals("--port", StringComparison.OrdinalIgnoreCase))
    {
        portText = args[1];
    }

    if (int.TryParse(portText, out int port) && port is >= 1 and <= 65535)
    {
        return port;
    }

    Console.Error.WriteLine("Invalid port. Use a number from 1 to 65535, for example: PagerTracker.exe 8080");
    Environment.Exit(1);
    return 80;
}

static int GetDefaultPort()
{
    if (CanListenOn(80))
    {
        return 80;
    }

    for (int port = 5000; port <= 5099; port++)
    {
        if (CanListenOn(port))
        {
            return port;
        }
    }

    Console.Error.WriteLine("No available default port was found. Tried 80 and 5000 through 5099.");
    Console.Error.WriteLine("Run with a custom port, for example: PagerTracker.exe 8080");
    Environment.Exit(1);
    return 80;
}

static bool CanListenOn(int port)
{
    try
    {
        using TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        return true;
    }
    catch (SocketException)
    {
        return false;
    }
}
