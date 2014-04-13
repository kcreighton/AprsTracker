using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http; 
using System.Threading.Tasks;

namespace AprsWebClient
{
    public class AprsClient
    {
        public event EventHandler OnResponse;

        private static string _myKey = "63756.G9o4o2KEXhgcj";
        private static string _addressFormat = @"http://api.aprs.fi/api/get?name={0}&what=loc&apikey={1}&format=json";

        public string ApiKey
        {
            get { return _myKey; }
            set { _myKey = value; }
        }

        public void GetLastLocation(string callSign)
        {
            var url = string.Format(_addressFormat, callSign, _myKey);
            Task<string> result = RunClient(url);
            string resultAsText = result.Result;

            if (OnResponse != null)
            {
                OnResponse.Invoke(resultAsText, EventArgs.Empty);
            }
        }

        static async Task<string> RunClient(string url)
        {
            HttpClient client = new HttpClient();

            // Send asynchronous request
            HttpResponseMessage response = await client.GetAsync(url);

            // Check that response was successful or throw exception
            response.EnsureSuccessStatusCode();

            var resp = response.Content;

            return await response.Content.ReadAsStringAsync();
        }
         
    }
}
