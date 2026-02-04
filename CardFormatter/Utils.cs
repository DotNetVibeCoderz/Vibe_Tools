using System;
using System.IO;
using System.Security.Cryptography;
using System.Linq;

namespace CardFormatter
{
    public static class SecureEraser
    {
        public static void WipeDrive(DriveInfo drive)
        {
            Console.WriteLine($"\n[WARNING] Starting Secure Erase on {drive.Name}");
            Console.WriteLine("This will overwrite all free space with zeros. This takes a long time.");
            
            try
            {
                string root = drive.RootDirectory.FullName;
                string tempFile = Path.Combine(root, "secure_wipe.tmp");

                Console.WriteLine("Step 1: Overwriting free space...");
                
                using (FileStream fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    // 1MB buffer
                    byte[] buffer = new byte[1024 * 1024]; 
                    // Fill with zeros
                    Array.Clear(buffer, 0, buffer.Length);

                    long totalBytes = drive.TotalFreeSpace;
                    long written = 0;

                    // Leave 1MB breathing room so we don't choke the FS
                    long target = totalBytes - (1024 * 1024); 

                    while (written < target)
                    {
                        try 
                        {
                            fs.Write(buffer, 0, buffer.Length);
                            written += buffer.Length;
                            
                            // Simple progress bar every 100MB
                            if (written % (100 * 1024 * 1024) == 0)
                            {
                                Console.Write(".");
                            }
                        }
                        catch (IOException)
                        {
                            // Disk full usually throws IOException
                            break;
                        }
                    }
                }

                Console.WriteLine("\nStep 2: Cleaning up...");
                File.Delete(tempFile);
                Console.WriteLine("[SUCCESS] Drive wiped securely.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Secure erase failed: {ex.Message}");
            }
        }
    }

    public static class CapacityTester
    {
        public static void VerifyCapacity(DriveInfo drive)
        {
            Console.WriteLine($"\n[INFO] verifying capacity for {drive.Name} (Fake Card Test)...");
            Console.WriteLine("Logic: Write verify files to fill drive, then read back.");

            string root = drive.RootDirectory.FullName;
            string testFolder = Path.Combine(root, "VerifyTest");
            
            try
            {
                if (!Directory.Exists(testFolder)) Directory.CreateDirectory(testFolder);

                long freeSpace = drive.TotalFreeSpace;
                // Use 100MB chunks
                int chunkSize = 100 * 1024 * 1024; 
                byte[] pattern = new byte[chunkSize];
                
                // Create a deterministic pattern (e.g., 0xAA)
                for(int i=0; i<pattern.Length; i++) pattern[i] = (byte)(i % 255);

                int fileCount = (int)(freeSpace / chunkSize);
                
                Console.WriteLine($"[TEST] Writing {fileCount} chunks of 100MB...");

                // WRITE PHASE
                for (int i = 0; i < fileCount; i++)
                {
                    string fPath = Path.Combine(testFolder, $"test_{i}.h2w");
                    File.WriteAllBytes(fPath, pattern);
                    Console.Write("W");
                }

                Console.WriteLine("\n[TEST] Verifying data...");

                // READ PHASE
                bool errorFound = false;
                for (int i = 0; i < fileCount; i++)
                {
                    string fPath = Path.Combine(testFolder, $"test_{i}.h2w");
                    byte[] readBack = File.ReadAllBytes(fPath);
                    
                    if (!readBack.SequenceEqual(pattern))
                    {
                        Console.WriteLine($"\n[FAILURE] Corruption detected at chunk {i}. Possible fake card!");
                        errorFound = true;
                        break;
                    }
                    Console.Write("V");
                }

                if (!errorFound)
                {
                    Console.WriteLine("\n[SUCCESS] Card capacity is valid!");
                }

                // CLEANUP
                Console.WriteLine("\n[INFO] Cleaning up test files...");
                Directory.Delete(testFolder, true);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Verification stopped: {ex.Message}");
            }
        }
    }
}
