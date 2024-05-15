using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Info.Blockchain.API;
using Info.Blockchain.API.BlockExplorer;
using NBitcoin;

namespace VanityAddressGenerator
{
    public partial class FormMain : Form
    {
        private double _btcStored;
        private double BtcStored
        {
            get { return _btcStored; }
            set
            {
                _btcStored = value;
                UpdateLabelBtcStoredText(_btcStored + @" BTC"); //setting label to value
            }
        }
        delegate void UpdateLabelBtcStoredTextDelegate(string newText);
        private void UpdateLabelBtcStoredText(string newText)
        {
            if (labelBtcStored.InvokeRequired)
            {
                // this is worker thread
                UpdateLabelBtcStoredTextDelegate del = UpdateLabelBtcStoredText;
                labelBtcStored.Invoke(del, newText);
            }
            else
            {
                // this is UI thread
                labelBtcStored.Text = newText;
            }
        }

        private double _chkdAddrPc;
        private double ChkdAddrPc
        {
            set
            {
                _chkdAddrPc = value;
                UpdateLabelChkdAddrPcText(_chkdAddrPc + @"%"); //setting label to value
            }
        }
        delegate void UpdateLabelChkdAddrPcTextDelegate(string newText);
        private void UpdateLabelChkdAddrPcText(string newText)
        {
            if (labelChkdAddrPc.InvokeRequired)
            {
                // this is worker thread
                UpdateLabelChkdAddrPcTextDelegate del = UpdateLabelChkdAddrPcText;
                labelChkdAddrPc.Invoke(del, newText);
            }
            else
            {
                // this is UI thread
                labelChkdAddrPc.Text = newText;
            }
        }

        const string WorkFolderPath = @"work";
        const string FilePathAddresses = WorkFolderPath + @"\vanityAddresses.txt";
        const string FilePathNotEmptyAddresses = WorkFolderPath + @"\notEmptyAddresses.txt";
        const string FilePathSecretKeys = WorkFolderPath + @"\addressSecretPairs.txt";

        // 100       - small, doesn't matter
        // 1 000     - small, doesn't matter
        // 10 000    - 0.36MB
        // 100 000   - 3.6MB
        // 1 000 000 - 36MB, too big 
        private int _addressCnt = 100;
        private readonly List<int> _fileSizesMb = new List<int> { 100, 1000, 10000, 100000 }; 

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            buttonNotEmptyAddresses.Enabled = false;
            labelBtcStored.Enabled = false;
            pictureBoxAddressGenerating.Visible = false;
            pictureBoxBackgroundAddrGen.Visible = true;
            pictureBoxRefreshing.Visible = false;
            pictureBoxBackgroundWallet.Visible = true;

            labelStatusGenAddr.Text = "";

            _fileSizesMb.ForEach(x => comboBoxFileSize.Items.Add(x));
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            pictureBoxAddressGenerating.Visible = true;
            pictureBoxBackgroundAddrGen.Visible = false;
            buttonStart.Enabled = false;
            comboBoxFileSize.Enabled = false;
            labelStatusGenAddr.Text = @"Addresses are being generated...";

            backgroundWorkerStart.RunWorkerAsync();
        }

        private void buttonOpenAddressFolder_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(WorkFolderPath)) Directory.CreateDirectory(WorkFolderPath);

            Process.Start(WorkFolderPath);
        }

        private void backgroundWorkerStart_DoWork(object sender, DoWorkEventArgs e)
        {
            var keys = new HashSet<Key>();
            for (int i = 0; i < _addressCnt; i++)
            {
                var key = new Key();

                keys.Add(key);
            }

            if (!Directory.Exists(WorkFolderPath)) Directory.CreateDirectory(WorkFolderPath);
            if (File.Exists(FilePathAddresses)) File.Delete(FilePathAddresses);
            if(File.Exists(FilePathSecretKeys)) File.Delete(FilePathSecretKeys);
            using (StreamWriter sw = File.CreateText(FilePathAddresses))
            using (StreamWriter sw2 = File.CreateText(FilePathSecretKeys))
            {
                foreach (var key in keys)
                {
                    string addr = key.PubKey.GetAddress(Network.Main).ToString();
                    sw.WriteLine(addr);
                    sw2.WriteLine(addr + ":" + key.GetBitcoinSecret(Network.Main));
                }
                sw.Flush();
                sw2.Flush();
            }
            
            var sortedAddresses = new SortedSet<string>();
            var sortedSecretKeys = new SortedSet<string>();
            foreach (var line in File.ReadAllLines(FilePathAddresses))
            {
                sortedAddresses.Add(line);
            }
            foreach (var line in File.ReadAllLines(FilePathSecretKeys))
            {
                sortedSecretKeys.Add(line);
            }
            if (File.Exists(FilePathAddresses)) File.Delete(FilePathAddresses);
            if(File.Exists(FilePathSecretKeys)) File.Delete(FilePathSecretKeys);
            using (StreamWriter sw = File.CreateText(FilePathAddresses))
            using (StreamWriter sw2 = File.CreateText(FilePathSecretKeys))
            {
                foreach (var a in sortedAddresses)
                    sw.WriteLine(a);
                foreach (var s in sortedSecretKeys)
                    sw2.WriteLine(s);

                sw.Flush();
                sw2.Flush();
            }

        }

        private void backgroundWorkerStart_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonStart.Enabled = true;
            comboBoxFileSize.Enabled = true;
            pictureBoxAddressGenerating.Visible = false;
            pictureBoxBackgroundAddrGen.Visible = true;
            labelStatusGenAddr.Text = @"Address generation is finished.";
        }

        private void comboBoxFileSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            _addressCnt = (int) comboBoxFileSize.SelectedItem;
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            labelBtcStored.Enabled = false;
            buttonNotEmptyAddresses.Enabled = false;
            buttonRefresh.Enabled = false;
            pictureBoxRefreshing.Visible = true;
            pictureBoxBackgroundWallet.Visible = false;

            backgroundWorkerRefresh.RunWorkerAsync();
        }

        private void backgroundWorkerRefresh_DoWork(object sender, DoWorkEventArgs e)
        {
            SetBtcStored();
        }

        private void backgroundWorkerRefresh_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            labelBtcStored.Enabled = true;

            if (BtcStored > 0)
            {
                buttonNotEmptyAddresses.Enabled = true;
            }
            buttonRefresh.Enabled = true;
            pictureBoxRefreshing.Visible = false;
            pictureBoxBackgroundWallet.Visible = true;

            labelBtcStored.Text = BtcStored + @" BTC";
        }

        private void SetBtcStored()
        {
            BtcStored = 0;
            ChkdAddrPc = 0;

            if (!Directory.Exists(WorkFolderPath)) Directory.CreateDirectory(WorkFolderPath);
            if (File.Exists(FilePathNotEmptyAddresses)) File.Delete(FilePathNotEmptyAddresses);
            using (StreamWriter sw = File.CreateText(FilePathNotEmptyAddresses))
            {
                int addrCount = File.ReadLines(FilePathAddresses).Count();
                int addrIdx = 0;
                var blockExplorer = new BlockExplorer();
                foreach (var a in File.ReadAllLines(FilePathAddresses))
                {
                    long currAddrSatoshi = 0;
                    try
                    {
                        currAddrSatoshi = blockExplorer.GetAddress(a).FinalBalance;
                    }
                    catch (APIException e)
                    {
                        if (e.Message == "Quota Exceeded")
                        {
                            Thread.Sleep(60*1000);
                            continue;
                        }
                    }

                    if (currAddrSatoshi > 0)
                    {
                        BtcStored += (double)currAddrSatoshi / 100000000;
                        foreach (var pubsec in File.ReadAllLines(FilePathSecretKeys))
                        {
                            if (pubsec.StartsWith(a))
                                sw.WriteLine(pubsec);
                        }
                        sw.Flush();
                    }

                    addrIdx += 1;
                    ChkdAddrPc = 100 * (addrIdx / (double)addrCount);
                }
            }
        }

        private void buttonNotEmptyAddresses_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(WorkFolderPath)) Directory.CreateDirectory(WorkFolderPath);

            Process.Start(WorkFolderPath);
        }
    }
}
