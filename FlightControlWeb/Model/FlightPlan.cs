using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace FlightControlWeb.Model
{
    public class StartingLocation
    {
        [JsonPropertyName("longitude")]
        public double Longtitude { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("date_time")]
        public string DateAndTime { get; set; }
    }
    public class Segment
    {
        [JsonPropertyName("longitude")]
        public double Longtitude { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("timespan_seconds")]
        public int TimespanSeconds { get; set; }
    }

    public class FlightPlan
    {
        [JsonPropertyName("passengers")]
        public int Passengers { get; set; }

        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }

        [JsonPropertyName("initial_location")]
        public StartingLocation InitialLocation { get; set; }

        [JsonPropertyName("segments")]
        public IEnumerable<Segment> Segments { get; set; }
    }
}
