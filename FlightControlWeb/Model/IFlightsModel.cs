using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Model
{
    public class IFlightsModel
    {
        public volatile List<Model.Flight> flightList;
        public volatile List<string> serverList;
       
    }
}
