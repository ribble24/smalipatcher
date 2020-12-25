using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CC_Functions.Commandline;
using SmaliLib;
using SmaliLib.Patches;

namespace SmaliPatcherMin
{
    internal class Program
    {
        private static void Main(string[] a)
        {
            ArgsParse args = new(a);
            if (!args.GetBool("no-cd"))
                Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            IPlatform platform = new Platform();
            SmaliLibMain lib = new(platform);
            if (args.GetBool("help"))
            {
                Console.WriteLine(@$"SmaliPatcher.JF min ({lib.GetVersion()})
Usage:  patcher [parameters...]

Parameters:
    help             -  displays this message
    no-download      -  does not re-download resources. This is mostly useful for testing
    framework:<dir>  -  builds based on the content of the specified directory instead of pulling
    skip-cleanup     -  prevents removal of temporary files. This is mostly useful for testing
    no-cd            -  do not change the directory to the binary. Useful for packaged installs");
                return;
            }
            if (!args.GetBool("no-download"))
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
                        string framework = args["framework"] ?? lib.DumpFramework();

                        //Nice menu
                        IPatch[] available = lib.GetPatches();
                        IPatch[] selected =
                            {available.First(s => s is HighVolumeWarning), available.First(s => s is MockLocations)};
                        bool selecting = true;
                        int currentI = 0;
                        while (selecting)
                        {
                            IPatch[] current = {available[currentI]};
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
                                    selected = selected.Contains(available[currentI])
                                        ? selected.Except(current).ToArray()
                                        : selected.Concat(current).ToArray();
                                    break;
                            }
                        }
                        Console.ResetColor();
                        Console.Clear();
                        lib.PatchFramework(framework, selected);
                        lib.PackModule(selected, args.GetBool("skip-cleanup"), args["framework"] == null);
                    }
                    break;
            }
            platform.Log("DONE!");
        }
    }
}