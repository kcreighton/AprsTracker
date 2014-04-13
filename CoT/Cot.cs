using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Location;

namespace CoT
{
    public class CotGenerator 
    {  
        //ToDo do we need flowtags? I don't think so...
        //private string _cotTemplate =  "<?xml version=\"1.0\" standalone=\"yes\"?><event version=\"2.0\" uid=\"{0}\" type=\"a-f-A\" how=\"m-r\" time=\"{1}\" start=\"{1}\" stale=\"{2}\"><point lat=\"{3}\" lon=\"{4}\" ce=20\" le=\"20\" hae=\"{5}\" /><detail><_flow-tags_ SCG=\"2014-04-07T22:53:27.95Z\" /></detail></event>\"";

        private static string _cotTemplate = "<?xml version=\"1.0\" standalone=\"yes\"?><event version=\"2.0\" uid=\"{0}\" type=\"a-f-A\" how=\"m-r\" time=\"{1}\" start=\"{1}\" stale=\"{2}\"><point lat=\"{3}\" lon=\"{4}\" ce=20\" le=\"20\" hae=\"{5}\" /></event>\"";
         
        public static string GetCot(string uid, string time, string lat, string lng, string hae)
        {
            // want time string in this format:
            //2014-04-07T22:52:48.61Z 

            return string.Format(_cotTemplate, uid, time, time, GetStaleTime(), lat, lng, hae);
        }

        public static string GetCot(LocationObject location)
        {
            // Do we want to add anything to the callsign for the ID?
            return string.Format(_cotTemplate, location.Srccall, location.Lasttime, location.Lasttime, GetStaleTime(), location.Lat, location.Lng, location.Hae);
        }

        private static string GetStaleTime()
        {
            // what is the T in timestring?
           // return DateTime.UtcNow.ToString("yyyy-mm-ddTHH:")

            var dateTime = DateTime.UtcNow + new TimeSpan(1,0,0);
            var strReturn = string.Format("{0:s}", dateTime);
            return strReturn;
        }
    }
}
 