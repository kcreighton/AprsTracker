﻿using System;
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
        private string _pathToWatch; 
        private string _latestLogTimeProcessed;
        private Thread _fileWatchThread;

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

                if (_fileWatchThread != null)
                {
                    try
                    {
                        _fileWatchThread.Abort();
                    }
                    catch(Exception ex)
                    {
                        UpdateOutput("Exception stopping file watch thread: " + ex.Message);
                    }
                }
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        // Monitors log file generated by APRSIS32 APRS radio software
        void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        { 
            try
            {
                if (e.Name == "APRSIS32.log")
                {
                    var lines = File.ReadAllLines(e.FullPath);
                    
                        int counter = 0;
                        string line = "";
                        string uid = "";
                        string time = "";
                        string lat = "";
                        string lng = "";
                        string hae = "";
                        string callsign = "";
                        string comment = "";

                        
                            if (line.Contains(textBoxCallsign.Text))
                            {
                                var elements = line.Split('/');

                                // If there are 8 elements, we hope this is an IGated position message...
                                if (elements.Length == 8)
                                {
                                    // This is what is in my sample file...
                                    /*
                                        [0]	"IGate:WinMain:2014-05-10T20:22:03.526 RFtoIS:[Mobilinkd]IGated KG6PPZ-11>KG6PPZ,WIDE2-1,qAR,KG6PPZ:"	
                                        [1]	"202201h3635.49N"	
                                        [2]	"08716.31WO000"	
                                        [3]	"000"	
                                        [4]	"A=000553"	
                                        [5]	"Ti=32"	
                                        [6]	"Te=70"	
                                        [7]	"V=11429 ForceX high altitude balloon test"	                          
                             
                                     */

                                    //TODO: figure out how to get timestamp from additional msgs we might process.

                                    // This works for  IGATE  and KISS log entries
                                    var dateIndex = elements[0].IndexOf(DateTime.Now.Year.ToString());
                                    if (dateIndex >= 0)
                                    {  
                                            // TODO: if we are going to process the line, log it
                                            UpdateOutput("Processing log file line: " + line);

                                            //Get callsign -  need entire callsign because chase car and balloon use same one
                                            // with different extension
                                            var callsignIndex = elements[0].IndexOf(textBoxCallsign.Text);
                                            if (callsignIndex >= 0)
                                            {
                                                var endCallsignIndex = elements[0].IndexOf('>');
                                                callsign = elements[0].Substring(callsignIndex, endCallsignIndex - callsignIndex);
                                            }

                                            var index = elements[1].IndexOf('h');
                                            if (index > 0)
                                            {
                                                var rawTime = elements[1].Substring(0, index);
                                                var rawLat = elements[1].Substring(index + 1);

                                                // Get raw lat into a decimal string
                                                var degrees = int.Parse(rawLat.Substring(0, 2));
                                                var decimalMinutes = decimal.Parse(rawLat.Substring(2, rawLat.Length - 3));

                                                // limit to 5 decimal places
                                                var decimalDegrees = degrees + (decimalMinutes / 60);

                                                lat = decimalDegrees.ToString("F5", System.Globalization.CultureInfo.InvariantCulture);

                                                if (rawTime.Length == 6)
                                                {
                                                    var hour = int.Parse(rawTime.Substring(0, 2));
                                                    var minute = int.Parse(rawTime.Substring(2, 2));
                                                    var second = int.Parse(rawTime.Substring(4, 2));
                                                    DateTime msgTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);

                                                    time = msgTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                                                }
                                            }

                                            // Assuming western hemisphere...
                                            index = elements[2].IndexOf('W');
                                            if (index > 0)
                                            {
                                                var rawLon = elements[2].Substring(0, index);
                                                var degrees = int.Parse(rawLon.Substring(0, 3));
                                                var decimalMinutes = decimal.Parse(rawLon.Substring(3));
                                                var decimalDegrees = degrees + (decimalMinutes / 60);

                                                //limit to 5 decimal places 
                                                lng = decimalDegrees.ToString("F5", System.Globalization.CultureInfo.InvariantCulture);
                                            }

                                            // parse altitude
                                            if (!string.IsNullOrEmpty(elements[4]))
                                            {
                                                hae = elements[4].Substring(elements[4].IndexOf("A=") + 2);
                                            }

                                            //TODO: looks like there is another field before the comment... what is it?
                                            comment = elements[7];

                                            // Srccall, Lat, Lng, Hae, Lasttime, Comment);
                                            var location = new LocationObject
                                            {
                                                Srccall = callsign,
                                                Lat = lat,
                                                Lng = lng,
                                                Hae = hae,
                                                Lasttime = time,
                                                Comment = comment
                                            };

                                            EmitCot(location);
                                        }
                                    }
                                }
                            }
                  
            }
            finally
            { 
            }
        }

        // Parses a line from the log file generated by APRSIS32 APRS radio software
        void ProcessLine(string line)
        {
            if (!line.Contains(textBoxCallsign.Text))
                return;
            try
            {
                int counter = 0;
                string uid = "";
                string time = "";
                string lat = "";
                string lng = "";
                string hae = "";
                string callsign = "";
                string comment = "";
                 
                var elements = line.Split('/');
                if (elements.Length == 2)
                {
                    /*
                            [0]	"MyCall:WinMain:2014-05-31T19:03:00.367 [TransmitAPRS] KG6PPZ>APWW10,WIDE1-1,WIDE2-1:@190300h3635.50N"	
                            [1]	"08716.35WkAPRS-IS for Win32"	                     
                     */

                    var dateIndex = elements[0].IndexOf(DateTime.Now.Year.ToString());
                    if (dateIndex >= 0)
                    {
                        var spaceIndex = elements[0].IndexOf(' ');
                        var thisTime = elements[0].Substring(dateIndex, elements[0].IndexOf(' ') - dateIndex);

                        //if we are going to process the line, log it
                        UpdateOutput("Processing log file line: " + line);

                        //Get callsign -  need entire callsign because chase car and balloon use same one
                        // with different extension
                        var callsignIndex = elements[0].IndexOf(textBoxCallsign.Text);
                        if (callsignIndex >= 0)
                        {
                            var endCallsignIndex = elements[0].IndexOf('>');
                            callsign = elements[0].Substring(callsignIndex, endCallsignIndex - callsignIndex);
                        }

                        // messages that parse into 4 elements have the time before the first '/'
                        var index = elements[0].IndexOf('@');

                        if (index > 0)
                        {
                            var timeLatPart = elements[0].Substring(index);
                            var timeSeparatorIndex = timeLatPart.IndexOf('h');

                            if (timeSeparatorIndex > 0)
                            {
                                var rawTime = timeLatPart.Substring(1, timeSeparatorIndex - 1);
                                var rawLat = timeLatPart.Substring(timeSeparatorIndex + 1);

                                // Get raw lat into a decimal string
                                var degrees = int.Parse(rawLat.Substring(0, 2));
                                var decimalMinutes = decimal.Parse(rawLat.Substring(2, rawLat.Length - 3));

                                // limit to 5 decimal places
                                var decimalDegrees = degrees + (decimalMinutes / 60);

                                lat = decimalDegrees.ToString("F5", System.Globalization.CultureInfo.InvariantCulture);

                                if (rawTime.Length == 6)
                                {
                                    var hour = int.Parse(rawTime.Substring(0, 2));
                                    var minute = int.Parse(rawTime.Substring(2, 2));
                                    var second = int.Parse(rawTime.Substring(4, 2));
                                    DateTime msgTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);

                                    time = msgTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                                }
                            }
                        }

                        // Assuming western hemisphere...
                        index = elements[1].IndexOf('W');
                        if (index > 0)
                        {
                            var rawLon = elements[1].Substring(0, index);
                            
                            var degrees = int.Parse(rawLon.Substring(0, 3));
                            var decimalMinutes = decimal.Parse(rawLon.Substring(3));
                            var decimalDegrees = degrees + (decimalMinutes / 60);
                            // make longitude negative for western hemisphere
                            decimalDegrees = decimalDegrees - (2 * decimalDegrees);

                            //limit to 5 decimal places 
                            lng = decimalDegrees.ToString("F5", System.Globalization.CultureInfo.InvariantCulture);

                            // parse comment (this size message doesn't have altitude.
                            if (elements[1].Length > index)
                            {
                                comment = elements[1].Substring(index + 1);
                            }                          
                        }

                        // Srccall, Lat, Lng, Hae, Lasttime, Comment);
                        var location = new LocationObject
                        {
                            Srccall = callsign,
                            Lat = lat,
                            Lng = lng,
                            Hae = hae,
                            Lasttime = time,
                            Comment = comment
                        };

                        EmitCot(location);
                    }

                }
                else if (elements.Length == 4)
                {
                    // Sample of own location from file 
                    /*
                        [0]	"MyCall:WinMain:2014-06-14T15:29:55.108 [TransmitAPRS] KG6PPZ>APWW10,WIDE1-1,WIDE2-1:@152953h3635.52N"	 
                        [1]	"08716.29Wk056"	 
                        [2]	"000"	 
                        [3]	"A=000708APRS-IS for Win32"	 

                        */
                    var dateIndex = elements[0].IndexOf(DateTime.Now.Year.ToString());
                    if (dateIndex >= 0)
                    {
                        var spaceIndex = elements[0].IndexOf(' ');
                        var thisTime = elements[0].Substring(dateIndex, elements[0].IndexOf(' ') - dateIndex);

                        //if we are going to process the line, log it
                        UpdateOutput("Processing log file line: " + line);

                        //Get callsign -  need entire callsign because chase car and balloon use same one
                        // with different extension
                        var callsignIndex = elements[0].IndexOf(textBoxCallsign.Text);
                        if (callsignIndex >= 0)
                        {
                            var endCallsignIndex = elements[0].IndexOf('>');
                            callsign = elements[0].Substring(callsignIndex, endCallsignIndex - callsignIndex);
                        }

                        // messages that parse into 4 elements have the time before the first '/'
                        var index = elements[0].IndexOf('@');

                        if (index > 0)
                        {
                            var timeLatPart = elements[0].Substring(index);
                            var timeSeparatorIndex = timeLatPart.IndexOf('h');

                            if (timeSeparatorIndex > 0)
                            {
                                var rawTime = timeLatPart.Substring(1, timeSeparatorIndex - 1);
                                var rawLat = timeLatPart.Substring(timeSeparatorIndex + 1);

                                // Get raw lat into a decimal string
                                var degrees = int.Parse(rawLat.Substring(0, 2));
                                var decimalMinutes = decimal.Parse(rawLat.Substring(2, rawLat.Length - 3));

                                // limit to 5 decimal places
                                var decimalDegrees = degrees + (decimalMinutes / 60);

                                lat = decimalDegrees.ToString("F5", System.Globalization.CultureInfo.InvariantCulture);

                                if (rawTime.Length == 6)
                                {
                                    var hour = int.Parse(rawTime.Substring(0, 2));
                                    var minute = int.Parse(rawTime.Substring(2, 2));
                                    var second = int.Parse(rawTime.Substring(4, 2));
                                    DateTime msgTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);

                                    time = msgTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                                }
                            }
                        }

                        // Assuming western hemisphere...
                        index = elements[1].IndexOf('W');
                        if (index > 0)
                        { 
                            var rawLon = elements[1].Substring(0, index);
                            var degrees = int.Parse(rawLon.Substring(0, 3));
                            var decimalMinutes = decimal.Parse(rawLon.Substring(3));
                            var decimalDegrees = degrees + (decimalMinutes / 60);
                            // make longitude negative for western hemisphere
                            decimalDegrees = decimalDegrees - (2 * decimalDegrees);

                            //limit to 5 decimal places 
                            lng = decimalDegrees.ToString("F5", System.Globalization.CultureInfo.InvariantCulture);
                             
                        }

                        // parse altitude and comment
                        if (!string.IsNullOrEmpty(elements[3]))
                        {
                            hae = elements[3].Substring(elements[3].IndexOf("A=") + 2, 6);
                            comment = elements[3].Substring(8);
                        }

                        // Srccall, Lat, Lng, Hae, Lasttime, Comment);
                        var location = new LocationObject
                        {
                            Srccall = callsign,
                            Lat = lat,
                            Lng = lng,
                            Hae = hae,
                            Lasttime = time,
                            Comment = comment
                        };

                        EmitCot(location);
                    }
                }


                     // If there are 8 elements, we hope this is an expected position message...
                else if (elements.Length == 8)
                {
                    // sample  of IGate data from file...
                    /*
                        [0]	"IGate:WinMain:2014-05-10T20:22:03.526 RFtoIS:[Mobilinkd]IGated KG6PPZ-11>KG6PPZ,WIDE2-1,qAR,KG6PPZ:"	
                        [1]	"202201h3635.49N"	
                        [2]	"08716.31WO000"	
                        [3]	"000"	
                        [4]	"A=000553"	
                        [5]	"Ti=32"	
                        [6]	"Te=70"	
                        [7]	"V=11429 ForceX high altitude balloon test"	                          
                             
                     */

                    var dateIndex = elements[0].IndexOf(DateTime.Now.Year.ToString());
                    if (dateIndex >= 0)
                    {
                        var spaceIndex = elements[0].IndexOf(' ');
                        var thisTime = elements[0].Substring(dateIndex, elements[0].IndexOf(' ') - dateIndex);

                        // TODO: if we are going to process the line, log it
                        UpdateOutput("Processing log file line: " + line);

                        //Get callsign -  need entire callsign because chase car and balloon use same one
                        // with different extension
                        var callsignIndex = elements[0].IndexOf(textBoxCallsign.Text);
                        if (callsignIndex >= 0)
                        {
                            var endCallsignIndex = elements[0].IndexOf('>');
                            callsign = elements[0].Substring(callsignIndex, endCallsignIndex - callsignIndex);
                        }

                        var index = elements[1].IndexOf('h');
                        if (index > 0)
                        {
                            var rawTime = elements[1].Substring(0, index);
                            var rawLat = elements[1].Substring(index + 1);

                            // Get raw lat into a decimal string
                            var degrees = int.Parse(rawLat.Substring(0, 2));
                            var decimalMinutes = decimal.Parse(rawLat.Substring(2, rawLat.Length - 3));

                            // limit to 5 decimal places
                            var decimalDegrees = degrees + (decimalMinutes / 60);

                            lat = decimalDegrees.ToString("F5", System.Globalization.CultureInfo.InvariantCulture);

                            if (rawTime.Length == 6)
                            {
                                var hour = int.Parse(rawTime.Substring(0, 2));
                                var minute = int.Parse(rawTime.Substring(2, 2));
                                var second = int.Parse(rawTime.Substring(4, 2));
                                DateTime msgTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);

                                time = msgTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
                            }
                        }

                        // Assuming western hemisphere...
                        index = elements[2].IndexOf('W');
                        if (index > 0)
                        {
                            var rawLon = elements[2].Substring(0, index);
                            var degrees = int.Parse(rawLon.Substring(0, 3));
                            var decimalMinutes = decimal.Parse(rawLon.Substring(3));
                            var decimalDegrees = degrees + (decimalMinutes / 60);
                            // Need negative longitude for W hemisphere
                            decimalDegrees = decimalDegrees - (2 * decimalDegrees);

                            //limit to 5 decimal places 
                            lng = decimalDegrees.ToString("F5", System.Globalization.CultureInfo.InvariantCulture);
                        }

                        // parse altitude
                        if (!string.IsNullOrEmpty(elements[4]))
                        {
                            hae = elements[4].Substring(elements[4].IndexOf("A=") + 2);
                        }

                        //TODO: looks like there is another field before the comment... what is it?
                        comment = elements[7];

                        // Srccall, Lat, Lng, Hae, Lasttime, Comment);
                        var location = new LocationObject
                        {
                            Srccall = callsign,
                            Lat = lat,
                            Lng = lng,
                            Hae = hae,
                            Lasttime = time,
                            Comment = comment
                        };

                        EmitCot(location);

                    }
                }
                
            }
            catch (Exception ex)
            {
            }
            finally 
            {
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
            if (_fileWatchThread == null)
            {
                _pathToWatch = Path.Combine(directoryToWatch, "APRSIS32.log");
                _fileWatchThread = new Thread(new ThreadStart(ProcessWatchFile));
                _fileWatchThread.Start();
            }
        }
        
        private void ProcessWatchFile()
        { 
                using(StreamReader reader = new StreamReader(new FileStream(_pathToWatch, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    // Start at the end of the file;
                    var lastMaxOffset = reader.BaseStream.Length;

                    while (true)
                    {
                        // if the file size has not changed, do nothing
                        if (reader.BaseStream.Length == lastMaxOffset)
                        {
                            continue;
                        }
                        
                        // seek to the last max offset
                        reader.BaseStream.Seek(lastMaxOffset, SeekOrigin.Begin);
                         
                        // read the file to EOF
                        var line = reader.ReadLine();

                        while (line != null)
                        {
                            ProcessLine(line);

                            line = reader.ReadLine();
                        }

                        //reset last max offset
                        lastMaxOffset = reader.BaseStream.Position;

                        // Wait 10 seconds before checking the file again
                        System.Threading.Thread.Sleep(10000);
                    }
                }
        }
        
        
        private void WatchFileWithFileSystemWatcher(string directoryToWatch)
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
             textBoxOutput.Clear();
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
