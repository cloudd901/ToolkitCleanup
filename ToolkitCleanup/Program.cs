using Microsoft.Win32;
using PCAFFINITY;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ToolkitCleanup
{
    internal static class Program
    {
        public static bool Automate;
        public static int CleanType;
        public static ImpersonationContext CompImpersonationContext = null;
        public static string Computer;

        public static string[] Creds = new string[2]
        {
            null,
            null
        };

        public static string[] FolderListUser = new string[31]
        {
            @"C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Teams\tmp\*",
            @"C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Teams\blob_storage\*",
            @"C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Teams\Cache\*",
            @"C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Teams\IndexedDB\*",
            @"C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Teams\GPUCache\*",
            @"C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Teams\databases\*",
            @"C:\Users\<USERNAME>\AppData\Roaming\Microsoft\Software Center\.cache\*",
            @"C:\Users\<USERNAME>\AppData\Local\Temp\*",
            @"C:\Users\<USERNAME>\AppData\Local\Cisco\Unified Communications\Jabber\CSF\cef_cache\*",
            @"C:\Users\<USERNAME>\AppData\Local\Cisco\Unified Communications\Jabber\CSF\Photo Cache\*",
            @"C:\Users\<USERNAME>\AppData\Local\Cisco\Unified Communications\Jabber\CSF\IMP Cache\*",
            @"C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\Caches\*",
            @"C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\Explorer\iconcache*.*",
            @"C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\Explorer\thumbcache*.*",
            @"C:\Users\<USERNAME>\AppData\Local\IconCache.db",
            @"C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\Temporary Internet\*",//Added for pre 8.0
            @"C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\Cookies\*",//Added for pre 8.0
            @"C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\IECompatCache\*",
            @"C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\IECompatUaCache\*",
            @"C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\IEDownloadHistory\*",
            @"C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\INetCookies\*",
            @"C:\Users\<USERNAME>\AppData\Local\Microsoft\Windows\INetCache\*",
            @"C:\Users\<USERNAME>\AppData\Local\Google\Chrome\User Data\Default\Cache\*",
            @"C:\Users\<USERNAME>\AppData\Local\Google\Chrome\User Data\Default\Cookies\*",
            @"C:\Users\<USERNAME>\AppData\Local\Google\Chrome\User Data\Default\History\*",
            @"C:\Users\<USERNAME>\AppData\Local\Packages\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\AC\*",
            @"C:\Users\<USERNAME>\AppData\Local\Packages\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\LocalCache\*",
            @"C:\Users\<USERNAME>\AppData\Local\Packages\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\AppData\User\Default\CacheStorage\*",
            @"C:\Users\<USERNAME>\AppData\Local\Chromium\User Data\Default\Cache\*",
            @"C:\Users\<USERNAME>\AppData\Local\Chromium\User Data\Default\GPUCache\*",
            @"C:\Users\<USERNAME>\AppData\Local\Chromium\User Data\ShaderCache\*"
        };

        public static string[] FolderListWin = new string[16]
        {
            @"C:\temp\*",
            @"C:\Windows\Downloaded Program Files\*",
            @"C:\Windows\LiveKernelReports\*",
            @"C:\Windows\Prefetch\*.pf",
            @"C:\Windows\Temp\*",
            @"C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache\*",
            @"C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Logs\*",
            @"C:\JavaCache\*",
            @"C:\Program Files (x86)\Google\Temp\*",
            @"C:\Program Files (x86)\Google\Update\Download\*",
            @"C:\Program Files (x86)\Google\UpdateD\Download\*",
            @"C:\ProgramData\USOShared\Logs\*",
            @"C:\ProgramData\Microsoft\Windows\Caches\*",
            @"C:\Windows\SoftwareDistribution\Download\*",
            @"C:\Windows\SoftwareDistribution\EventCache.v2\*",
            @"C:\Windows\SoftwareDistribution\PostRebootEventCache.V2\*",
        };

        public static bool Individual;
        public static int MaxAgeMonths = 2;
        public static ProfileCompareType ProfileCompare = ProfileCompareType.NTUserWithDesktopAndAPPData;
        public static int TotalFileThreads = 15;
        public static int TotalFolderThreads = 5;
        public static float TotalMB;

        private static void Main(string[] args)
        {
            Console.SetWindowSize(78, 35);
            Console.SetBufferSize(78, Console.BufferHeight);

            WriteHeader();

            if (args.Length > 0)
            {
                foreach (string arg in args)
                {
                    bool valid = true;
                    if (arg.Contains("-?", StringComparison.CurrentCultureIgnoreCase) || arg.Contains("/?", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Console.WriteLine("Toolkit Cleanup Help.");
                        Console.WriteLine("");
                        Console.WriteLine("Command Line Switches:");
                        Console.WriteLine("-?");
                        Console.WriteLine("-Computer:<computername>");
                        Console.WriteLine("-User:<username>");
                        Console.WriteLine("-Pass:<password>");
                        Console.WriteLine("-Clean <cleanType>");
                        Console.WriteLine("<CleanType>:0 = Main Menu Choices,");
                        Console.WriteLine("<CleanType>:1 = Full Auto Clean, 2 = Full Prompt Clean, 3 = Old Profile Removal w/Prompt,");
                        Console.WriteLine("<CleanType>:4 = PC Temp Cleanup, 5 = User Temp Cleanup, 6 = PC & User Temp Cleanup");
                        Console.WriteLine("<CleanType>:7 = Check & Repair Profile Structure");
                        Console.WriteLine("");
                        Console.WriteLine("Example 1: Cleanup all old profiles from current computer.");
                        Console.WriteLine("ToolkitCleanup.exe -Clean:1");
                        Console.WriteLine("");
                        Console.WriteLine("Example 2: Cleanup PC Temp files from remote computer.");
                        Console.WriteLine("ToolkitCleanup.exe -Computer:computername -Clean:4 -User:myname -Pass:mypass");
                        Console.WriteLine("");
                        valid = false;
                    }
                    else if (arg.Contains("-Clean", StringComparison.CurrentCultureIgnoreCase) || arg.Contains("/Clean", StringComparison.CurrentCultureIgnoreCase))
                    {
                        try
                        {
                            CleanType = int.Parse(arg.Split(':')[1]);
                        }
                        catch
                        {
                            Console.WriteLine(" - Invalid Clean parameter.");
                            valid = false;
                        }
                    }
                    else if (arg.Contains("-Computer", StringComparison.CurrentCultureIgnoreCase) || arg.Contains("/Computer", StringComparison.CurrentCultureIgnoreCase))
                    {
                        try
                        {
                            Computer = arg.Split(':')[1];
                        }
                        catch
                        {
                            Console.WriteLine(" - Invalid Computer parameter.");
                            valid = false;
                        }
                    }
                    else if (arg.Contains("-User", StringComparison.CurrentCultureIgnoreCase) || arg.Contains("/User", StringComparison.CurrentCultureIgnoreCase))
                    {
                        try
                        {
                            Creds[0] = arg.Split(':')[1];
                        }
                        catch
                        {
                            Console.WriteLine(" - Invalid Username parameter.");
                            valid = false;
                        }
                    }
                    else if (arg.Contains("-Pass", StringComparison.CurrentCultureIgnoreCase) || arg.Contains("/Pass", StringComparison.CurrentCultureIgnoreCase))
                    {
                        try
                        {
                            Creds[1] = Crypto.EncryptStringAES(arg.Split(':')[1], Environment.UserName);
                        }
                        catch
                        {
                            Console.WriteLine(" - Invalid Password parameter.");
                            valid = false;
                        }
                    }

                    if (!valid)
                    {
                        Environment.Exit(0);
                    }
                }
            }

            WelcomeText();

            if (CleanType == 0)
            {
                MenuLoad();
            }
            else if (CleanType == 1)
            {
                Automate = true;
                FolderTempPCCleanup();
                FolderProfileCleanup();
                FolderTempUserCleanup();
            }
            else if (CleanType == 2)
            {
                Automate = false;
                FolderTempPCCleanup();
                FolderProfileCleanup();
                FolderTempUserCleanup();
            }
            else if (CleanType == 3)
            {
                Automate = false;
                FolderProfileCleanup();
            }
            else if (CleanType == 4)
            {
                Automate = false;
                FolderTempPCCleanup();
            }
            else if (CleanType == 5)
            {
                Automate = false;
                FolderTempUserCleanup();
            }
            else if (CleanType == 6)
            {
                Automate = false;
                FolderTempPCCleanup();
                FolderTempUserCleanup();
            }
            else if (CleanType == 7)
            {
                Automate = true;
                RepairUserDirectory();
            }

            char ans;
            ConsoleWriter cw = new ConsoleWriter();
            DirectoryInfo tempFolder = new DirectoryInfo(@$"\\{Computer}\c$\Users\Temp");
            if (tempFolder.Exists && CleanType != 7)
            {
                DirectoryInfo[] tdi = tempFolder.GetDirectories("*", SearchOption.TopDirectoryOnly);
                if (tdi.Length > 0)
                {
                    if (CleanType == 0)
                    {
                        cw.WriteLineAnimated("")
                            .WriteLineAnimated("Remaining Temp User Files found. Cleanup Now? (y/n)", 10, ConsoleColor.Yellow, ConsoleColor.Blue)
                            .WriteAnimated("(y)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);

                        while (Console.KeyAvailable)
                        {
                            Console.ReadKey(false);
                        }

                        ans = Console.ReadKey(true).KeyChar;
                    }
                    else
                    {
                        ans = 'y';
                    }

                    if (string.Equals(ans.ToString(), "y", StringComparison.OrdinalIgnoreCase) || char.IsWhiteSpace(ans))
                    {
                        bool test = CleanupDirectoryQuick(tempFolder);
                        if (!test)
                        {
                            Console.WriteLine($"Unable to remove all files in {tempFolder.FullName}.");
                        }
                    }
                }
                else
                {
                    try
                    {
                        tempFolder.Delete();
                    }
                    catch
                    {
                    }
                }
            }

            if (Computer != Environment.MachineName && !string.IsNullOrWhiteSpace(Creds[0]) && !string.IsNullOrWhiteSpace(Creds[1]))
            {
                RemoveConnection();
            }

            if (CleanType != 7)
            {
                cw.WriteLineAnimated("")
                    .WriteAnimated("Cleaned ", 10, ConsoleColor.Green, ConsoleColor.Blue, AnimationType.SlideIn)
                    .WriteAnimated($"{TotalMB:n2}MB", 10, ConsoleColor.White, ConsoleColor.Blue, AnimationType.SlideIn)
                    .WriteLineAnimated(".", 0, ConsoleColor.Green, ConsoleColor.Blue, AnimationType.SlideIn);
            }

            cw.WriteLineAnimated("")
                .WriteLineAnimated("Restart is recommended after cleanings.", 10, ConsoleColor.Red, ConsoleColor.Blue);

            cw.WriteAnimated("Press ", 10, ConsoleColor.Green, ConsoleColor.Blue)
                .WriteAnimated("Any Key", 10, ConsoleColor.White, ConsoleColor.Blue)
                .WriteLineAnimated(" to exit.", 10, ConsoleColor.Green, ConsoleColor.Blue);

            while (Console.KeyAvailable)
            {
                Console.ReadKey(false);
            }

            Console.ReadKey();
        }

        #region Console Text

        public static bool StopSequenceThread { get; set; }

        public static string[] CredentialPrompt()
        {
            string pass = string.Empty;
            string user;
            if (Creds[0] == null)
            {
                Console.WriteLine("Please enter your Admin Username");
                user = Console.ReadLine();
            }
            else
            {
                user = Creds[0];
            }

            if (Creds[1] == null)
            {
                Console.WriteLine("Please enter your Admin Password");
                ConsoleKey key;
                do
                {
                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(false);
                    }
                    var keyInfo = Console.ReadKey(true);
                    key = keyInfo.Key;

                    if (key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        Console.Write("\b \b");
                        pass = pass.Substring(0, pass.Length - 1);
                    }
                    else if (!char.IsControl(keyInfo.KeyChar))
                    {
                        Console.Write("*");
                        pass += keyInfo.KeyChar;
                    }
                } while (key != ConsoleKey.Enter);

                pass = Crypto.EncryptStringAES(pass, Environment.UserName);
            }
            else
            {
                pass = Creds[1];
            }

            return new string[2] { user, pass };
        }

        public static Thread ThreadSequenceCurrentLineText(string phrase, char repeater, int length)
        {
            return new Thread(() =>
            {
                int counter = 0;
                while (Thread.CurrentThread.IsBackground && !StopSequenceThread)
                {
                    string v = new string(repeater, counter) + new string(' ', length - counter);
                    Console.Write($"\r{phrase}{v}");
                    Thread.Sleep(1000);
                    counter++;
                    if (counter > length)
                    {
                        counter = 0;
                    }
                }
            })
            { IsBackground = true };
        }

        public static void WelcomeText(bool newComputer = false)
        {
            char ans;
            ConsoleWriter cw = new ConsoleWriter();
            if (newComputer)
            {
                Console.Clear();
                WriteHeader(false);
                RemoveConnection();
                Computer = null;
            }

            if (Computer == null)
            {
                if (!newComputer)
                {
                    cw.WriteLineAnimated("Welcome to the Computer Toolkit Cleanup.", 10, ConsoleColor.Green, ConsoleColor.Red)
                        .WriteLineAnimated("")
                        .WriteLineAnimated("Are you cleaning THIS computer? (y/n)", 10, ConsoleColor.Yellow, ConsoleColor.Blue)
                        .WriteAnimated("(y)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);

                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                    }

                    ans = Console.ReadKey(false).KeyChar;
                }
                else
                {
                    ans = 'n';
                }

                if (char.ToUpperInvariant(ans) == char.ToUpperInvariant('y') || char.IsWhiteSpace(ans))
                {
                    Computer = Environment.MachineName;
                }
                else
                {
                    cw.WriteLineAnimated("").WriteLineAnimated("")
                        .WriteLineAnimated("Please enter the computer name.", 25, ConsoleColor.Yellow, ConsoleColor.Blue)
                        .WriteAnimated($"({Environment.MachineName})", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);

                    Computer = Console.ReadLine().Trim();
                    if (string.IsNullOrWhiteSpace(Computer))
                    {
                        Computer = Environment.MachineName;
                    }
                    else
                    {
                        cw.WriteLineAnimated("")
                            .WriteAnimated("Is this correct? Computer: ", 10, ConsoleColor.Yellow, ConsoleColor.Blue)
                            .WriteAnimated(Computer, 10, ConsoleColor.White, ConsoleColor.Blue)
                            .WriteLineAnimated(" (y/n)", 10, ConsoleColor.Yellow, ConsoleColor.Blue)
                            .WriteAnimated("(y)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);

                        while (Console.KeyAvailable)
                        {
                            Console.ReadKey(true);
                        }

                        ans = Console.ReadKey(false).KeyChar;
                        if (char.ToUpperInvariant(ans) != char.ToUpperInvariant('y') && !char.IsWhiteSpace(ans))
                        {
                            WelcomeText(true);
                            return;
                        }
                    }
                }
            }

            if (Computer != Environment.MachineName && string.IsNullOrWhiteSpace(Creds[0]) && string.IsNullOrWhiteSpace(Creds[1]))
            {
                cw.WriteLineAnimated("")
                    .WriteLineAnimated("Use alternate credentials? (y/n)", 10, ConsoleColor.Yellow, ConsoleColor.Blue)
                    .WriteAnimated("(y)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(false);
                }

                ans = Console.ReadKey(true).KeyChar;
                if (string.Equals(ans.ToString(), "y", StringComparison.OrdinalIgnoreCase) || char.IsWhiteSpace(ans))
                {
                    Console.WriteLine("");
                    Console.WriteLine("");
                    Creds = CredentialPrompt();
                    Console.WriteLine("");
                    CompImpersonationContext = new ImpersonationContext(Environment.UserDomainName, Creds[0], Crypto.DecryptStringAES(Creds[1], Environment.UserName));
                    CompImpersonationContext?.Enter();
                    ExecuteCommand($"Net Use \\\\{Computer}\\c$ /user:\"{ Environment.UserDomainName}\\{Creds[0]}\" \"{Crypto.DecryptStringAES(Creds[1], Environment.UserName)}\"", true);
                }
                else
                {
                    ExecuteCommand($"Net Use \\\\{Computer}\\c$", true);
                }
            }
        }

        private static string CreateStringLimit(string s, int count, bool showEnd)
        {
            if (s.Length > count)
            {
                if (showEnd)
                {
                    s = "..." + s.Substring(s.Length - count + 3, count - 3);
                }
                else
                {
                    s = s.Substring(0, count - 3) + "...";
                }
            }

            return s;
        }

        private static void MenuLoad(int speed = 4)
        {
            char ans;
            int fixIssues = 0;

            Console.Clear();

            WriteHeader(false, ConsoleColor.DarkBlue);

            using ConsoleWriter cw = new ConsoleWriter();
            cw.AniTextColorExplicit = ConsoleColor.Green;
            cw.TextColorExplicit = ConsoleColor.White;

            cw.WriteAnimated($"Toolkit connected to ", speed, ConsoleColor.Green)
                .WriteAnimated($"{Computer}", speed, ConsoleColor.Yellow)
                .WriteAnimated($" as ", speed, ConsoleColor.Green)
                .WriteAnimated($"{(string.IsNullOrWhiteSpace(Creds[0]) ? Environment.UserName : Creds[0])}", speed, ConsoleColor.Yellow)
                .WriteLineAnimated($".", speed, ConsoleColor.Green)

                .WriteLineAnimated("Please choose an option to clean.", speed)
                .WriteLineAnimated("", 0)
                .WriteAnimated("1) - ", speed).WriteLineAnimated("Full cleanup - All options.", speed, ConsoleColor.Blue)
                .WriteAnimated("2) - ", speed).WriteLineAnimated("Cleanup App/Windows Temporary files.", speed, ConsoleColor.Blue)
                .WriteAnimated("3) - ", speed).WriteLineAnimated("Remove old User Profiles.", speed, ConsoleColor.Blue)
                .WriteAnimated("4) - ", speed).WriteLineAnimated("Cleanup User Temporary files.", speed, ConsoleColor.Blue)
                .WriteLineAnimated("", 0)

                .WriteAnimated("5) - ", speed).WriteLineAnimated("Repair User Directories/Registry Keys.", speed, ConsoleColor.Magenta)
                .WriteLineAnimated("", 0)
                .WriteAnimated("6) ", speed).WriteLineAnimated("Modify settings.", speed, ConsoleColor.Yellow)
                .WriteAnimated("9) ", speed).WriteLineAnimated("Change Computer.", speed, ConsoleColor.Yellow)

                .WriteLineAnimated("0) Exit.", speed)
                .WriteLineAnimated("", 0)
                .WriteAnimated("(1)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);

            while (Console.KeyAvailable)
            {
                Console.ReadKey(false);
            }

            ans = Console.ReadKey(false).KeyChar;
            Console.Clear();
            WriteHeader(false, ConsoleColor.DarkBlue);
            if (ans == '1' || char.IsWhiteSpace(ans))
            {
                cw.WriteLineAnimated("")
                    .WriteLineAnimated("Full cleanup selected. Continue? y/n", 10, ConsoleColor.Yellow, ConsoleColor.Blue)
                    .WriteAnimated("(y)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(false);
                }

                ans = Console.ReadKey(false).KeyChar;
                if (char.ToUpperInvariant(ans) == char.ToUpperInvariant('y') || char.IsWhiteSpace(ans))
                {
                    Console.Clear();
                    WriteHeader(false, ConsoleColor.DarkBlue);
                    FolderTempPCCleanup();
                    Console.Clear();
                    WriteHeader(false, ConsoleColor.DarkBlue);
                    FolderProfileCleanup();
                    Console.Clear();
                    WriteHeader(false, ConsoleColor.DarkBlue);
                    FolderTempUserCleanup();
                }
            }
            else if (ans == '2')
            {
                cw.WriteLineAnimated("")
                    .WriteLineAnimated("App/Windows Temporary files selected. Continue? y/n", 10, ConsoleColor.Yellow, ConsoleColor.Blue)
                    .WriteAnimated("(y)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(false);
                }

                ans = Console.ReadKey(false).KeyChar;
                if (char.ToUpperInvariant(ans) == char.ToUpperInvariant('y') || char.IsWhiteSpace(ans))
                {
                    Console.Clear();
                    WriteHeader(false, ConsoleColor.DarkBlue);
                    FolderTempPCCleanup();
                }
            }
            else if (ans == '3')
            {
                cw.WriteLineAnimated("")
                    .WriteLineAnimated("Remove old User Profiles selected. Continue? y/n", 10, ConsoleColor.Yellow, ConsoleColor.Blue)
                    .WriteAnimated("(y)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(false);
                }

                ans = Console.ReadKey(false).KeyChar;
                if (char.ToUpperInvariant(ans) == char.ToUpperInvariant('y') || char.IsWhiteSpace(ans))
                {
                    Console.Clear();
                    WriteHeader(false, ConsoleColor.DarkBlue);
                    FolderProfileCleanup();
                }
            }
            else if (ans == '4')
            {
                cw.WriteLineAnimated("")
                    .WriteLineAnimated("Cleanup User Temporary files selected. Continue? y/n", 10, ConsoleColor.Yellow, ConsoleColor.Blue)
                    .WriteAnimated("(y)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(false);
                }

                ans = Console.ReadKey(false).KeyChar;
                if (char.ToUpperInvariant(ans) == char.ToUpperInvariant('y') || char.IsWhiteSpace(ans))
                {
                    Console.Clear();
                    WriteHeader(false, ConsoleColor.DarkBlue);
                    FolderTempUserCleanup();
                }
            }
            else if (ans == '5')
            {
                Console.Clear();
                WriteHeader(false, ConsoleColor.DarkMagenta);
                fixIssues = RepairUserDirectory();
            }
            else if (ans == '0')
            {
                return;
            }
            else if (ans == '6')
            {
                MenuSettings();
            }
            else if (ans == '9')
            {
                WelcomeText(true);
            }

            //-------------------------------

            if (ans == '5')
            {
                cw.WriteLineAnimated("", 0);

                cw.WriteAnimated("Repair Complete. Attempted to fix ", 25, ConsoleColor.Green, ConsoleColor.Blue, AnimationType.SlideIn)
                    .WriteAnimated(fixIssues.ToString(), 5, ConsoleColor.White, ConsoleColor.Blue, AnimationType.SlideIn)
                    .WriteLineAnimated(" issues.", 5, ConsoleColor.Green, ConsoleColor.Blue, AnimationType.SlideIn);

                cw.WriteAnimated("Press ", 5, ConsoleColor.Green, ConsoleColor.Blue, AnimationType.SlideIn)
                    .WriteAnimated("Any Key", 5, ConsoleColor.White, ConsoleColor.Blue, AnimationType.SlideIn)
                    .WriteLineAnimated(" to return to menu.", 10, ConsoleColor.Green, ConsoleColor.Blue, AnimationType.SlideIn);

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(false);
                }

                Console.ReadKey(true);
            }
            else if (ans != '6' && ans != '9' && char.ToUpperInvariant(ans) != char.ToUpperInvariant('n'))
            {
                cw.WriteLineAnimated("", 0);

                cw.WriteAnimated("Cleaning Complete. Total cleaned this session: ", 25, ConsoleColor.Green, ConsoleColor.Blue, AnimationType.SlideIn)
                    .WriteLineAnimated($"{TotalMB:n2}MB", 5, ConsoleColor.White, ConsoleColor.Blue, AnimationType.SlideIn);

                cw.WriteAnimated("Press ", 5, ConsoleColor.Green, ConsoleColor.Blue, AnimationType.SlideIn)
                    .WriteAnimated("Any Key", 5, ConsoleColor.White, ConsoleColor.Blue, AnimationType.SlideIn)
                    .WriteLineAnimated(" to return to menu.", 10, ConsoleColor.Green, ConsoleColor.Blue, AnimationType.SlideIn);

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(false);
                }

                Console.ReadKey(true);
            }

            MenuLoad();
        }

        private static void MenuSettings(int speed = 4)
        {
            char ans;

            Console.Clear();

            WriteHeader(false, ConsoleColor.DarkMagenta);

            using ConsoleWriter cw = new ConsoleWriter();
            cw.AniTextColorExplicit = ConsoleColor.Green;
            cw.TextColorExplicit = ConsoleColor.White;

            cw.WriteAnimated("User Profile max age is set to [", speed)
                .WriteAnimated(MaxAgeMonths.ToString(), 0, ConsoleColor.Yellow)
                .WriteLineAnimated("] months.", speed);
            cw.WriteAnimated("User Profile max age gathered using [", speed)
                .WriteAnimated(ProfileCompareToString(ProfileCompare), 0, ConsoleColor.Yellow)
                .WriteLineAnimated("].", speed);
            cw.WriteAnimated("File Delete thread count set to [", speed)
                .WriteAnimated(TotalFileThreads.ToString(), 0, ConsoleColor.Yellow)
                .WriteLineAnimated("] threads.", speed);
            cw.WriteAnimated("Folder Delete thread count set to [", speed)
                .WriteAnimated(TotalFolderThreads.ToString(), 0, ConsoleColor.Yellow)
                .WriteLineAnimated("] threads.", speed);
            cw.WriteAnimated("Automation - Cleaning [", speed)
                .WriteAnimated($"WILL", 0, ConsoleColor.Yellow)
                .WriteAnimated(Automate ? " NOT" : "", 0, ConsoleColor.Red)
                .WriteLineAnimated("] prompt for input.", speed);

            cw.WriteLineAnimated("", 0)
                .WriteAnimated("1) ", speed).WriteLineAnimated("Change Max Profile Age.", speed, ConsoleColor.Green)
                .WriteAnimated("2) ", speed).WriteLineAnimated("Change how Max Profile Age is gathered.", speed, ConsoleColor.Green)
                .WriteLineAnimated("", 0)
                .WriteAnimated("3) ", speed).WriteLineAnimated("Change File Threads.", speed, ConsoleColor.Green)
                .WriteAnimated("4) ", speed).WriteLineAnimated("Change Folder Threads.", speed, ConsoleColor.Green)
                .WriteLineAnimated("", 0)
                .WriteAnimated("5) ", speed).WriteLineAnimated("Toggle automation setting.", speed, ConsoleColor.Green)
                .WriteLineAnimated("", 0)
                .WriteAnimated("6) ", speed).WriteLineAnimated("Modify locations to clean. (Not Implemented)", speed, ConsoleColor.Green)
                .WriteLineAnimated("", 0)
                .WriteLineAnimated("0) Return.", speed)
                .WriteLineAnimated("", 0)
                .WriteAnimated("(0)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);

            while (Console.KeyAvailable)
            {
                Console.ReadKey(false);
            }

            ans = Console.ReadKey(false).KeyChar;
            if (string.Equals(ans.ToString(), "0") || char.IsWhiteSpace(ans))
            {
                return;
            }
            else if (string.Equals(ans.ToString(), "1"))
            {
                cw.WriteLineAnimated("", 0)
                    .WriteLineAnimated("", 0)
                    .WriteLineAnimated("Set the max age of User Profiles to keep:", 5, ConsoleColor.Blue)
                    .WriteAnimated("(2)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);
                while (Console.KeyAvailable)
                {
                    Console.ReadKey(false);
                }

                string threads = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(threads))
                {
                    threads = "2";
                }

                if (int.TryParse(threads, out int newAge))
                {
                    MaxAgeMonths = newAge;
                    cw.WriteLineAnimated("", 0)
                    .WriteAnimated("Max age set to ", 5, ConsoleColor.Blue)
                    .WriteAnimated(MaxAgeMonths.ToString(), 5, ConsoleColor.Yellow)
                    .WriteLineAnimated(" months.", 5, ConsoleColor.Blue);
                    Thread.Sleep(1200);
                }
                else
                {
                    cw.WriteLineAnimated("", 0)
                    .WriteAnimated("Number was invalid. Continuing with current max age. ", 5, ConsoleColor.Blue)
                    .WriteAnimated(MaxAgeMonths.ToString(), 5, ConsoleColor.Yellow)
                    .WriteLineAnimated(" months.", 5, ConsoleColor.Blue);
                    Thread.Sleep(1200);
                }
            }
            else if (string.Equals(ans.ToString(), "2"))
            {
                cw.WriteLineAnimated("", 0)
                    .WriteLineAnimated("", 0)
                    .WriteLineAnimated("Set how max age of User Profiles is determined:", 5, ConsoleColor.Blue)
                    .WriteLineAnimated("", 0)
                    .WriteAnimated("1) ", 0).WriteLineAnimated(ProfileCompareToString(ProfileCompareType.NTUserWithDesktopAndAPPData), 5, ConsoleColor.Blue)
                    .WriteAnimated("2) ", 0).WriteLineAnimated(ProfileCompareToString(ProfileCompareType.NTUser), 5, ConsoleColor.Blue)
                    .WriteAnimated("3) ", 0).WriteLineAnimated(ProfileCompareToString(ProfileCompareType.DesktopOnly), 5, ConsoleColor.Blue)
                    .WriteAnimated("(1)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);
                while (Console.KeyAvailable)
                {
                    Console.ReadKey(false);
                }

                ans = Console.ReadKey(false).KeyChar;
                if (ans == '1' || char.IsWhiteSpace(ans))
                {
                    ProfileCompare = ProfileCompareType.NTUserWithDesktopAndAPPData;
                }
                else if (ans == '2')
                {
                    ProfileCompare = ProfileCompareType.NTUser;
                }
                else if (ans == '3')
                {
                    ProfileCompare = ProfileCompareType.DesktopOnly;
                }

                cw.WriteLineAnimated("", 0)
                    .WriteLineAnimated("", 0)
                    .WriteAnimated("User Profiles is determined using ", 5, ConsoleColor.Blue)
                    .WriteAnimated(ProfileCompareToString(ProfileCompare), 5, ConsoleColor.Yellow)
                    .WriteLineAnimated(".", 5, ConsoleColor.Blue);
                Thread.Sleep(1200);
            }
            else if (string.Equals(ans.ToString(), "3"))
            {
                cw.WriteLineAnimated("", 0)
                    .WriteLineAnimated("", 0)
                    .WriteLineAnimated("Set the number of File Delete threads:", 5, ConsoleColor.Blue)
                    .WriteAnimated("(15)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);
                while (Console.KeyAvailable)
                {
                    Console.ReadKey(false);
                }

                string threads = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(threads))
                {
                    threads = "15";
                }

                if (int.TryParse(threads, out int newThreads))
                {
                    TotalFileThreads = newThreads;
                    cw.WriteLineAnimated("", 0)
                        .WriteAnimated("File thread count set to ", 5, ConsoleColor.Blue)
                        .WriteAnimated(TotalFileThreads.ToString(), 5, ConsoleColor.Yellow)
                        .WriteLineAnimated(".", 5, ConsoleColor.Blue);
                    Thread.Sleep(1200);
                }
                else
                {
                    cw.WriteLineAnimated("", 0)
                        .WriteAnimated("Number was invalid. Continuing with default File thread count. (", 5, ConsoleColor.Blue)
                        .WriteAnimated(TotalFileThreads.ToString(), 5, ConsoleColor.Yellow)
                        .WriteLineAnimated(")", 5, ConsoleColor.Blue);
                    Thread.Sleep(1200);
                }
            }
            else if (string.Equals(ans.ToString(), "4"))
            {
                cw.WriteLineAnimated("", 0)
                    .WriteLineAnimated("", 0)
                    .WriteLineAnimated("Set the number of Folder Delete threads:", 5, ConsoleColor.Blue)
                    .WriteAnimated("(5)", 0, ConsoleColor.DarkGray).WriteAnimated(":", 0);
                while (Console.KeyAvailable)
                {
                    Console.ReadKey(false);
                }

                string threads = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(threads))
                {
                    threads = "5";
                }

                if (int.TryParse(threads, out int newThreads))
                {
                    TotalFolderThreads = newThreads;
                    cw.WriteLineAnimated("", 0)
                        .WriteAnimated("Folder thread count set to ", 5, ConsoleColor.Blue)
                        .WriteAnimated(TotalFolderThreads.ToString(), 5, ConsoleColor.Yellow)
                        .WriteLineAnimated(".", 5, ConsoleColor.Blue);
                    Thread.Sleep(1200);
                }
                else
                {
                    cw.WriteLineAnimated("", 0)
                        .WriteAnimated("Number was invalid. Continuing with default Folder thread count. (", 5, ConsoleColor.Blue)
                        .WriteAnimated(TotalFolderThreads.ToString(), 5, ConsoleColor.Yellow)
                        .WriteLineAnimated(")", 5, ConsoleColor.Blue);
                    Thread.Sleep(1200);
                }
            }
            else if (string.Equals(ans.ToString(), "5"))
            {
                Automate = !Automate;
            }
            else if (string.Equals(ans.ToString(), "6"))
            {
            }

            MenuSettings(0);
        }

        private static string ProfileCompareToString(ProfileCompareType type)
        {
            string profdate = "";

            if (type == ProfileCompareType.DesktopOnly)
            {
                profdate = "Desktop(LastWrite)";
            }
            else if (type == ProfileCompareType.NTUser)
            {
                profdate = "NTUser.DAT";
            }
            else if (type == ProfileCompareType.NTUserWithDesktopAndAPPData)
            {
                profdate = "NTUser.DAT/Desktop/AppData";
            }

            return profdate;
        }

        private static void WriteHeader(bool animate = true, ConsoleColor backgroundColor = ConsoleColor.DarkMagenta)
        {
            using ConsoleWriter cw = new ConsoleWriter();
            cw.BackgroundColor = backgroundColor;
            cw.GarbledCharacterString = "#";

            string appname = "Toolkit Cleanup";
            string byname = " - by Derrick Ducote";
            string v = Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(0, 5);
            ConsoleColor mainColor = ConsoleColor.White;
            ConsoleColor titlecolor = ConsoleColor.Yellow;

            int padd = ((Console.BufferWidth - 1) / 2) + ((appname.Length + byname.Length) / 2) - (byname.Length);
            int padd2 = ((Console.BufferWidth - 1) / 2) + ((byname.Length + byname.Length) / 2) - (byname.Length);
            string nameText = appname.PadLeft(padd) + byname.PadRight(padd2);

            cw.WriteLineAnimated(nameText, animate ? 30 : 0, mainColor, ConsoleColor.Black, AnimationType.GarbledRandomWhiteSpace);

            padd = ((Console.BufferWidth - 1) / 2) + (("Version - ".Length + v.Length) / 2) - (v.Length);
            padd2 = ((Console.BufferWidth - 1) / 2) + ((v.Length + v.Length) / 2) - (v.Length) - 2;
            string versionText = "Version - ".PadLeft(padd) + v.PadRight(padd2);

            cw.WriteLineAnimated(versionText, animate ? 30 : 0, titlecolor, ConsoleColor.Black, AnimationType.GarbledRandomWhiteSpace);

            Console.WriteLine("");
        }

        #endregion Console Text

        #region Cleanup Methods

        private static bool CleanupDirectoryDetailed(DirectoryInfo di)
        {
            //Stopwatch sw = new Stopwatch();

            Console.Write("Gathering Folder Info");
            Thread t = ThreadSequenceCurrentLineText("Gathering Folder Info", '.', 6);
            t.Start();

            //sw.Start();
            List<Tuple<string, long>> files = new List<Tuple<string, long>>();
            List<string> folders = new List<string>();
            FindAllFiles(di, ref files, ref folders);

            float totalMB = 0;
            foreach (Tuple<string, long> file in files)
            {
                totalMB += file.Item2;
            }

            totalMB = totalMB / 1024f / 1024f;
            t.IsBackground = false;
            t.Join();
            Console.Write("\rGathering Folder Info......Complete\r\n");
            Console.WriteLine($"- FileCnt:{files.Count} - DirCnt:{folders.Count} - {totalMB:n2}MB");

            //sw.Restart();
            Removing(files, folders);

            Console.Write("\r\n");
            t = ThreadSequenceCurrentLineText("Removing Remaining Files/Folders", '.', 6);
            t.Start();
            try
            {
                ExecuteCommand($"del /f/s/q foldername {di.FullName} > nul", true);
                ExecuteCommand($"rmdir /s/q {di.FullName}", true);
            }
            catch
            {
                if (di.Exists)
                {
                    try
                    {
                        Thread.Sleep(100);
                        ExecuteCommand($"rmdir /s /q {di.FullName}", true, false, false, Creds[0], Crypto.DecryptStringAES(Creds[1], Environment.UserName));
                        Thread.Sleep(100);
                    }
                    catch
                    {
                        Console.Write("\r\n");
                        Console.WriteLine($"Unable to remove Temp directory. \"{di.FullName}\"");
                        return false;
                    }
                }
            }
            t.IsBackground = false;
            t.Join();
            Console.Write("\rRemoving Remaining Files/Folders......Complete");
            Console.Write("\r\n");
            return !di.Exists;
        }

        private static bool CleanupDirectoryQuick(DirectoryInfo di)
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();

            Thread t = ThreadSequenceCurrentLineText("Removing Remaining Files/Folders", '.', 6);
            t.Start();
            try
            {
                ExecuteCommand($"del /f/s/q foldername {di.FullName} > nul", true);
                Thread.Sleep(100);
                ExecuteCommand($"rmdir /s/q {di.FullName}", true);
                Thread.Sleep(100);
            }
            catch
            {
                if (di.Exists)
                {
                    try
                    {
                        Thread.Sleep(100);
                        ExecuteCommand($"rmdir /s/q {di.FullName}", true, false, false, Creds[0], Crypto.DecryptStringAES(Creds[1], Environment.UserName));
                        Thread.Sleep(100);
                    }
                    catch
                    {
                        Console.Write("\r\n");
                        Console.WriteLine($"Unable to remove Temp directory. \"{di.FullName}\"");
                        return false;
                    }
                }
            }
            t.IsBackground = false;
            t.Join();
            Console.Write("\rRemoving Remaining Files/Folders......Complete");
            Console.Write($"\r\n - Time:{sw.ElapsedMilliseconds / 1000}s");
            Console.Write("\r\n");
            return !di.Exists;
        }

        private static void FolderProfileCleanup()
        {
            char ans;
            string profdate = ProfileCompareToString(ProfileCompare);
            ConsoleWriter cw = new ConsoleWriter();
            Console.WriteLine($"\r\nGathering User Profile information for {Computer}");

            cw.WriteAnimated("Settings: ", 10, ConsoleColor.Gray, ConsoleColor.Blue)
                .WriteAnimated($"MaxAgeMonths", 10, ConsoleColor.White, ConsoleColor.Blue).WriteAnimated(" [", 0, ConsoleColor.Gray)
                .WriteAnimated(MaxAgeMonths.ToString(), 10, ConsoleColor.Yellow, ConsoleColor.Blue).WriteAnimated("], ", 0, ConsoleColor.Gray)
                .WriteAnimated("ProfileDateCheck", 10, ConsoleColor.White, ConsoleColor.Blue).WriteAnimated(" [", 0, ConsoleColor.Gray)
                .WriteAnimated(profdate.ToString(), 10, ConsoleColor.Yellow, ConsoleColor.Blue).WriteAnimated("] ", 0, ConsoleColor.Gray);

            Thread.Sleep(500);
            Console.WriteLine("");
            Console.Write("Working");
            Thread t = ThreadSequenceCurrentLineText("Working", '.', 6);
            t.Start();
            string[][][] list = GetProfileInfoDat(Computer, MaxAgeMonths, ProfileCompare, Creds[0], Crypto.DecryptStringAES(Creds[1], Environment.UserName));
            t.IsBackground = false;
            t.Join();
            Console.WriteLine("");
            Console.WriteLine("-----------------------SID---------------------|--User--|--LastProfileUpdate--");
            if (list[0] != null)
            {
                Console.WriteLine("------- Keep");
                foreach (string[] l in list[0])
                {
                    Console.WriteLine(string.Join("|", l));
                }
            }

            if (list[1] != null)
            {
                Console.WriteLine("------- Remove");
                foreach (string[] l in list[1])
                {
                    Console.WriteLine(string.Join("|", l));
                }
            }

            Console.WriteLine("");
            if (list[1] != null && list[1].Length > 0)
            {
                int profProcessed = 0;
                if (Automate)
                {
                    string lastResult = "";
                    Individual = false;
                    for (int i = 0; i < list[1].Count(); i++)
                    {
                        string[] l = list[1][i];

                        Console.Clear();
                        Console.WriteLine("-------------------------------------");
                        Console.WriteLine("Press Esc to stop after current job is finished.");
                        Console.WriteLine("-------------------------------------");

                        Console.WriteLine($"Processing {i + 1} of {list[1].Count()} profiles. Total {TotalMB:n2}MB Cleaned");
                        if (i > 0)
                        {
                            Console.WriteLine($"Last Result: {list[1][i - 1][1]} - {lastResult}");
                        }

                        Console.WriteLine("");
                        int response = RemoveUserProfile(l[0], l[1], Computer != Environment.MachineName ? Computer : null, Individual);
                        if (response == -1)
                        {
                            Console.WriteLine("Stop Cleaning User Profiles? (Y/n)");
                            while (Console.KeyAvailable)
                            {
                                Console.ReadKey(false);
                            }

                            ans = Console.ReadKey(true).KeyChar;
                            if (string.Equals(ans.ToString(), "y", StringComparison.OrdinalIgnoreCase) || char.IsWhiteSpace(ans))
                            {
                                Console.WriteLine("\r\nStopping.\r\n");
                                break;
                            }
                            else
                            {
                                response = -2;
                                lastResult = "Profile Skipped.";
                            }
                        }
                        else if (response == -2)
                        {
                            lastResult = "Profile Skipped.";
                        }
                        else if (response == 0)
                        {
                            lastResult = "Failed to Remove Profile.";
                        }
                        else if (response == 1)
                        {
                            lastResult = "Profile Removed.";
                        }

                        profProcessed++;
                        Console.WriteLine("\r\n" + lastResult + "\r\n");
                    }

                    Console.Clear();
                    Console.WriteLine("-------------------------------------");
                    Console.WriteLine("User cleanup completed.");
                    Console.WriteLine("-------------------------------------");
                    Console.WriteLine("");
                    Console.WriteLine($"Processed {profProcessed.ToString()} profiles. Total {TotalMB:n2}MB Cleaned");
                }
                else
                {
                    Console.WriteLine($"Ready to remove {list[1].Count()} accounts older than {MaxAgeMonths} months? (Y/n).");
                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(false);
                    }

                    ans = Console.ReadKey(true).KeyChar;
                    if (string.Equals(ans.ToString(), "y", StringComparison.OrdinalIgnoreCase) || char.IsWhiteSpace(ans))
                    {
                        Console.WriteLine("Individually prompt to remove each profile? (Y/n).");
                        while (Console.KeyAvailable)
                        {
                            Console.ReadKey(false);
                        }

                        ans = Console.ReadKey(true).KeyChar;
                        if (string.Equals(ans.ToString(), "y", StringComparison.OrdinalIgnoreCase) || char.IsWhiteSpace(ans))
                        {
                            Individual = true;
                        }

                        Console.Clear();
                        if (!Individual)
                        {
                            Console.WriteLine("Press Esc to stop after current job is finished.\r\n");
                        }

                        string lastResult = "";
                        for (int i = 0; i < list[1].Count(); i++)
                        {
                            string[] l = list[1][i];

                            Console.Clear();
                            Console.WriteLine("-------------------------------------");
                            Console.WriteLine("Press Esc to stop after current job is finished.");
                            Console.WriteLine("-------------------------------------");

                            Console.WriteLine($"Processing {i + 1} of {list[1].Count()} profiles. Total {TotalMB:n2}MB Cleaned");
                            if (i > 0)
                            {
                                Console.WriteLine($"Last Result: {list[1][i - 1][1]} - {lastResult}");
                            }

                            Console.WriteLine("");
                            int response = RemoveUserProfile(l[0], l[1], Computer != Environment.MachineName ? Computer : null, Individual);
                            if (response == -1)
                            {
                                Console.WriteLine("Stop Cleaning User Profiles? (Y/n)");
                                while (Console.KeyAvailable)
                                {
                                    Console.ReadKey(false);
                                }

                                ans = Console.ReadKey(true).KeyChar;
                                if (string.Equals(ans.ToString(), "y", StringComparison.OrdinalIgnoreCase) || char.IsWhiteSpace(ans))
                                {
                                    Console.WriteLine("\r\nStopping.\r\n");
                                    break;
                                }
                                else
                                {
                                    response = -2;
                                    lastResult = "Profile Skipped.";
                                }
                            }
                            else if (response == -2)
                            {
                                lastResult = "Profile Skipped.";
                            }
                            else if (response == 0)
                            {
                                lastResult = "Failed to Remove Profile.";
                            }
                            else if (response == 1)
                            {
                                lastResult = "Profile Removed.";
                            }

                            profProcessed++;
                            Console.WriteLine("\r\n" + lastResult + "\r\n");
                        }

                        Console.Clear();
                        Console.WriteLine("-------------------------------------");
                        Console.WriteLine("User cleanup completed.");
                        Console.WriteLine("-------------------------------------");
                        Console.WriteLine("");
                        Console.WriteLine($"Processed {profProcessed.ToString()} profiles. Total {TotalMB:n2}MB Cleaned");
                    }
                }
            }
            else
            {
                Console.WriteLine($"No accounts older than {MaxAgeMonths} months.");
            }
        }

        private static void FolderTempPCCleanup()
        {
            List<string[]> folderList = new List<string[]>();
            Console.WriteLine($"\r\nGathering temp file list for {Computer}.");
            Console.Write("Working");
            Thread t = ThreadSequenceCurrentLineText("Working", '.', 6);
            t.Start();
            GetExistingFolders(Computer, "", FolderListWin, ref folderList);
            List<Tuple<string, long>> files = new List<Tuple<string, long>>();
            List<string> folders = new List<string>();
            foreach (string[] directory in folderList)
            {
                FindAllFiles(new DirectoryInfo(directory[0]), ref files, ref folders, directory[1]);
            }

            t.IsBackground = false;
            t.Join();
            foreach (string s in FolderListWin)
            {
                int index = folders.FindIndex(x => x == s);
                if (index > 0)
                {
                    folders.RemoveAt(index);
                }
            }

            Removing(files, folders, !Automate);
        }

        private static void FolderTempUserCleanup()
        {
            List<string[]> folderList = new List<string[]>();
            Console.WriteLine($"\r\nGathering User Temp Folder information for {Computer}.");
            Thread t = ThreadSequenceCurrentLineText("Working", '.', 6);
            t.Start();
            string[][][] list = GetProfileInfoDat(Computer, MaxAgeMonths, ProfileCompare, Creds[0], Crypto.DecryptStringAES(Creds[1], Environment.UserName));

            foreach (string[] l in list[0])
            {
                GetExistingFolders(Computer, l[1], FolderListUser, ref folderList);
            }

            foreach (string[] l in list[1])
            {
                GetExistingFolders(Computer, l[1], FolderListUser, ref folderList);
            }

            t.IsBackground = false;
            t.Join();
            Console.Write("\r\nCollecting File data in all user locations.\r\n");
            Console.Write("Press Escape to cancel.\r\n");
            List<Tuple<string, long>> files = new List<Tuple<string, long>>();
            List<string> folders = new List<string>();
            int cnt = 0;
            string write1 = "";
            string write2 = "";
            foreach (string[] directory in folderList)
            {
                cnt++;
                write1 = $"\r({cnt}/{folderList.Count}) {CreateStringLimit($"{directory[0]}\\{directory[1]}", 55, true)}";
                write2 = "";
                if (write1.Length < 66)
                {
                    write2 = new string(' ', 66 - write1.Length);
                }

                Console.Write(write1 + write2);

                while (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(false);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        return;
                    }
                }

                FindAllFiles(new DirectoryInfo(directory[0]), ref files, ref folders, directory[1]);
            }

            foreach (string s in FolderListUser)
            {
                int index = folders.FindIndex(x => x == s);
                if (index > 0)
                {
                    folders.RemoveAt(index);
                }
            }
            Console.WriteLine("\r\nGathering Info......Complete");
            Removing(files, folders, !Automate);
        }

        #endregion Cleanup Methods

        #region File/Folder Operations

        public static void FindAllFiles(DirectoryInfo rootDirectory, ref List<Tuple<string, long>> fileList, ref List<string> folderList, string fileSearchPattern = "*")
        {
            if (fileList == null)
            {
                fileList = new List<Tuple<string, long>>();
            }

            if (folderList == null)
            {
                folderList = new List<string>();
            }

            if (rootDirectory.Exists)
            {
                IEnumerable<FileInfo> fi = Array.Empty<FileInfo>();
                try
                {
                    fi = rootDirectory.EnumerateFiles(fileSearchPattern, SearchOption.TopDirectoryOnly).Where(dir => (dir.Attributes & FileAttributes.ReparsePoint) == 0);
                }
                catch
                {
                }

                foreach (var f in fi)
                {
                    Tuple<string, long> t = new Tuple<string, long>(f.FullName, f.Length);
                    if ((f.Attributes & FileAttributes.ReadOnly) != 0)
                    {
                        try
                        {
                            f.Attributes = FileAttributes.Normal;
                        }
                        catch
                        {
                        }
                    }

                    fileList.Add(t);
                }

                IEnumerable<DirectoryInfo> di = Array.Empty<DirectoryInfo>();
                try
                {
                    if (fileSearchPattern == "*")
                    {
                        di = rootDirectory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).Where(dir => (dir.Attributes & FileAttributes.ReparsePoint) == 0);
                    }
                }
                catch
                {
                }

                foreach (var d in di)
                {
                    FindAllFiles(d, ref fileList, ref folderList, fileSearchPattern);
                    if (fileSearchPattern == "*" && d.FullName.Contains(rootDirectory.FullName))
                    {
                        folderList.Add(d.FullName);
                    }
                }
            }
        }

        public static void FindAllFiles2(DirectoryInfo rootDirectory, ref List<Tuple<string, long>> fileList, ref List<string> folderList)
        {
            if (fileList == null)
            {
                fileList = new List<Tuple<string, long>>();
            }

            if (folderList == null)
            {
                folderList = new List<string>();
            }

            if (rootDirectory.Exists)
            {
                IEnumerable<FileInfo> fi = Array.Empty<FileInfo>();
                try
                {
                    fi = rootDirectory.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Where(dir => (dir.Attributes & FileAttributes.ReparsePoint) == 0);
                }
                catch
                {
                }

                foreach (var f in fi)
                {
                    Tuple<string, long> t = new Tuple<string, long>(f.FullName, f.Length);
                    if ((f.Attributes & FileAttributes.ReadOnly) != 0)
                    {
                        f.Attributes = FileAttributes.Normal;
                    }

                    fileList.Add(t);
                }

                IEnumerable<DirectoryInfo> di = Array.Empty<DirectoryInfo>();
                try
                {
                    di = rootDirectory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).Where(dir => (dir.Attributes & FileAttributes.ReparsePoint) == 0);
                }
                catch (UnauthorizedAccessException)
                {
                    try
                    {
                        rootDirectory.Attributes = FileAttributes.Normal;
                    }
                    catch
                    {
                    }
                }
                catch
                {
                }

                foreach (var d in di)
                {
                    FindAllFiles2(d, ref fileList, ref folderList);
                    folderList.Add(d.FullName);
                }
            }
        }

        public static string[][][] GetProfileInfoDat(string nameOrAddress, int monthsOld = 2, ProfileCompareType profileCompare = ProfileCompareType.NTUserWithDesktopAndAPPData, string username = "", string password = "")
        {
            string[][][] profiles = new string[2][][];
            ConnectionOptions options = new ConnectionOptions();
            if (!string.Equals(Environment.MachineName, nameOrAddress, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                options.Username = username;
                options.Password = password;
            }

            ManagementScope scope = new ManagementScope($"\\\\{nameOrAddress}\\root\\cimv2", options);
            scope.Options.Timeout = new TimeSpan(0, 0, 6);
            try
            {
                scope.Connect();
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException || ex is ManagementException)
                {
                    if (ex.Message.Contains("denied", StringComparison.CurrentCultureIgnoreCase))
                    {
                        StopSequenceThread = true;
                        Console.Clear();
                        ConsoleWriter cw = new ConsoleWriter();
                        cw.WriteLineAnimated("We have run into a permissions error.", 30)
                            .WriteLineAnimated("Please relaunch this script with necessary credentials.", 30)
                            .WriteLineAnimated("", 0)
                            .WriteLineAnimated("Press any key to exit.", 30);
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }

            ObjectQuery query = new ObjectQuery($"SELECT * FROM Win32_UserProfile Where Special = false And Not LocalPath Like \"%Admin%\"{(!string.IsNullOrWhiteSpace(username) ? " And Not LocalPath Like \"%{username}%\"" : "")}");
            using ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            if (searcher != null)
            {
                ManagementObjectCollection results = searcher.Get();
                if (results.Count > 0)
                {
                    List<string[]> l1 = new List<string[]>();
                    List<string[]> l2 = new List<string[]>();
                    try
                    {
                        foreach (ManagementObject result in results)
                        {
                            string[] path = ((string)result["LocalPath"])?.Split('\\');
                            string user = path[path.Length - 1];
                            DateTime last = DateTime.MinValue;

                            DirectoryInfo directoryInfo = new DirectoryInfo(@$"\\{nameOrAddress}\c$\Users\{user}");
                            if (directoryInfo.Exists)
                            {
                                if (profileCompare == ProfileCompareType.NTUser)
                                {
                                    FileInfo fileInfo = new FileInfo(@$"\\{nameOrAddress}\c$\Users\{user}\NTUser.DAT");
                                    DateTime last0 = fileInfo.LastWriteTime;
                                    DateTime last1 = fileInfo.LastAccessTime;
                                    last = last0.CompareTo(last1) < 0 ? last1 : last0;
                                }
                                else if (profileCompare == ProfileCompareType.NTUserWithDesktopAndAPPData)
                                {
                                    FileInfo ntFile1 = new FileInfo(@$"\\{nameOrAddress}\c$\Users\{user}\NTUSER.DAT");
                                    if (ntFile1.Exists)
                                    {
                                        DateTime last00 = ntFile1.LastWriteTime;
                                        DateTime last01 = ntFile1.LastAccessTime;
                                        last = last00.CompareTo(last01) < 0 ? last01 : last00;
                                        if (last.AddMonths(monthsOld).CompareTo(DateTime.Now) >= 0)
                                        {
                                            DirectoryInfo directoryInfo2 = new DirectoryInfo(@$"\\{nameOrAddress}\c$\Users\{user}\AppData\Local\Temp");
                                            FileInfo fileInfo = new FileInfo(@$"\\{nameOrAddress}\c$\Users\{user}\Desktop\desktop.ini");
                                            DateTime last0 = fileInfo.LastWriteTime;
                                            DateTime last1 = fileInfo.LastAccessTime;
                                            DateTime last2 = directoryInfo.LastWriteTime;
                                            DateTime last3 = directoryInfo2.LastWriteTime;
                                            last = last0.CompareTo(last1) < 0 ? last1 : last0;
                                            last = last.CompareTo(last2) < 0 ? last2 : last;
                                            last = last.CompareTo(last3) < 0 ? last3 : last;
                                        }
                                    }
                                }
                                else if (profileCompare == ProfileCompareType.DesktopOnly)
                                {
                                    DirectoryInfo directoryInfo2 = new DirectoryInfo(@$"\\{nameOrAddress}\c$\Users\{user}\Desktop");
                                    FileInfo fileInfo = new FileInfo(@$"\\{nameOrAddress}\c$\Users\{user}\Desktop\desktop.ini");
                                    DateTime last0 = fileInfo.LastWriteTime;
                                    DateTime last2 = directoryInfo.LastWriteTime;
                                    DateTime last3 = directoryInfo2.LastWriteTime;
                                    last = last0;
                                    last = last.CompareTo(last2) < 0 ? last2 : last;
                                    last = last.CompareTo(last3) < 0 ? last3 : last;
                                }

                                if (last.AddMonths(monthsOld).CompareTo(DateTime.Now) >= 0)
                                {
                                    l1.Add(new string[] { (string)result["SID"], user, last.ToString() });
                                }
                                else
                                {
                                    l2.Add(new string[] { (string)result["SID"], user, last.ToString() });
                                }
                            }
                        }
                    }
                    catch
                    {
                        //Ignore
                    }

                    profiles[0] = l1.ToArray();
                    profiles[1] = l2.ToArray();
                }
            }

            return profiles;
        }

        public static int RemoveUserProfile(string sid, string user, string pc = null, bool ask = true)
        {
            int response = 0;
            RegistryCommands regProfileList = new RegistryCommands(Registry.LocalMachine, $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\{sid}", false, pc);
            string originLocation = regProfileList.Read("ProfileImagePath");
            if (!string.IsNullOrWhiteSpace(pc) && !string.IsNullOrWhiteSpace(originLocation))
            {
                originLocation = originLocation.Substring(3, originLocation.Length - 3);
                originLocation = @$"\\{pc}\c$\{originLocation}";
            }

            string workingLocation = originLocation;
            if (Directory.Exists(workingLocation) && workingLocation?.Contains(user, StringComparison.OrdinalIgnoreCase) == true)
            {
                DirectoryInfo workingDirectoryInfo = new DirectoryInfo(workingLocation);
                Console.WriteLine($"Removing profile: {user}");

                char ans;
                bool stop = false;
                if (ask)
                {
                    Console.WriteLine("Continue? (Y/n)(s=skip)");
                    while (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(false);
                        if (key.Key == ConsoleKey.Escape)
                        {
                            stop = true;
                        }
                    }
                    if (stop)
                    {
                        ans = 'n';
                    }
                    else
                    {
                        ans = Console.ReadKey(true).KeyChar;
                    }
                }
                else
                {
                    while (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(false);
                        if (key.Key == ConsoleKey.Escape)
                        {
                            stop = true;
                        }
                    }
                    if (stop)
                    {
                        ans = 'n';
                    }
                    else
                    {
                        ans = 'y';
                    }
                }

                if (string.Equals(ans.ToString(), "y", StringComparison.OrdinalIgnoreCase) || char.IsWhiteSpace(ans))
                {
                    Console.Write("\r\nMoving Folder to Temp Directory...");
                    string tempFolder = workingDirectoryInfo.FullName.Replace(workingDirectoryInfo.Name, "Temp");
                    if (!Directory.Exists(tempFolder))
                    {
                        Directory.CreateDirectory(tempFolder);
                    }

                    try
                    {
                        try
                        {
                            workingDirectoryInfo.MoveTo(tempFolder + "\\" + user);
                        }
                        catch (IOException ex) when (ex.Message.Contains("exists", StringComparison.OrdinalIgnoreCase))
                        {
                            CleanupDirectoryQuick(new DirectoryInfo(tempFolder + "\\" + user));
                            workingDirectoryInfo.MoveTo(tempFolder + "\\" + user);
                        }
                        catch
                        {
                            throw;
                        }
                        Console.Write("\rMoving Folder to Temp Directory...Done\r\n");
                        try
                        {
                            if (!Directory.Exists(originLocation))
                            {
                                Console.Write("Cleaning Registry...");

                                string guid = regProfileList.Read("Guid");

                                RegistryCommands regProfileGuid = new RegistryCommands(RegistryView.Registry64, Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileGuid\", false, pc);
                                bool test1 = regProfileGuid.DeleteSubKeyTree(guid, false);

                                regProfileList = new RegistryCommands(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\", false, pc);
                                bool test2 = regProfileList.DeleteSubKeyTree(sid, true);

                                if (test1 && test2)
                                {
                                    response = 1;
                                }
                                else
                                {
                                    response = 0;
                                }
                                Console.Write("\rCleaning Registry...Done\r\n");
                            }
                            else
                            {
                                response = 0;
                            }

                            if (response == 1)
                            {
                                CleanupDirectoryDetailed(workingDirectoryInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Err: {ex.Message}");
                            response = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\r\nErr: {ex.Message}");
                        response = 0;
                    }
                }
                else if (string.Equals(ans.ToString(), "y", StringComparison.OrdinalIgnoreCase))
                {
                    response = -2;
                }
                else
                {
                    response = -1;
                }
            }

            return response;
        }

        public static void SetAttributesNormal(DirectoryInfo dir)
        {
            foreach (DirectoryInfo subDir in dir.GetDirectories("*", SearchOption.AllDirectories))
            {
                subDir.Attributes = FileAttributes.Normal;
                SetAttributesNormal(subDir);
            }

            foreach (FileInfo file in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                file.Attributes = FileAttributes.Normal;
            }
        }

        private static void GetExistingFolders(string computer, string user, string[] folderList, ref List<string[]> returnFolderList)
        {
            int cnt = folderList.Length;
            for (int i = 0; i < cnt; i++)
            {
                string folder = folderList[i];
                if (folder.Contains("C:", StringComparison.OrdinalIgnoreCase))
                {
                    folder = folder.Replace("C:", $@"\\{computer}\c$");
                }

                if (folder.Contains("<USERNAME>", StringComparison.OrdinalIgnoreCase))
                {
                    folder = folder.Replace("<USERNAME>", user);
                }

                int sep = folder.LastIndexOf('\\');
                string path = folder.Substring(0, sep);
                string files = folder.Substring(sep + 1, folder.Length - sep - 1);
                if (Directory.Exists(path))
                {
                    returnFolderList.Add(new string[] { path, files });
                }
            }
        }

        private static void Removing(List<Tuple<string, long>> files, List<string> folders, bool ask = false)
        {
            int cnt = files.Count;
            int i;
            int perc = 0;
            int stars = perc / 5;
            float totalMB = 0;
            float cleanedMB = 0;
            foreach (Tuple<string, long> file in files)
            {
                totalMB += file.Item2 / 1024f / 1024f;
            }

            if (ask)
            {
                Console.WriteLine("");
                Console.WriteLine($"Found {totalMB}MB of files to remove. Cleanup Now? (Y/n)");
                while (Console.KeyAvailable)
                {
                    Console.ReadKey(false);
                }

                char ans = Console.ReadKey(true).KeyChar;
                if (string.Equals(ans.ToString(), "n", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            Console.WriteLine("\r\nRemoving Files...");
            string write = $"[{new string('*', stars)}{new string('-', 20 - stars)}] {perc}%  {cleanedMB:n2}/{totalMB:n2}MB    ";
            Console.Write(write);

            List<Task<long>> TaskList = new List<Task<long>>();
            List<Task> TaskList2 = new List<Task>();
            for (i = 0; i < cnt; i++)
            {
                perc = (int)Math.Round((double)(i * 100 / cnt), 0);
                stars = perc / 5;
                write = $"[{new string('*', stars)}{new string('-', 20 - stars)}] {perc}%  {cleanedMB:n2}/{totalMB:n2}MB    ";
                Console.Write($"\r{write}");

                while (TaskList.Count >= TotalFileThreads)
                {
                    int completed = Task.WaitAny(TaskList.ToArray());
                    long result = TaskList[completed].Result;
                    if (result >= 0)
                    {
                        cleanedMB += result / 1024f / 1024f;
                    }
                    TaskList.RemoveAt(completed);
                }

                if (i < files.Count)
                {
                    string name = files[i].Item1;
                    long size = files[i].Item2;
                    Task<long> LastTask = new Task<long>(() => TryDeleteFile(name, size));
                    LastTask.Start();
                    TaskList.Add(LastTask);
                }
            }

            while (TaskList.Count > 0)
            {
                int completed = Task.WaitAny(TaskList.ToArray());
                long result = TaskList[completed].Result;
                if (result >= 0)
                {
                    cleanedMB += result / 1024f / 1024f;
                }
                TaskList.RemoveAt(completed);
            }

            TotalMB += cleanedMB;

            perc = (int)Math.Round((double)(i * 100 / cnt), 0);
            stars = perc / 5;
            write = $"[{new string('*', stars)}{new string('-', 20 - stars)}] {perc}%  {cleanedMB:n2}/{totalMB:n2}MB    ";
            Console.Write($"\r{write}");

            cnt = folders.Count;
            perc = 0;
            stars = perc / 5;

            Console.WriteLine("\r\nRemoving Folders...");
            int lineLen = write.Length;
            write = $"[{new string('*', stars)}{new string('-', 20 - stars)}] {perc}%  ";
            write += new string(' ', lineLen - write.Length);
            Console.Write($"{write}");

            TaskList.Clear();
            for (i = 0; i < cnt; i++)
            {
                perc = (int)Math.Round((double)(i * 100 / cnt), 0);
                stars = perc / 5;
                write = $"[{new string('*', stars)}{new string('-', 20 - stars)}] {perc}%  ";
                write += new string(' ', lineLen - write.Length);
                Console.Write($"\r{write}");

                while (TaskList2.Count >= TotalFolderThreads)
                {
                    TaskList2.RemoveAt(Task.WaitAny(TaskList2.ToArray()));
                }

                Task LastTask2 = new Task(() =>
                {
                    if (i < folders.Count)
                    {
                        NativeMethods.RemoveDirectory(folders[i]);
                    }
                });
                LastTask2.Start();
                TaskList2.Add(LastTask2);
            }

            Task.WaitAll(TaskList2.ToArray());

            perc = (int)Math.Round((double)(i * 100 / cnt), 0);
            stars = perc / 5;
            write = $"[{new string('*', stars)}{new string('-', 20 - stars)}] {perc}%  ";
            write += new string(' ', lineLen - write.Length);
            Console.Write($"\r{write}");
            Console.WriteLine("");
        }

        private static long TryDeleteFile(string file, long sizeReturn)
        {
            if (string.IsNullOrWhiteSpace(file) || sizeReturn < 0)
            {
                return -1;
            }

            bool test = NativeMethods.DeleteFile(file);
            if (!test)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    return -1;
                }
            }
            return sizeReturn;
        }

        #endregion File/Folder Operations

        #region Other

        public static void ExecuteCommand(string command, bool wait = false, bool showWindow = false, bool interactable = false, string user = null, string pass = null)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"{(showWindow && interactable ? "/k" : "/c")} {command}",
                CreateNoWindow = !showWindow,
                UseShellExecute = false
            };

            if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
            {
                startInfo.UserName = user;
                startInfo.PasswordInClearText = pass;
                startInfo.Domain = Environment.UserDomainName;
            }

            using Process process = new Process { StartInfo = startInfo };

            try
            {
                process.Start();
                if (wait)
                {
                    process.WaitForExit();
                }
            }
            catch
            {
                throw;
            }
        }

        public static void RemoveConnection()
        {
            ExecuteCommand(@$"Net Use \\{Computer}\c$ /delete", true);
            CompImpersonationContext?.Leave();
        }

        private static int RepairUserDirectory()
        {
            List<string[]> folderList = new List<string[]>();
            Console.WriteLine($"\r\nGathering User Temp Folder information for {Computer}.");
            Thread t = ThreadSequenceCurrentLineText("Working", '.', 6);
            t.Start();

            Dictionary<string, string> comparisons = new Dictionary<string, string>();

            Dictionary<string, string> allUserKeys = new Dictionary<string, string>();
            List<string> allUserFolders = new List<string>();

            DirectoryInfo[] userDirectories = null;
            DirectoryInfo directoryInfo = new DirectoryInfo(@$"\\{Computer}\c$\Users\");
            if (directoryInfo.Exists)
            {
                userDirectories = directoryInfo.GetDirectories();
            }

            const string profileListKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\";
            RegistryCommands regProfileList = new RegistryCommands(RegistryView.Registry64, Registry.LocalMachine, profileListKey, false, Computer);
            string[] userRegistries = regProfileList.GetAllSubKeys();

            t.IsBackground = false;
            t.Join();

            foreach (DirectoryInfo di in userDirectories)
            {
                if (di.Name == "Temp" || di.Name == "Public" || di.Name == "Administrator" || di.Name.StartsWith("Default") || di.Name.StartsWith("All"))
                {
                    continue;
                }

                allUserFolders.Add(di.FullName);
            }

            foreach (string s in userRegistries)
            {
                if (s.Length > 20)
                {
                    string imagePath = regProfileList.ReadSubFolderKey(s, "ProfileImagePath");
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        string profName = new DirectoryInfo(imagePath).Name;
                        if (profName == "Temp" || profName == "Public" || profName == "Administrator" || profName.StartsWith("Default") || profName.StartsWith("All"))
                        {
                            continue;
                        }

                        allUserKeys.Add(s, imagePath);
                    }
                }
            }

            foreach (string s in allUserFolders)
            {
                string cleanedPath = s.Replace($"\\\\{Computer}\\c$", "C:");
                if (allUserKeys.ContainsValue(cleanedPath))
                {
                    comparisons.Add(cleanedPath, allUserKeys.FirstOrDefault(o => o.Value == cleanedPath).Key);
                }
                else
                {
                    comparisons.Add(cleanedPath, "");
                }
            }

            int cnt = 0;
            foreach (KeyValuePair<string, string> kvp in allUserKeys)
            {
                if (!comparisons.ContainsValue(kvp.Key))
                {
                    cnt++;
                    comparisons.Add((cnt + 1).ToString(), kvp.Key);
                }
            }

            char ans = 'y';
            Dictionary<string, string> issues = comparisons.Where(o => string.IsNullOrEmpty(o.Value) || o.Key.Length <= 3).ToDictionary(o => o.Key, o => o.Value);
            if (issues.Count == 0)
            {
                Console.WriteLine("\r\n\r\nNo issues found.");
            }
            else
            {
                Console.WriteLine($"\r\n{issues.Count} issue(s) found.\r\n");
                if (!Automate)
                {
                    foreach (KeyValuePair<string, string> kvp in issues)
                    {
                        if (kvp.Key.Length <= 3)
                        {
                            Console.WriteLine($"Missing User Folder for key: {kvp.Value} ({allUserKeys.FirstOrDefault(o => o.Key == kvp.Value).Value})");
                        }
                        else if (string.IsNullOrEmpty(kvp.Value))
                        {
                            Console.WriteLine($"Missing Registry key for folder: {kvp.Key}");
                        }
                    }

                    Console.WriteLine("\r\nFix all issues? (Y/n)");
                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(false);
                    }

                    ans = Console.ReadKey(true).KeyChar;
                    Console.WriteLine("");
                }
            }

            if (string.Equals(ans.ToString(), "y", StringComparison.OrdinalIgnoreCase) || char.IsWhiteSpace(ans))
            {
                //<User Folder, Registry Key>
                foreach (KeyValuePair<string, string> kvp in issues)
                {
                    Console.WriteLine("Working...");
                    if (kvp.Key.Length <= 3)
                    {
                        RegistryCommands regProfile = new RegistryCommands(RegistryView.Registry64, Registry.LocalMachine, profileListKey + kvp.Value, false, Computer);
                        bool test = regProfile.DeleteSubKeyTree(false);
                        Console.WriteLine($"{kvp.Value} - {(test ? "Removed" : "Unable to remove.")}");
                    }
                    else if (string.IsNullOrEmpty(kvp.Value))
                    {
                        DirectoryInfo di = new DirectoryInfo(kvp.Key);
                        if (di.Exists)
                        {
                            bool test;
                            try
                            {
                                string tempPath = Path.Combine(di.Parent.FullName, "Temp");
                                if (!Directory.Exists(tempPath))
                                {
                                    Directory.CreateDirectory(tempPath);
                                }

                                di.MoveTo(Path.Combine(tempPath, di.Name));
                                test = true;
                            }
                            catch
                            {
                                test = false;
                            }

                            Console.WriteLine($"{Path.Combine(new string[] { di.Parent.Name, "Temp", di.Name })} - {(test ? "Moved to Temp" : "Unable to move.")}");
                        }
                    }
                }
            }

            return issues.Count;
        }

        #endregion Other
    }
}