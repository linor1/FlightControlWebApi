using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Model
{
    public class Server
    {
        private string URL;
        private string Id;

        [JsonProperty("ServerID")]
        public string ServerId
        {
            get
            {
                return Id;
            }
            set
            {
                Id = value;
            }
        }
        [JsonProperty("ServerURL")]
        public string ServerURL
        {
            get
            {
                return URL;
            }
            set
            {
                URL = value;
            }
        }
    }
}
