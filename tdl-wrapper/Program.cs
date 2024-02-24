using System.Reflection;

namespace tdlWrapper;

class Program {
    static async Task Main(string[] args)
    {
        Console.WriteLine("hiii");

        string tdlUrl = GetDownloadLink();
        string tdlPath = GetTdlDownloadPath();
        await DownloadTdl(tdlUrl, tdlPath);

        string channel = GetChannelName();
        string sort = GetSorting();
        int count = GetCount();
        string type = GetContentType();
        string downloadPatch = GetContentDownloadPath();

        await DownloadContent();
    }

    private static string GetChannelName()
    {
        // a-z 0-9 _
        // min 5

        string? name = null;
        while (name is null)
        {
            Console.WriteLine("enter the link / name of the chat: ");
            string? input = Console.ReadLine();
            if (input is null) continue;
            input = input.Trim().Replace("https://t.me/", "");
            if ( input.Length < 5 || input.Length > 32) continue;
            if (!IsChannelNameValid(input)) continue;

            name = input;
        }
        return name;
    }

    private static bool IsChannelNameValid(string input)
    {
        throw new NotImplementedException();
    }

    private static string GetTdlDownloadPath()
    {
        throw new NotImplementedException();
    }

    private static async Task DownloadTdl(string tdlUrl, string tdlPath)
    {
        HttpClient client = new();
        var x = await client.GetByteArrayAsync(tdlUrl);
         // File.WriteAllBytes(, x);
    }

    private static string GetDownloadLink()
    {
        return "https://github.com/iyear/tdl/releases/download/v0.16.0/tdl_Windows_64bit.zip";
    }
}