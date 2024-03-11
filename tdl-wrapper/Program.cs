using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace tdlWrapper;

public sealed class Program : WriteFunctions
{
    private static string? TdlExec;
    static void Main()
    {
        Thread installTdl = new(InstallTdl)
        {
            IsBackground = true
        };
        installTdl.Start();

        string[] channels = GetChannelNames();
        int count = GetCount();
        string[] types = GetContentType();
        string downloadPath = GetContentDownloadPath();

        if (TdlExec is null)
            WriteWarning("Downloading prerequisites...");
        while (TdlExec is null)
            Thread.Sleep(500);

        LoginInTdl();
        DownloadContent(channels, count, types, downloadPath);
    }

    private static async void InstallTdl()
    {
        string tdlUrl = await GetDownloadLink();
        string tdlZipPath = GetTdlDownloadPath(tdlUrl);
        await DownloadTdl(tdlUrl, tdlZipPath);
    }

    private static void DownloadContent(string[] channels, int count, string[] types, string downloadPath)
    {
        string download = $"dl --skip-same --continue -l 1 -d ";

        for (int i = 0; i < channels.Length; i++)
        {
            string jsonExtractName = Path.Combine(Path.GetDirectoryName(TdlExec)!, $"{channels[i]}.json");
            string export = $"chat export -c {channels[i]} -T last -i {count} -f \"split(Media.Name, '.') | last() | lower() in ['{string.Join("','", types)}']\" -o {jsonExtractName}";

            RunTdl(export);

            string dlCommand = download + Path.Combine(downloadPath, channels[i]) + $" -f \"{jsonExtractName}\"";

            // when downloading a lot of files (100+) tdl might crash, so we throw it in while loop until it finishes
            while (!RunTdl(dlCommand)) ;
        }
    }

    private static string GetContentDownloadPath()
    {
        while (true)
        {
            Console.Write("Enter the path where the files should be saved: ");
            string input = Console.ReadLine()!;
            try
            {
                var dir = Directory.CreateDirectory(input);
                return dir.FullName;
            }
            catch
            {
                WriteError("The provided path is invalid! please try again.");
                continue;
            }
        }
    }

    private static string[] GetContentType()
    {
        Console.Write("Enter the extensions of the files to download separated by space (default: mp4): ");

        List<string> input = Console.ReadLine()!.Trim().Replace('.', '\0').Split(' ').ToList();

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
        while (true)
        {
            Console.Write("Enter the number of files to download: ");
            try
            {
                int input = Convert.ToInt32(Console.ReadLine());
                if (input > 0) return input;
                WriteWarning("You sure you want to download 0 files?");
            }
            catch
            {
                WriteError("You can't do that!!!");
            }
        }
    }

    private static void LoginInTdl()
    {
        bool isLoggedIn = RunTdl("chat ls -f \"len(Topics)<0\"", true);
        if (isLoggedIn)
            return;

        while (!isLoggedIn)
        {
            Console.Write("Choose a login method 'qr' / 'code': ");
            string input = Console.ReadLine()!.Trim().ToLower();

            if (input != "qr" && input != "code")
            {
                WriteError("Invalid input! please try again.");
                continue;
            }
            isLoggedIn = RunTdl("login -T " + input);
        }
    }

    private static string[] GetChannelNames()
    {
        // a-z 0-9 _ +
        // min 5 / max 32

        while (true)
        {
            Console.Write("Enter the link / name of the chats separated by space: ");
            string[]? input = Console.ReadLine()!.Trim().Split(' ');

            if (!IsChannelNameValid(ref input)) continue;

            return input!;
        }
    }

    private static bool IsChannelNameValid(ref string[]? input)
    {
        if (input is null || input.Length == 0)
        {
            WriteError("You must provide at least one channel / group name!");
            return false;
        }

        for (int i = 0; i < input.Length; i++)
        {
            input[i] = input[i].Trim().Replace("https://t.me/", "");

            if (input[i].IndexOf('/') > 0)
                input[i] = input[i].Remove(input[i].IndexOf('/'));

            if (input[i].Length < 5 || input[i].Length > 32)
            {
                WriteError("One of the channel names is not between 5 and 32 characters long!");
                return false;
            }

            if (!IsValidChannelName(input[i]))
            {
                WriteError($"'{input[i]}' has an illegal character! must only contain 'a-z A-Z 0-9 _ +'");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// verifies if a given channel name is valid
    /// </summary>
    /// <param name="name">the channel name to check</param>
    /// <returns></returns>
    private static bool IsValidChannelName(string name)
    {
        // the link of a group starts with a +, other than that it's an illegal character
        if (name.IndexOf('+') > 0)
            return false;

        // Fuck regex
        for (int i = 0; i < name.Length; i++)
        {
            char s = name[i];

            //   [A-Z]                      [a-z]                  [0-9]                _          +
            if ((s < 65 || s > 90) && (s < 97 || s > 122) && (s < 48 || s > 57) && s != 95 && s != 43)
                return false;
        }
        return true;
    }

    private static async Task DownloadTdl(string tdlUrl, string tdlPath)
    {
        HttpClient client = new();
        var x = await client.GetByteArrayAsync(tdlUrl);
        File.WriteAllBytes(tdlPath, x);

        string extractPath = Path.Combine(Path.GetDirectoryName(tdlPath)!, "tdl");
        if (Path.Exists(extractPath))
            Directory.Delete(extractPath, true);

        TdlExec = Path.Combine(extractPath, "tdl");

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
            RunProcess("tar", "zxf " + tdlPath + " -C " + extractPath);
        }
    }

    private static async Task<string> GetDownloadLink()
    {
        string version = await GetLatestTdlVersion();
        string assetName = GetTdlAssetName();
        // https://github.com/iyear/tdl/releases/download/v0.16.1/tdl_Windows_64bit.zip
        return $"https://github.com/iyear/tdl/releases/download/{version}/{assetName}";
    }

    private static string GetTdlDownloadPath(string tdlUrl)
    {
        string path = Path.Combine(Path.GetTempPath(), string.Concat("", tdlUrl.AsSpan(tdlUrl.IndexOf("tdl_"))));
        if (File.Exists(path))
            File.Delete(path);
        return path;
    }

    private static string GetTdlAssetName()
    {
        string name = "tdl_";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) name += "Windows_";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) name += "Linux_";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) name += "MacOS_";
        else throw new Exception("Unknown OS!");

        name += Arch.GetArchString(RuntimeInformation.OSArchitecture);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) name += ".zip";
        else name += ".tar.gz";

        return name;
    }

    private static async Task<string> GetLatestTdlVersion()
    {
        string uri = "https://api.github.com/repos/iyear/tdl/releases/latest";
        string agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36";

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", agent);
        using HttpResponseMessage response = await client.GetAsync(uri);

        var jsonResponse = await response.Content.ReadAsStringAsync();
        return GetJsonValueByProperty(jsonResponse, "tag_name");
    }

    private static bool RunTdl(string command, bool IsHidden = false) => RunProcess(TdlExec!, command, IsHidden);
    private static bool RunProcess(string app, string command, bool IsHidden = false)
    {
        bool success = false;
        using (Process process = new())
        {
            process.StartInfo.FileName = app;
            process.StartInfo.Arguments = command;
            process.StartInfo.RedirectStandardOutput = IsHidden;
            process.Start();
            process.WaitForExit();
            success = process.ExitCode == 0;
        }
        return success;
    }

    private static string GetJsonValueByProperty(string json, string property)
    {
        int propIndex = json.IndexOf(property, StringComparison.OrdinalIgnoreCase) + property.Length;
        string value = json.Substring(propIndex+2, json.IndexOf(',', propIndex) - (propIndex+2)).Trim(); // ": "v0.16.1"
        return value.Trim('"');
    }
}
