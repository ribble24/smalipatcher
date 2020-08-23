using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SmaliLib;
using SmaliLib.Patches;

namespace SmaliPatcherMin
{
    //TODO make the patches more nice (replace constant path.combine with cached value, see MockLocations)
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Select(s => s.TrimStart('-', '/')).Contains("help"))
            {
                Console.WriteLine(@"SmaliPatcher.JF min
Usage:  patcher [parameters...]

Parameters:
    help             -  displays this message
    no-download      -  does not re-download resources. This is mostly useful for testing
    framework:<dir>  -  builds based on the content of the specified directory instead of pulling
    skip-cleanup     -  prevents removal of temporary files. This is mostly useful for testing");
                return;
            }
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            IPlatform platform = new Platform();
            SmaliLibMain lib = new SmaliLibMain(platform);
            if (!args.Select(s => s.TrimStart('-', '/')).Contains("no-download"))
                lib.DownloadDeps();
            lib.CheckResources();
            int status = lib.CheckAdb();
            switch (status)
            {
                case -1:
                    platform.ErrorCritical("The connected device is not authorized. Have you accepted the popup?");
                    break;
                case 0:
                    platform.ErrorCritical("No device found! Make sure it is connected and has ADB enabled");
                    break;
                default:
                    if (status > 1)
                        platform.ErrorCritical("Too many devices connected. Use one only!");
                    else
                    {
                        bool pullFramework = !args.Any(s => s.TrimStart('-', '/').StartsWith("framework:"));
                        string framework = pullFramework
                            ? lib.DumpFramework()
                            : args.First(s => s.TrimStart('-', '/').StartsWith("framework:"))
                                .TrimStart('-', '\\').Substring(10);
                        
                        //Nice menu
                        IPatch[] available = lib.GetPatches();
                        IPatch[] selected = new [] {available.First(s => s is HighVolumeWarning), available.First(s => s is MockLocations)};
                        bool selecting = true;
                        int currentI = 0;
                        while (selecting)
                        {
                            IPatch[] current = new[] {available[currentI]};
                            //Drawing
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Clear();
                            foreach (IPatch patch in available)
                            {
                                Console.BackgroundColor = current[0] == patch ? ConsoleColor.White : ConsoleColor.Black;
                                Console.ForegroundColor = current[0] == patch ? ConsoleColor.Black : ConsoleColor.White;
                                Console.Write(selected.Contains(patch) ? "[X] " : "[ ] ");
                                Console.WriteLine(patch.Title);
                                Console.WriteLine($"    {patch.Description}");
                            }
                            //Input
                            switch (Console.ReadKey().Key)
                            {
                                case ConsoleKey.Escape:
                                    Console.ResetColor();
                                    Console.Clear();
                                    Console.WriteLine("Cancelled");
                                    return;
                                case ConsoleKey.Enter:
                                    selecting = false;
                                    break;
                                case ConsoleKey.Tab:
                                case ConsoleKey.DownArrow:
                                    currentI++;
                                    if (currentI >= available.Length)
                                        currentI = 0;
                                    break;
                                case ConsoleKey.UpArrow:
                                    currentI--;
                                    if (currentI < 0)
                                        currentI = available.Length - 1;
                                    break;
                                case ConsoleKey.LeftArrow:
                                case ConsoleKey.RightArrow:
                                case ConsoleKey.Spacebar:
                                    selected = selected.Contains(available[currentI]) ? selected.Except(current).ToArray() : selected.Concat(current).ToArray();
                                    break;
                            }
                        }
                        Console.ResetColor();
                        Console.Clear();
                        lib.PatchFramework(framework, selected);
                        lib.PackModule(selected, args.Select(s => s.TrimStart('-', '/')).Contains("skip-cleanup"), pullFramework);
                    }
                    break;
            }
            platform.Log("DONE!");
        }
    }
}
