using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FlightControlWeb.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace FlightControlWeb.Controllers
{
    //  [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        FlightsModel flightsModel;
        
        public FlightController(IFlightsModel f)
        {
            this.flightsModel = (FlightsModel)f;
        }

      
        // External server methods
        [HttpGet]
        [Route("api/servers")]
        public IEnumerable<string> getServers()
        {
            //TODO: Work with DB
            return flightsModel.serverList;
        }

        //[HttpGet]
        //[Route("api/Flights")]
        //public List<Flight> GetFromInternal(string relative_to)
        //{
        //    List<Flight> flightList = SQLiteDBAccess.GetAllFlights();
        //    return flightList;
        //}

        [HttpGet]
        [Route("/api/Flights")]
        public List<Flight> GetAllFlights(string relative_to)
        {
            List<Flight> flightsFfromServer = new List<Flight>();
            List<Flight> flightList = flightsModel.getAllFlights(relative_to);

            flightsFfromServer.AddRange(flightList);
            List<Server> serversList = this.flightsModel.getServerList();
            int length = serversList.Count;
            if (Request.QueryString.Value.Contains("sync_all") && length > 0)
            {
                using var client = new HttpClient();
                //var result;
                //call GetFromInternal method of all external servers.
                
                for (int i = 0; i < length; i++)
                {
                    try
                    {
                        var t = Task.Run(() => FlightsModel.GetURI(new Uri(serversList[0].ServerURL + "/api/Flights?relative_to=2021-10-10T12:12:12Z")));
                        t.Wait();
                  
                //    dynamic res = JObject.Parse(t.Result);
                    List<Flight> oMycustomclassname = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Flight>>(t.Result);
                    Console.WriteLine(oMycustomclassname);
                    
                    flightsFfromServer.AddRange(oMycustomclassname);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                }
            }
            return flightsFfromServer;
        }

        [HttpGet]
        [Route("api/FlightPlan/{id}")]
        public FlightPlan GetFlightById(string id)
        {
            return flightsModel.getFlightById(id);
        }

        [HttpDelete]
        [Route("api/deleteFlight/{id}")]
        public ActionResult<Flight> DeleteFlight(string id)
        {
            FlightPlan myEle = SQLiteDBAccess.loadFlightPlan(id);
            if (myEle == null)
            {
                return  NotFound();
            }
            SQLiteDBAccess.removeFlightById(id);
            return Ok();
        }


        [HttpPost]
        [Route("api/FlightPlan")]
        public void PostFlight(FlightPlan flightPlan)
        {

            flightsModel.AddFlightPlan(flightPlan, false);
        }


        [HttpPost]
        [Route("api/servers")]
        public bool addExternalServer(object json)
        {
            dynamic json1 = JObject.Parse(json.ToString());
            Server externalServer = new Server();
            externalServer.ServerId = json1.ServerId;
            externalServer.ServerURL = json1.ServerURL;

            //= JsonConvert.DeserializeObject<Server>(json1);
            int ret = flightsModel.addExternalServer(externalServer);
            //TODO: return result based on ret, if successful or failed
            return true;
            //TODO: change return type to Action Result with: return CreatedAtAction(actionName: "AddedServer", new {id =  });
        }

        [HttpDelete]
        [Route("api/servers/{id}")]
        public bool deleteExternalServer(string id)
        {
            flightsModel.deleteExternalServer(id);
            return true;
        }

     
    }
}