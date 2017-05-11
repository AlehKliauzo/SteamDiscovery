using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Steam
{
    class WebClientWithDelay
    {
        public int Delay { get; set; }

        private WebClient webClient;
        private DateTime? lastDownload;

        public WebClientWithDelay()
        {
            Delay = 1000;

            webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            webClient.Headers.Add("Cookie", "mature_content=1; birthtime=-2208994788;");
        }

        public string DownloadString(string url)
        {
            while (lastDownload != null && lastDownload.Value.AddMilliseconds(Delay) > DateTime.Now)
            {
                var sleepFor = lastDownload.Value.AddMilliseconds(Delay) - DateTime.Now;
                if (sleepFor.TotalMilliseconds > 0)
                {
                    Thread.Sleep(sleepFor);
                }
            }

            var result = webClient.DownloadString(url);
            lastDownload = DateTime.Now;
            return result;
        }
    }
}
