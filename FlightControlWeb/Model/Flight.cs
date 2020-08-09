using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FlightControlWeb.Model
{
    public class Flight
    {
        private string flightId;
        private double longtitude;
        private double latitude;
        private int passengers;
        private string companyName;
        private string dateTime;
        private bool isExternal;

        [JsonPropertyName("flight_id")]
        public string FlightId
        {
            get
            {
                return flightId;
            }
            set
            {
                flightId = value;
            }
        }
        [JsonPropertyName("longtitude")]
        public double Longtitude
        {
            get
            {
                return longtitude;
            }
            set
            {
                longtitude = value;
            }
        }
        [JsonPropertyName("latitude")]
        public double Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                latitude = value;
            }
        }
        [JsonPropertyName("passengers")]
        public int Passengers
        {
            get
            {
                return passengers;
            }
            set
            {
                passengers = value;
            }
        }
        [JsonPropertyName("date_time")]
        public string DTime
        {
            get
            {
                return dateTime;
            }
            set
            {
                dateTime = value;
            }
        }
        [JsonPropertyName("company_name")]
        public string CompanyName
        {
            get
            {
                return companyName;
            }
            set
            {
                companyName = value;
            }
        }
        [JsonPropertyName("is_external")]
        public bool IsExternal
        {
            get
            {
                return isExternal;
            }
            set
            {
                isExternal = value;
            }
        }
    }
}
