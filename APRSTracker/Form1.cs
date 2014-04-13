using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        #endregion

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            AprsParser.Parser.OnParsed += Parser_OnParsed;
        }

        void Parser_OnParsed(object sender, EventArgs e)
        {
            var location = sender as LocationObject;

            if (location != null)
            {
                if (!string.IsNullOrEmpty(location.Error))
                {
                    UpdateOutput("Call sign: " + location.Srccall +  " Error:  " + location.Error);
                }
                else
                {
                    UpdateOutput(location.ToString());
                }
            }
        }

        #region event handlers
 
        void Form1_Load(object sender, EventArgs e)
        {
            RestoreSettings();

            _aprsWebClient.OnResponse += _aprsWebClient_OnResponse;
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

            Properties.Settings.Default.Save();        
        }

        private void RestoreSettings()
        { 
            textBoxApiKey.Text = Properties.Settings.Default.ApiKey;
            textBoxCallsign.Text = Properties.Settings.Default.Callsign;
            textBoxCotIp.Text =  Properties.Settings.Default.CotIp ;
            textBoxCotPort.Text =  Properties.Settings.Default.CotPort ;
            checkBoxCot.Checked =   Properties.Settings.Default.EmitCot ;
            checkBoxRadio.Checked = Properties.Settings.Default.UseRadio ;
            checkBoxWeb.Checked = Properties.Settings.Default.UseWeb ;
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
                    if (Properties.Settings.Default.UseRadio)
                    {
                        DoRadioStuff();
                    }

                    if (Properties.Settings.Default.UseWeb &&
                        !string.IsNullOrEmpty(Properties.Settings.Default.Callsign) &&
                         !string.IsNullOrEmpty(Properties.Settings.Default.ApiKey))
                    {
                        DoAprsFiStuff();
                    }
                    else
                    {
                        UpdateOutput(@"Need a callsign and an API key to track via www.aprs.fi.");
                    }
                }
                Thread.Sleep(30000);
            }
        }

        private  bool ShouldDoWork()
        {
            return Properties.Settings.Default.UseRadio || 
                    Properties.Settings.Default.UseWeb;
        }

         private void DoRadioStuff()
        { 
        
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
    }
}
