using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace tdlWrapper;

public sealed class Program
{
    private static string? TdlExec;
    static async Task Main(string[] args)
    {
        Console.WriteLine("hiii");
        string tdlUrl = GetDownloadLink();
        string tdlZipPath = GetTdlDownloadPath(tdlUrl);
        await DownloadTdl(tdlUrl, tdlZipPath);
        LoginInTdl();

        string channel = GetChannelName();
        int count = GetCount();
        string[] types = GetContentType();
        string downloadPath = GetContentDownloadPath();

        DownloadContent(channel, count, types, downloadPath);
    }

    private static void DownloadContent(string channel, int count, string[] types, string downloadPath)
    {
        string export = $"chat export -c {channel} -T last -i {count} -f \"split(Media.Name, '.') | last() | lower() in ['{string.Join("','", types)}']\"";
        string download = $"dl -f tdl-export.json -d \"{downloadPath}\" --skip-same --continue -l 1";

        RunTdl(export);
        while(!RunTdl(download));
    }

    private static string GetContentDownloadPath()
    {
        while (true)
        {
            Console.Write("where to save the files?: ");
            string input = Console.ReadLine();
            try
            {
                var dir = Directory.CreateDirectory(input);
                return dir.FullName;
            }
            catch
            {
                Console.WriteLine("Incorrect path...");
                continue;
            }
        }
    }

    private static string[] GetContentType()
    {
        Console.Write("Enter the extensions of the files to download separated by space (default: mp4): ");

        List<string> input = Console.ReadLine().Trim().Replace('.', '\0').Split(' ').ToList();

        for (int i = 0; i < input.Count; i++)
        {
            if (input[i].Replace(' ', '\0').Length != 0)
                continue;
            input.RemoveAt(i);
            i--;
        }

        if (input.Count == 0)
            return ["mp4"];

        return input.ToArray();
    }

    private static int GetCount()
    {
        int count = 0;
        while (count <= 0)
        {
            Console.Write("how many to download? ");
            try
            {
                int input = Convert.ToInt32(Console.ReadLine());
                if (input > 0) count = input;
            }
            catch
            {
                Console.WriteLine("you can't do that!!!");
            }
        }
        return count;
    }

    private static void LoginInTdl()
    {
        bool isLoggedIn = false;
        while (!isLoggedIn)
        {
            Console.Write("choose a login method 'qr' / 'code': ");
            string input = Console.ReadLine()!.Trim();

            if (input != "qr" && input != "code")
            {
                Console.WriteLine("invalid input, please try again.");
                continue;
            }
            isLoggedIn = RunTdl("login -T " + input);
        }
    }

    private static string GetChannelName()
    {
        // a-z 0-9 _
        // min 5 / max 32

        string? name = null;
        while (name is null)
        {
            Console.WriteLine("enter the link / name of the chat: ");
            string? input = Console.ReadLine();

            if (!IsChannelNameValid(ref input)) continue;

            name = input;
        }
        return name;
    }

    private static bool IsChannelNameValid(ref string? input)
    {
        if (input is null)
        {
            Console.WriteLine("must provide a channel / group name");
            return false;
        }

        input = input.Trim().Replace("https://t.me/", "");
        if (input.IndexOf('/') > 0) input = input.Remove(input.IndexOf('/'));

        if (input.Length < 5 || input.Length > 32)
        {
            Console.WriteLine("the channel / group name must be between 5 and 32 letters long");
            return false;
        }

        return Regex.Match(input, "([A-Z_a-z+0-9])").Success; ;
    }

    private static async Task DownloadTdl(string tdlUrl, string tdlPath)
    {
        HttpClient client = new();
        var x = await client.GetByteArrayAsync(tdlUrl);
        File.WriteAllBytes(tdlPath, x);

        string extractPath = Path.Combine(Path.GetDirectoryName(tdlPath) + "/tdl");
        if (Path.Exists(extractPath))
            Directory.Delete(extractPath, true);

        TdlExec = Path.Combine(extractPath + "/tdl");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using (ZipArchive zip = ZipFile.Open(tdlPath, ZipArchiveMode.Read))
            {
                zip.ExtractToDirectory(extractPath);
            }
            TdlExec += ".exe";
        }
        else
        {
            Directory.CreateDirectory(extractPath);
            // tar -xzf
            RunProcess("tar", "zxf " + tdlPath + " -C " + extractPath);
        }
    }

    private static bool RunTdl(string command)
    {
        return RunProcess(TdlExec!, command);
    }
    private static bool RunProcess(string app, string command)
    {
        bool success = false;
        using (Process process = new())
        {
            process.StartInfo.FileName = app;
            process.StartInfo.Arguments = command;
            process.Start();
            process.WaitForExit();
            success = process.ExitCode == 0;
        }
        return success;
    }

    private static string GetDownloadLink()
    {
        return "https://github.com/iyear/tdl/releases/download/v0.16.0/tdl_Windows_64bit.zip";
        // return "https://github.com/iyear/tdl/releases/download/v0.16.0/tdl_Linux_64bit.tar.gz";
    }

    private static string GetTdlDownloadPath(string tdlUrl)
    {
        string path = Path.Combine(Path.GetTempPath() + "/", string.Concat("", tdlUrl.AsSpan(tdlUrl.IndexOf("tdl_"))));
        if (File.Exists(path))
            File.Delete(path);
        return path;
    }
}
