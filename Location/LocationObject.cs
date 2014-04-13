using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location
{
    public class LocationContainer
    {
        public string Command {get; set;}
        public string Result {get; set;}
        public string What {get; set;}
        public string Found {get; set;}
        public LocationObject[] Entries {get; set;}
        
        // This is a typical JSON string returned from APRS.fi
      //  {"command":"get","result":"ok","what":"loc","found":1,"entries":
        //[{"class":"a","name":"KG4CKR-2","type":"l","time":"1364003605","lasttime":"1397315940","lat":"36.86100","lng":"-87.47650","symbol":"S#","srccall":"KG4CKR-2","dstcall":"APN391","phg":"5150","comment":"W2, KYn-N Digi in Hopkinsville, KY","path":"N4CKV-1,NT4UX-3,N9CVA-10,WIDE2*,qAR,WB9TLH-10"}]}
    }

    public class LocationObject
    {
        // System.DateTime equivalent to the UNIX Epoch.
        System.DateTime _epochDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);

        public LocationObject()
        {
            Hae = "0";
            Error = "";
        }

        public void AdjustTimesForWindows()
        {
            //TODO
            //These times are off by an hour because aprs.fi reports these times as if they were in the Eastern Time Zone,
            // but W KY and W TN are in the Central Time Zone.


            // Add the number of seconds in UNIX timestamp to be converted.
           var  time = _epochDateTime.AddSeconds(int.Parse(Time));

            // The dateTime now contains the right date/time so to format the string,
            // use the standard formatting methods of the DateTime object.

            //TODO adjust these time formats to match CoT format
            Time = time.ToShortDateString() + " " + time.ToShortTimeString();

            var lastTime = _epochDateTime.AddSeconds(int.Parse(Lasttime));

            Lasttime = lastTime.ToShortDateString() + " " + lastTime.ToShortTimeString();
        }

        public string Class { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Time { get; set; }
        public string Lasttime { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }
        public string Symbol { get; set; }
        public string Srccall { get; set; }
        public string Dstcall { get; set; }
        public string Phg { get; set; }
        public string Comment { get; set; }
        public string Path { get; set; }
        public string Hae { get; set; }
        public string Error { get; set; }

        public override string ToString()
        {
            return string.Format("Call sign: {0} at ({1}, {2})  Hae = {3}. Last Time: {4} Comment: {5}", Srccall, Lat, Lng, Hae, Lasttime, Comment);
        }
    }

    
}
