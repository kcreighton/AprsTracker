using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Location;


namespace AprsParser
{
    public class Parser
    { 
        public static event EventHandler OnParsed;

        #region sample packets
        // sample raw aprs packets
        public static string[] _samplePackets = new string[]{
                                           "UNVMTN>APN382,WIDE2-1,qAR,W7XY-10:!4651.35NS11355.36W#PHG4920/W2,MTn,UNIVERSITY MOUNTAIN WR7HLN",
                                           "KC2QVT-B>APJI23,TCPIP*,qAC,KC2QVT-BS:!3958.20ND07453.88W&RNG0040 440 Voice 445.33125 - 5MHz",
                                           "SQ6NZR>APU25N,TCPIP*,qAC,T2POLAND:>211056zUI-View32 V2.03",
                                           "DV9HKD>APU25N,TCPIP*,qAC,T2XWT:=0825.81N/12417.69E-Renoir Qth:Naawan Mindanao {UIV32}",
                                           "EW0147>APRS,TCPXX*,qAX,CWOP-4:@121657z3520.38N/09412.28W_160/006g010t077r000p007P000b09712h70.WD 166",
                                           "VE2RON-3>APCAQC,WIDE2-2,qAR,VE2RUI-3:!4810.96N/07924.26W#PHG5690 VE2RON 146.820- TONE 114.8 (CRANOQ)",
                                           "KB9WGA-2>APRS,K9KJM-10*,KA9BAB-5*,WIDE2*,qAR,KB8ZXE:$GPRMC,8,A,4458.2127,N,08720.8152,W,0.000,0.0,120,2.2, W * 0 [Invalid timestamp in GPRMC sentence]",
                                           "NC4EC-4>BEACON,qAS,W4POX-5:>TAARS E-City,NC Digi 146.655 p131.8 www.taars.us",
                                           "YM9KRT>APRS,YM9KAR,YM9KRS*,WIDE3-1,qAR,YM9KK-3:!4111.09NW04155.67E#PHG5630/SACINKA TEPE 2040m WIDEn-N ",
                                           "HOLAND>BEACON,qAR,K8TB-9:> W8FSM",
                                           "ON4AIM-9>APRS,ON0OST,TCPIP*,qAU,T2BELGIUM:!5113.68N/00255.04E(259/0.9 APRS via  DMR",
                                           "KC2MVA>BEACON,WIDE3-3,qAR,N2MH-12:!4038.82N/07415.02WoPHG5160/Roselle OEM",
                                           "LA2YUA-C>APDG02,TCPIP*,qAC,LA2YUA-CS:!5957.00ND01100.63E&RNG0001 2m Voice 144.67500MHz +0.0000MHz",
                                           "SP5MXW>APDR12,qAS,SP5MXW-7:=5204.90N/02045.41E;227/000/A=000499 Wojtek 439.600",
                                           "S53DXX-2>APD225,TCPIP*,qAI,S53DXX-2,T1EIGHTH,FIRST,T2HUB3,APRSFI-C2:!4633.83N/01538.75EI aprsd Linux APRS Server",
                                           "DH0YAH-5>APDR12,TCPIP*,qAC,T2NUERNBG:=5218.0 N/00710.9 E>193/040/A=000284 mal hier mal da tralala",
                                           "EW1959>APRS,TCPXX*,qAX,CWOP-2:@121656z2926.53N/08137.82W_105/004g009t079r000p000P000b10228h47eMB21",
                                           "CT3FU-1>APU25N,TCPIP*,qAS,CT3FU:@121655z3248.42N/01652.59W_247/002g005t059r000p000P0,00b.....h74CW7433 {UIV32N}",
                                           "NY0I-2>APNU19,qAR,KB0NLY-2:!4449.00N/09532.05W#PHG54504/Granite Falls, MN.",
                                           "BG5BNM-10>APET51,TCPIP*,qAC,T2HGH:!2805.34N/12052.12ErYUEQING APRS 144390 13.6V 56.6<0xb0>C",
                                           "IN3EQA-S>APDG01,TCPIP*,qAC,IN3EQA-GS:;IN3EQA C *121656z4601.36ND01117.73EaRNG0022 2m Voice 145.56250MHz +0.0000MHz",
                                           "IQ0OL>APU25N,IR0DZ-11,WIDE1*,WIDE2,qAR,9A1CKL-11:=4129.90N/01303.56E-AOT Latina ",
                                           "HUMBOL>APNW01,TCPIP*,qAC,T2MCI::HUMBOL   :PARM.I1,I2,U1,U2,Temp,O1,O2,O3,O4,I1,I2,I3,I4",
                                           "HUMBOL>APNW01,TCPIP*,qAC,T2MCI::HUMBOL   :UNIT.mA,mA,Volt,Volt,C,On,On,On,On,Hi,Hi,Hi,Hi",
                                           "OK4PZ-4>APOTW1,OK1KTA-2*,WIDE2-2,qAR,OK0BCA-1:!4858.77N/01427.42E_326/001g003t064P000h39b10088OTW1",
                                           "EW3053>APRS,TCPXX*,qAX,CWOP-2:@121656z3813.17N/09921.73W_223/010g016t073r000p000P000b10025h35L462eMB22",
                                           "HUMBOL>APNW01,TCPIP*,qAC,T2MCI::HUMBOL   :EQNS.0,19.5,0,0,19.5,0,0,0.1,0,0,0.1,0,0,0.5,-64",
                                           "HUMBOL>APNW01,TCPIP*,qAC,T2MCI::HUMBOL   :BITS.11111111,Telemetry test",
                                           "EL-VK3WOW>RXTLM-1,TCPIP,qAR,VK3WOW::EL-VK3WOW:UNIT.RX Erlang,TX Erlang,RXcount/10m,TXcount/10m,none1,STxxxxxx,logic",
                                           "HS8XXZ-10>APGJW5-1,WIDE1-1,WIDE4-4,qAR,HS8SDZ:!1031.65N\09906.55EP056/000/A=000247",
                                           "IW7DGY>APU25N,TCPIP*,qAC,T2ITALYS:=4106.23N/01654.79E#PHG4030/A=00098 - iw7dgy@yahoo.it {UIV32}",
                                           "CQ0PCV-3>BEACON,WIDE,TRACE,WIDE,qAR,CQ0PQC-7:ASSOCIACAO DE RADIOAMADORES DA LINHA DE CASCAIS - www.arlc.pt",
                                           "IQ2LB-11>APBPQ1,TCPIP*,qAC,IQ2LB-JS:>DIGI Limbiate(MB) - www.arilimbiate.it",
                                           "JO2SIF-10>APU25N,TCPIP*,qAC,T2FUKUOKA:>121656zDX: JI2ZTU-1 35.10.09N 137.02.68E 23.8km 187<0x00> 01:26",
                                           "EL-VK3WOW>RXTLM-1,TCPIP,qAR,VK3WOW:T#050,0.01,0.01,2,1,0.0,00000000,SimplexLogic",
                                           "NC4HC-15>APNX01,qAR,W4DJW:;146.640bw*111111z3527.51N/08221.42WrT091 R50M Net W830P Su8PM",
                                           "9W2ALS-13>APRX28,TCPIP*,qAC,MYAPRS-1:T#633,0.0,0.0,0.0,0.0,0.0,00000000",
                                           "DL2ARH-7>APRS-1,qAR,DB0HDF:!5053.40N/01151.52E_360/002g...t057r000P...h70b10152OSBYC",
                                           "HS0AC-S>APDG01,TCPIP*,qAC,HS0AC-GS:;HS0AC  C *121656z1343.75ND10027.48EaRNG0019 2m Voice 145.50000MHz +0.0000MHz",
                                           "KE6VRK-3>APRS,WIDE2-2,qAR,W6KL-2:!3359.73N/11824.48W# KE6VRK solar powered digipeater"
                                             };
        #endregion
        
        public static LocationObject Parse(string packet)
        { 
            int cs = packet.IndexOf(">");
            String source = packet.Substring(0, cs).ToUpper();
            int ms = packet.IndexOf(":");
            String digiList = packet.Substring(cs + 1, ms);
            String[] digiTemp = digiList.Split(',');
            String dest = digiTemp[0].ToUpper(); 
            String body = packet.Substring(ms + 1);

            return ParseBody(source, body);       
        }

        private static LocationObject ParseBody(string source, string body)
        { 
            LocationObject loc = new LocationObject();
            loc.Srccall = source;
            if (source == "IW7DGY")
            { 
            
            }
            var bodyChars = body.ToCharArray();
            var packetType = bodyChars[0];

            if (packetType == '=' || packetType =='/' ||  packetType =='@'  ) 
            {
                // regular position packet

                if (body.Length < 10) 
                {
                    loc.Error = "Packet is less than 10 characters. Too short.";
                }

				// Normal or compressed location packet, with or without
				// timestamp, with or without messaging capability
				// ! and / have messaging, / and @ have a prepended
				// timestamp

				var cursor = 1;

				if (packetType == '/' || packetType == '@') 
                {
					//Process timestamp.
                    loc.Lasttime = ParseTime(body);
					cursor += 7;
				}
				char posChar = (char) bodyChars[cursor];
			
                if ('0' <= posChar && posChar <= '9') 
                {
                    // Latitude:
                    /*Latitude is expressed as a fixed 8-character field, in degrees and decimal
                    minutes (to two decimal places), followed by the letter N for north or S for
                    south.
                     Latitude degrees are in the range 00 to 90. Latitude minutes are expressed as
                    whole minutes and hundredths of a minute, separated by a decimal point.
                     For example:
                     4903.50N is 49 degrees 3 minutes 30 seconds north.
                     In generic format examples, the latitude is shown as the 8-character string
                    ddmm.hhN (i.e. degrees, minutes and hundredths of a minute north).*/

                    var rawLat = body.Substring(cursor, 8);
                    loc.Lat = rawLat.Insert(2, " ");

                    //Longitude
                    /*Longitude is expressed as a fixed 9-character field, in degrees and decimal
                    minutes (to two decimal places), followed by the letter E for east or W for
                    west.
                     Longitude degrees are in the range 000 to 180. Longitude minutes are
                    expressed as whole minutes and hundredths of a minute, separated by a
                    decimal point.
                     For example:
                     07201.75W is 72 degrees 1 minute 45 seconds west.
                     In generic format examples, the longitude is shown as the 9-character string
                    dddmm.hhW (i.e. degrees, minutes and hundredths of a minute west).
                     */
                    var rawLng = body.Substring(cursor + 9, 9);
                    loc.Lng = rawLng.Insert(3, " ");
				}   
                else // compressed packet
                {
                             //	if (validSymTableCompressed(posChar)) { /* [\/\\A-Za-j] */
					        // compressed position packet
			        //		position = PositionParser.parseCompressed(msgBody, cursor);
			        //		this.extension = PositionParser.parseCompressedExtension(msgBody, cursor);
			        ////		positionSource = "Compressed";
                    loc.Error = "Compressed packet. I don't know how to handle that yet.";
                }
            }
            else if(packetType == '`' || packetType == '\\')
            {
                // MIC-e packet
                loc.Error = "MIC-e packet. I don't know how to handle that yet.";
            }
            else if (packetType == '!')
            {
                //Ultimeter 2000 weather
                // instrument packet
                loc.Error = "Ultimeter 2000 packet. I don't think I want to know how to handle that packet type.";
            }
            else if (packetType == '$')
            {
                // NMEA packet

                loc.Error = "NMEA packet. I don't know how to handle that yet.";
            }  
            else 
            {
                // Not a position message
                 
                loc.Error = "Packet does not appear to be a position message.";
            }
        
            return loc;
        }

        private static string ParseTime(string body)
        {
            var result = "";
            var timeCode = body.Substring(1, 7);

            // from the APRS protocol document:
           /* Day/Hours/Minutes (DHM) format is a fixed 7-character field, consisting of
            a 6-digit day/time group followed by a single time indicator character (z or
            /). The day/time group consists of a two-digit day-of-the-month (01–31) and
            a four-digit time in hours and minutes.
             Times can be expressed in zulu (UTC/GMT) or local time. For example:
             092345z is 2345 hours zulu time on the 9th day of the month.
            092345/ is 2345 hours local time on the 9th day of the month.
             It is recommended that future APRS implementations only transmit zulu
            format on the air.
             Note: The time in Status Reports may only be in zulu format. */
 
            if (timeCode.EndsWith("z")) // zulu day+hours+minutes
            {
                var dayStr = timeCode.Substring(0, 2);
                var hourStr = timeCode.Substring(2, 2);
                var minuteStr= timeCode.Substring(4, 2);

                var month = DateTime.Today.Month;
                if (UsePreviousMonth(dayStr))
                {
                    month--;
                }

                //TODO check for previous year?????
                var day = int.Parse(dayStr); ;
                var hour = int.Parse(hourStr);
                var minute = int.Parse(minuteStr);

                var dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, day, hour, minute, 0);

                // want time string in this format: (But where does the T come from?)
                //2014-04-07T22:52:48.61Z 
                result = dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
            else if (timeCode.EndsWith("//")) // local day+hours+minutes
            {
                //TODO - process the date.
                var dayStr = timeCode.Substring(0, 2);
                var hourStr = timeCode.Substring(2, 2);
                var minuteStr = timeCode.Substring(4, 2);

                var month = DateTime.Today.Month;
                if (UsePreviousMonth(dayStr))
                {
                    month--;
                }
                //TODO check for previous year?????
                var day = int.Parse(dayStr);
                var hour = int.Parse(hourStr);
                var minute = int.Parse(minuteStr);

                var dateTime = new DateTime(DateTime.Now.Year, month, day, hour, minute, 0);

                // want time string in this format: (But where does the T come from?)
                //2014-04-07T22:52:48.61Z 
                result = dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
            else
            {
                /*Hours/Minutes/Seconds (HMS) format is a fixed 7-character field,
               consisting of a 6-digit time in hours, minutes and seconds, followed by the h
               time-indicator character. For example:
                234517h is 23 hours 45 minutes and 17 seconds zulu */

                var hourStr = timeCode.Substring(0, 2);
                var minuteStr = timeCode.Substring(2, 2);
                var secondStr = timeCode.Substring(4, 2);

                var hour = int.Parse(hourStr);
                var minute = int.Parse(minuteStr);
                var second = int.Parse(secondStr);

                var dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);

                // want time string in this format: (But where does the T come from?)
                //2014-04-07T22:52:48.61Z 
                result = dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
            return result;
        }

        public static bool UsePreviousMonth(string day)
        {
            var parseDay = int.Parse(day);
            var now = DateTime.Now.Day;

            // First pass at guessing whether the day was in a previous month.  
            // If it is the first of the month and the track was generated before the changeover.
            return parseDay - now >= 2;
        }

        public static void TestParse()
        {
            foreach (var packet in _samplePackets)
            {
                var location = Parser.Parse(packet);

                if (OnParsed != null)
                {
                    OnParsed.Invoke(location, EventArgs.Empty);
                }
            }
        }
    }
}
