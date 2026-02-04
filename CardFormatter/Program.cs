using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace CardFormatter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "CardFormatter - MicroSD Tool";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
   ______               __  ______                           __  __           
  / ____/___ __________/ / / ____/___  _________ ___  ____ _/ /_/ /____  _____
 / /   / __ `/ ___/ __  / / /_  / __ \/ ___/ __ `__ \/ __ `/ __/ __/ _ \/ ___/
/ /___/ /_/ / /  / /_/ / / __/ / /_/ / /  / / / / / / /_/ / /_/ /_/  __/ /    
\____/\__,_/_/   \__,_/ /_/    \____/_/  /_/ /_/ /_/\__,_/\__/\__/\___/_/     
                                                                              
            MicroSD Formatter & Utility Tool | v1.0.0
            By Jacky the Code Bender (Gravicode Studios)
            ");
            Console.ResetColor();

            Console.WriteLine($"Operating System: {RuntimeInformation.OSDescription}");
            Console.WriteLine("--------------------------------------------------\n");

            while (true)
            {
                ShowMainMenu();
            }
        }

        static void ShowMainMenu()
        {
            Console.WriteLine("\n[ MAIN MENU ]");
            Console.WriteLine("1. List & Select Drive");
            Console.WriteLine("2. Help / Info");
            Console.WriteLine("3. Exit");
            Console.Write("Select option: ");
            
            var key = Console.ReadLine() ?? "";

            switch (key)
            {
                case "1":
                    HandleDriveSelection();
                    break;
                case "2":
                    ShowHelp();
                    break;
                case "3":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }

        static void HandleDriveSelection()
        {
            var drives = DriveManager.GetRemovableDrives();
            if (drives.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No removable drives detected! Please insert a MicroSD card.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine("\nSelect a drive to manage:");
            for (int i = 0; i < drives.Count; i++)
            {
                try
                {
                    if (drives[i].IsReady)
                        Console.WriteLine($"{i + 1}. {drives[i].Name} [{drives[i].VolumeLabel}] ({DriveManager.FormatBytes(drives[i].TotalSize)}) - {drives[i].DriveFormat}");
                    else
                        Console.WriteLine($"{i + 1}. {drives[i].Name} [Not Ready]");
                }
                catch
                {
                    Console.WriteLine($"{i + 1}. {drives[i].Name} [Error reading info]");
                }
            }

            Console.Write("Enter number (or 0 to cancel): ");
            string? input = Console.ReadLine();
            
            if (int.TryParse(input, out int choice) && choice > 0 && choice <= drives.Count)
            {
                var selectedDrive = drives[choice - 1];
                DriveOperationsMenu(selectedDrive);
            }
        }

        static void DriveOperationsMenu(System.IO.DriveInfo drive)
        {
            if (!drive.IsReady)
            {
                Console.WriteLine("Drive is not ready. Please re-insert.");
                return;
            }

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ MANAGING: {drive.Name} ({drive.VolumeLabel}) ]");
                Console.ResetColor();
                Console.WriteLine("1. View Details");
                Console.WriteLine("2. Format Drive (FAT32/NTFS/exFAT)");
                Console.WriteLine("3. Error Check (Fix Logical Errors)");
                Console.WriteLine("4. Verify Capacity (Fake Card Check)");
                Console.WriteLine("5. Secure Erase (Wipe Data)");
                Console.WriteLine("6. Back to Main Menu");
                Console.Write("Select action: ");

                var key = Console.ReadLine() ?? "";
                switch (key)
                {
                    case "1":
                        DriveManager.ShowDriveDetails(drive);
                        break;
                    case "2":
                        PerformFormat(drive);
                        break;
                    case "3":
                        Formatter.CheckDisk(drive.Name);
                        break;
                    case "4":
                        CapacityTester.VerifyCapacity(drive);
                        break;
                    case "5":
                        Console.WriteLine("Are you sure? This cannot be undone! (y/n)");
                        string? confirm = Console.ReadLine();
                        if (confirm?.ToLower() == "y")
                            SecureEraser.WipeDrive(drive);
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        static void PerformFormat(System.IO.DriveInfo drive)
        {
            Console.WriteLine("\n[ FORMAT OPTIONS ]");
            Console.WriteLine("Select File System:");
            Console.WriteLine("1. FAT32 (Best for compatibility)");
            Console.WriteLine("2. exFAT (Best for >32GB cards)");
            Console.WriteLine("3. NTFS (Windows only)");
            
            Formatter.FileSystem fs = Formatter.FileSystem.FAT32;
            string? choice = Console.ReadLine();
            
            if (choice == "2") fs = Formatter.FileSystem.ExFAT;
            else if (choice == "3") fs = Formatter.FileSystem.NTFS;

            Console.Write("Enter Volume Label (Name): ");
            string? label = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(label)) label = "SDCARD";

            Console.Write("Quick Format? (y/n): ");
            string? quickInput = Console.ReadLine();
            bool quick = quickInput?.ToLower() == "y";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"WARNING: formatting {drive.Name} will erase ALL data.");
            Console.WriteLine("Type 'FORMAT' to confirm:");
            Console.ResetColor();

            if (Console.ReadLine() == "FORMAT")
            {
                Formatter.FormatDrive(drive.Name, fs, label, quick);
            }
            else
            {
                Console.WriteLine("Format cancelled.");
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("\n=== HELP ===");
            Console.WriteLine("CardFormatter helps you manage MicroSD cards.");
            Console.WriteLine("- Format: Changes file system using system tools.");
            Console.WriteLine("- Error Check: Runs chkdsk/fsck to fix logical errors.");
            Console.WriteLine("- Verify Capacity: Writes test data to check for fake cards.");
            Console.WriteLine("- Secure Erase: Overwrites empty space.");
            Console.WriteLine("\nNote: On Linux/Mac, run this tool with 'sudo' for formatting permissions.");
        }
    }
}
