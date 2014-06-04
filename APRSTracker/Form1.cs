using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

using AprsParser;
using AprsWebClient;
using Communication;
using CoT;
using Location;

namespace APRSTracker
{
    public partial class Form1 : Form
    {
        #region fields 

        private  bool _doWork;

        private Thread _workThread;
        private AprsClient _aprsWebClient = new AprsClient();
        private UdpClient _udpClient;
        private FileSystemWatcher _fileSystemWatcher;

        #endregion

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            AprsParser.Parser.OnParsed += Parser_OnParsed;
        }

        #region event handlers
 
        void Form1_Load(object sender, EventArgs e)
        {
            RestoreSettings();

            _aprsWebClient.OnResponse += _aprsWebClient_OnResponse;
        }

        void _udpClient_MessageSent(object sender, EventArgs e)
        {
            UpdateOutput("Sent Cot: " + sender);
        }

        void Parser_OnParsed(object sender, EventArgs e)
        {
            var location = sender as LocationObject;

            if (location != null)
            {
                if (!string.IsNullOrEmpty(location.Error))
                {
                    UpdateOutput("Call sign: " + location.Srccall + " Error:  " + location.Error);
                }
                else
                {
                    UpdateOutput(location.ToString());
                }
            }
        }

        void _aprsWebClient_OnResponse(object sender, EventArgs e)
        {
            var response = sender as string;

            LocationContainer locations =
              new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<LocationContainer>(response);

            foreach (var item in locations.Entries)
            {
                item.AdjustTimesForWindows();
                UpdateOutput(item.ToString());
                
                if (Properties.Settings.Default.EmitCot)
                {
                    EmitCot(item);
                }
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (buttonStart.Text == "Start")
            {
                _workThread = new Thread(DoWork);
                _doWork = true;
                panelSettings.Enabled = false;
                buttonStart.Text = "Stop";
                _workThread.Start();
            }
            else
            {
                _doWork = false;
                panelSettings.Enabled = true;
                buttonStart.Text = "Start";
                _workThread.Abort();
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }


        void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name == "APRSIS32.log")
            {
                int counter = 0;
                string line;
                string uid;
                string time;
                string lat;
                string lng;
                string hae;

                // Read the file line by line.
                using (System.IO.StreamReader file =
                    new System.IO.StreamReader(e.FullPath))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.Contains(textBoxCallsign.Text))
                        {
                            var elements = line.Split('/');
                            if (elements.Length == 8)
                            {
                                /*
                                    [0]	"IGate:WinMain:2014-05-10T20:22:03.526 RFtoIS:[Mobilinkd]IGated KG6PPZ-11>KG6PPZ,WIDE2-1,qAR,KG6PPZ:"	
                                    [1]	"202201h3635.49N"	
                                    [2]	"08716.31WO000"	
                                    [3]	"000"	
                                    [4]	"A=000553"	
                                    [5]	"Ti=32"	string
                                    [6]	"Te=70"	string
                                    [7]	"V=11429 ForceX high altitude balloon test"	                          
                             
                                 */

                                //TODO: figure out how to get timestamp from various msgs we are processing.
                                // will we have IGATE and direct messages or just IGATE?
                                var index = elements[1].IndexOf('h');
                                if (index > 0)
                                {
                                    var rawTime = elements[1].Substring(0, index);
                                    var rawLat = elements[1].Substring(index + 1);
                                    if (rawTime.Length == 6)
                                    {
                                        var hour = int.Parse(rawTime.Substring(0, 2));
                                        var minute = int.Parse(rawTime.Substring(2, 2));
                                        var second = int.Parse(rawTime.Substring(4, 2));
                                        DateTime msgTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);
                                    }
                                }

                                // Assuming western hemisphere...
                                index = elements[2].IndexOf('W');
                                if (index > 0)
                                {
                                    var rawLon = elements[2].Substring(0, index + 1);
                                }


                                // GetCot(string uid, string time, string lat, string lng, string hae)
                   // want time string in this format:
            //2014-04-07T22:52:48.61Z 
                            }
                            UpdateOutput(line);
                        }
                    }

                    file.Close();
                }
            }
        }

        #endregion

        #region Private methods

        private void SaveSettings()
        {
            Properties.Settings.Default.ApiKey = textBoxApiKey.Text;
            Properties.Settings.Default.Callsign = textBoxCallsign.Text;
            Properties.Settings.Default.CotIp = textBoxCotIp.Text;
            Properties.Settings.Default.CotPort = textBoxCotPort.Text;
            Properties.Settings.Default.EmitCot = checkBoxCot.Checked;
            Properties.Settings.Default.UseRadio = checkBoxRadio.Checked;
            Properties.Settings.Default.UseWeb = checkBoxWeb.Checked;
            Properties.Settings.Default.MonitorPath = textBoxLogPath.Text ;
            
            Properties.Settings.Default.Save();        
        }

        private void RestoreSettings()
        { 
            textBoxApiKey.Text = Properties.Settings.Default.ApiKey;
            textBoxCallsign.Text = Properties.Settings.Default.Callsign;
            textBoxCotIp.Text =  Properties.Settings.Default.CotIp;
            textBoxCotPort.Text =  Properties.Settings.Default.CotPort;
            checkBoxCot.Checked =   Properties.Settings.Default.EmitCot;
            checkBoxRadio.Checked = Properties.Settings.Default.UseRadio;
            checkBoxWeb.Checked = Properties.Settings.Default.UseWeb;
            textBoxLogPath.Text = Properties.Settings.Default.MonitorPath;
        }

         private void DoWork()
        {
            while (_doWork)
            {
                if (!ShouldDoWork())
                {
                    UpdateOutput("No actions are checked. Check \"use radio\" and/or \"Use www.aprs.fi\"");
                }
                else
                {
                    if (checkBoxRadio.Checked)
                    {
                        DoRadioStuff();
                    }

                    if (checkBoxWeb.Checked)
                    {
                        if (!string.IsNullOrEmpty(Properties.Settings.Default.Callsign) &&
                         !string.IsNullOrEmpty(Properties.Settings.Default.ApiKey))
                        {

                            DoAprsFiStuff();
                        }
                        else
                        {
                            UpdateOutput(@"Need a callsign and an API key to track via www.aprs.fi.");
                        }
                    }
                }
                Thread.Sleep(30000);
            }
        }

        private  bool ShouldDoWork()
        {
            return checkBoxRadio.Checked  || 
                    checkBoxWeb.Checked;
        }

        private void DoRadioStuff()
        {
            if(_fileSystemWatcher != null)
            { 
                // enable watching by allowing events to be raised
                _fileSystemWatcher.EnableRaisingEvents =  checkBoxRadio.Checked;
            }
            else if (checkBoxRadio.Checked && !string.IsNullOrEmpty(textBoxLogPath.Text))
            {
                WatchFile(textBoxLogPath.Text);
            }
        }

        private void WatchFile(string directoryToWatch)
        {
            _fileSystemWatcher  = new FileSystemWatcher();

            // set the directory to watch
            _fileSystemWatcher.Path = directoryToWatch;

            //this is the heart - setup multiple filters
            // to watch various types of changes to watch
            _fileSystemWatcher.NotifyFilter = NotifyFilters.Size |
                                NotifyFilters.FileName |
                                NotifyFilters.DirectoryName |
                                NotifyFilters.CreationTime;

            //TODO get file extension for log files...
            // setup which file types do we want to monitor
            _fileSystemWatcher.Filter = "*.log";

            // setup event handlers to watch for changes
            _fileSystemWatcher.Created += _fileSystemWatcher_Changed;
            _fileSystemWatcher.Changed += _fileSystemWatcher_Changed;

            // just some debugging
            UpdateOutput(string.Format("Monitoring {0} .", directoryToWatch));

            // enable watching by allowing events to be raised
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void DoAprsFiStuff()
        {
            try
            {
                _aprsWebClient.GetLastLocation(Properties.Settings.Default.Callsign);
            }
            catch (Exception ex)
            {
                UpdateOutput("Error connecting to APRS Client:" + ex.Message);
            }
        }

        private void InitializeUdp()
        {
            if(string.IsNullOrEmpty(textBoxCotIp.Text) || string.IsNullOrEmpty(textBoxCotPort.Text))
            {
                UpdateOutput("No IP address or port supplied for UDP communication.");
                return;
            }

            _udpClient = new UdpClient(textBoxCotIp.Text, textBoxCotPort.Text);
            _udpClient.MessageSent += _udpClient_MessageSent;
        }
        
        private void EmitCot(LocationObject location)
        { 
            if(_udpClient == null)
            {
                InitializeUdp();
            }
            if (_udpClient != null)
            {
                string cotString = CotGenerator.GetCot(location);
                _udpClient.SendUdp(cotString);
            }
        }

        private void UpdateOutput(string text)
         {
             if (textBoxOutput.InvokeRequired)
             {
                 textBoxOutput.Invoke(new MethodInvoker(() => { UpdateOutput(text); }));
             }
             else
             {
                 textBoxOutput.Text += text + "\r\n";
             }
         
         }
        #endregion

         private void button1_Click(object sender, EventArgs e)
         {
             AprsParser.Parser.TestParse();
         }

         private void buttonSetPath_Click(object sender, EventArgs e)
         {
             using (FolderBrowserDialog dialog = new FolderBrowserDialog())
             {
                 //Set the root folder
                 dialog.RootFolder = Environment.SpecialFolder.Desktop;

                 //Set the currently selected directory
                 dialog.SelectedPath = Environment.CurrentDirectory;

                 //Do you want to allow the user to create folders in this dialog?
                 dialog.ShowNewFolderButton = true;

                 //A meaningful description
                 dialog.Description = "This is a description";

                 if (dialog.ShowDialog() == DialogResult.OK) 
                 { 
                     textBoxLogPath.Text = dialog.SelectedPath;
                 }
             }
       }
    }
}
