using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FlightControlWeb.Model
{
    public class FlightsModel : Model.IFlightsModel
    {

        public int addExternalServer(Server externalServer)
        {
            try
            {
                SQLiteDBAccess.addExternalServer(externalServer);
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }

        public int deleteExternalServer(string serverId)
        {
            try
            {
                SQLiteDBAccess.removeExternalServer(serverId);
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }

        public List<Server> getServerList()
        {
            return SQLiteDBAccess.getAllServers();
        }

        public List<Flight> getAllFlights(string relative_to)
        {
            List<Flight> allFlights = SQLiteDBAccess.GetAllFlights(relative_to);
            return allFlights;
        }
        public int AddFlightPlan(FlightPlan flightPlan, bool isExternal)
        {
            string flightId = calculateFlightId(flightPlan);
            try
            {
                SQLiteDBAccess.saveFlight(flightId, flightPlan, isExternal);
                SQLiteDBAccess.saveFlightPath(flightId, flightPlan);
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }
        public string calculateFlightId(FlightPlan flightPlan)
        {
            string initial = "sf45" + flightPlan.InitialLocation.DateAndTime + "53cd" + flightPlan.CompanyName;
            // byte array representation of that string
            byte[] encodedPassword = new UTF8Encoding().GetBytes(initial);
            // string representation (similar to UNIX format)
            // need MD5 to calculate the hash
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
            string fid = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower().Substring(0, 7);
            return fid;
        }
        public static async Task<string> GetURI(Uri u)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                HttpResponseMessage result = await client.GetAsync(u);
                if (result.IsSuccessStatusCode)
                {
                    response = await result.Content.ReadAsStringAsync();
                }
            }
            return response;
        }

        public FlightPlan getFlightById(string id)
        {
            try
            {
                FlightPlan flightPlan = SQLiteDBAccess.loadFlightPlan(id);
                return flightPlan;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                string URL = SQLiteDBAccess.getURLForFlight(id);
                if (URL != string.Empty)
                {
                    var t = Task.Run(() => GetURI(new Uri(URL + "api/FlightPlan/{id}")));
                    t.Wait();
                    //    dynamic res = JObject.Parse(t.Result);
                    FlightPlan flightPlan = Newtonsoft.Json.JsonConvert.DeserializeObject<FlightPlan>(t.Result);
                    return flightPlan;
                }
                else
                {
                    return null;
                }
            }

        }
    }
}
