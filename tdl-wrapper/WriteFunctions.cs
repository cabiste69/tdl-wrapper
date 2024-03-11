namespace tdlWrapper;

public class WriteFunctions
{
    public static void WriteError(string message) => Write(message, ConsoleColor.Red);

    public static void WriteWarning(string message) => Write(message, ConsoleColor.Yellow);

    private static void Write(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.White;
    }
}