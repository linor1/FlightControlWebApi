using Microsoft.Extensions.Configuration;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;

namespace FlightControlWeb.Model
{
    public class SQLiteDBAccess
    {
        /**
         * Adds an external server to the ExternalServers table.
         * 
         * @param externURL string
         */
        public static void addExternalServer(Server exServer)
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                string query = string.Format("INSERT INTO Servers " +
                    "(ServerID, ServerURL) " +
                    "VALUES ('{0}', '{1}')", exServer.ServerId, exServer.ServerURL);
                conn.Execute(query);
            }
        }

        /**
         * Deletes an external server from the ExternalServers table by ID.
         * 
         * @param externServerId string
         */
        public static void removeExternalServer(string externServerId)
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                string query = string.Format("DELETE FROM Servers " +
                    "WHERE ServerID = {0}", externServerId);
                conn.Execute(query);
            }
        }

        /**
         * Returns a list of all external servers in the table Servers.
         */
        public static List<Server> getAllServers()
        {
            List<Server> res = new List<Server>();

            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = conn.Query<Server>("SELECT * from Servers", new DynamicParameters());
                res = output.ToList();
            }
            return res;
        }

        private static string LoadConnectionString(string id = "Default")
        {
            var configuration = new ConfigurationBuilder()
                                           .AddJsonFile("./appsettings.json")
                                           .Build();
            string connectionString = configuration.GetConnectionString("FlightControl");
            return connectionString;
        }

        public static void removeFlightById(string Id)
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                //TODO: add condition isExternal is false.
                string q1 = "DELETE FROM Flights WHERE FlightId = " + Id;
                conn.Execute(q1);
                string query = "DELETE FROM FlightPaths WHERE FlightId=" + Id;
                conn.Execute(query);
            }
        }
        public static void saveFlight(string flightId, FlightPlan flightPlan, bool isExternal)
        {
            try
            {
                int isExt = (isExternal) ? 1 : 0;
                using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
                {
                    CommandDefinition cmd = new CommandDefinition(string.Format("INSERT into Flights " +
                        "(FlightId,Passengers, " +
                        "CompanyName,Longtitude, Latitude, DateAndTime,IsExternal) " +
                        "VALUES ('{0}',{1}, '{2}',{3},{4}, '{5}',{6})",
                        flightId, flightPlan.Passengers, flightPlan.CompanyName,
                        flightPlan.InitialLocation.Longtitude,
                        flightPlan.InitialLocation.Latitude, flightPlan.InitialLocation.DateAndTime, isExt));
                    conn.Execute(cmd);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }
        public static void saveFlightPath(string flightId, FlightPlan flightPlan)
        {
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                List<Segment> segs = flightPlan.Segments.ToList();
                int segSize = segs.Count;
                for (int i = 0; i < segSize; i++)
                {
                    CommandDefinition cmd = new CommandDefinition(string.Format("INSERT INTO FlightPaths " +
                        "(FlightId, Longtitude, Latitude, TimespanSeconds, SegmentNumber)" +
                        " VALUES ('{0}',{1},{2},'{3}',{4})",
                        flightId, segs[i].Longtitude, segs[i].Latitude, segs[i].TimespanSeconds, i));
                    conn.Execute(cmd);
                }
            }
        }

        public static FlightPlan loadFlightPlan(string flightId)
        {
            FlightPlan f = new FlightPlan();
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                string query = "SELECT Passengers, CompanyName FROM Flights WHERE FlightId = " + flightId;
                var fPlan = conn.Query<FlightPlan>(query, new DynamicParameters());
                List<FlightPlan> flightPlans = fPlan.ToList();
                f = flightPlans[0];

                query = "SELECT Longtitude, Latitude, DateAndTime FROM Flights WHERE FlightId = " + flightId;
                var initialLoc = conn.Query<StartingLocation>(query, new DynamicParameters());
                f.InitialLocation = initialLoc.ToList()[0];

                query = "SELECT * FROM FlightPaths WHERE FlightId = " + flightId;
                var segments = conn.Query<Segment>(query, new DynamicParameters());
                f.Segments = segments;
            }
            return f;
        }

        public static void postFlight(Flight f)
        {
            string command = string.Format("INSERT INTO AllFlights values ('{0}',{1},{2},{3},'{4}','{5}',{6})",
                    f.FlightId, f.Longtitude, f.Latitude, f.Passengers, f.CompanyName, f.DTime, f.IsExternal);
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                conn.Execute(command);
            }
        }

        public static void linearInterpolation(DateTime currentTimeAtClient,
                                                DateTime endOfSegment,
                                                Segment currentSegment,
                                                Segment previousSegment,
                                                Flight flight)
        {
            // Using Thales' theorem
            double secsDiff = (endOfSegment - currentTimeAtClient).TotalSeconds;
            double secondsFromStart = currentSegment.TimespanSeconds - secsDiff;
            double proportion = secondsFromStart / ((double)currentSegment.TimespanSeconds);

            double newLatitude = previousSegment.Latitude +
                (currentSegment.Latitude - previousSegment.Latitude) * proportion;
            double newLongtitude = previousSegment.Longtitude +
                (currentSegment.Longtitude - previousSegment.Longtitude) * proportion;

            flight.Latitude = newLatitude;
            flight.Longtitude = newLongtitude;
        }


        public static void calculateCurrentLocation(FlightPlan flightPlan,
                                                    string relative_to,
                                                    Flight flight)
        {
            DateTime currentTimeAtClient = DateTime.ParseExact(relative_to, "yyyy-MM-ddTHH:mm:ssZ",
                                System.Globalization.CultureInfo.InvariantCulture);
            DateTime flightDateTime = DateTime.ParseExact(flightPlan.InitialLocation.DateAndTime, "yyyy-MM-ddTHH:mm:ssZ",
                                System.Globalization.CultureInfo.InvariantCulture);
            flightDateTime = flightDateTime - (new TimeSpan(2, 0, 0));
            //string flightDateTimeStr = flightPlan.InitialLocation.DateAndTime;

            //DateTimeOffset dto1 = DateTimeOffset.Parse(relative_to);
            //DateTime currentTimeAtClient = dto1.DateTime;
            //DateTimeOffset dto2 = DateTimeOffset.Parse(flightDateTimeStr);
            //DateTime flightDateTime = dto2.DateTime;

            DateTime endOfSegment = new DateTime();

            List<Segment> segs = flightPlan.Segments.ToList();
            int segNum = segs.Count();
            for (int i = 0; i < segNum; i++)
            {
                endOfSegment = flightDateTime + (new TimeSpan(0, 0, segs[i].TimespanSeconds));
                if (currentTimeAtClient < flightDateTime)
                {
                    flight.Latitude = flightPlan.InitialLocation.Latitude;
                    flight.Longtitude = flightPlan.InitialLocation.Longtitude;
                    return;
                }
                if (currentTimeAtClient < endOfSegment)
                {
                    Segment prev = new Segment();
                    if (i == 0)
                    {
                        prev.Latitude = flightPlan.InitialLocation.Latitude;
                        prev.Longtitude = flightPlan.InitialLocation.Longtitude;
                        prev.TimespanSeconds = 0;
                    }
                    else
                    {
                        prev = segs[i - 1];
                    }
                    linearInterpolation(currentTimeAtClient, endOfSegment, segs[i], prev, flight);
                    return;
                }
            }
            flight.Longtitude = segs[segNum - 1].Longtitude;
            flight.Latitude = segs[segNum - 1].Latitude;
            return;
        }

        public static List<Flight> GetAllFlights(string relative_to)
        {
            List<Flight> flights = new List<Flight>();

            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                string query = "SELECT FlightId FROM flights";
                var fids = conn.Query<string>(query, new DynamicParameters());
                List<string> flightIds = fids.ToList();
                int numOfFlights = flightIds.Count();
                for (int i = 0; i < numOfFlights; i++)
                {
                    string flightId = flightIds[i];
                    query = "SELECT Passengers, CompanyName " +
                                "FROM Flights " +
                                "WHERE FlightId = '" + flightId + "'";
                    var fp = conn.Query<FlightPlan>(query, new DynamicParameters());
                    List<FlightPlan> flightPlans = fp.ToList();

                    query = "SELECT Longtitude, Latitude, DateAndTime " +
                            "FROM Flights " +
                            "WHERE FlightId = '" + flightId + "'";
                    var initialLoc = conn.Query<StartingLocation>(query, new DynamicParameters());
                    flightPlans[0].InitialLocation = new StartingLocation();
                    flightPlans[0].InitialLocation = initialLoc.ToList()[0];
                    query = "SELECT * FROM FlightPaths WHERE FlightId = '" + flightId + "'";
                    var segments = conn.Query<Segment>(query, new DynamicParameters());
                    flightPlans[0].Segments = new List<Segment>();
                    flightPlans[0].Segments = segments;

                    //Creating the flight object
                    Flight flight = new Flight();
                    flight.FlightId = flightId;
                    if (flightPlans[0].Segments.ToList().Count() != 0)
                    {
                        calculateCurrentLocation(flightPlans[0], relative_to, flight);
                    }
                    else
                    {
                        flight.Longtitude = flightPlans[0].InitialLocation.Longtitude;
                        flight.Latitude = flightPlans[0].InitialLocation.Latitude;
                    }
                    flight.CompanyName = flightPlans[0].CompanyName;
                    flight.Passengers = flightPlans[0].Passengers;
                    flight.IsExternal = false;
                    flights.Add(flight);
                }
            }

            return flights;
        }
        public static string getURLForFlight(string flightId)
        {
            string command = "SELECT ServerURL " +
                "FROM ExtrnalFlights " +
                "WHERE FlightId = '" + flightId + "'";
            using (IDbConnection conn = new SQLiteConnection(LoadConnectionString()))
            {
                // conn.Execute(command);
                var output = conn.Query<string>(command);
                //var output = conn.Query<string>("SELECT ServerURL" +
                //                "FROM ExtrnalFlights " +
                //                "WHERE FlightId = '"+ flightId + "');
                List<string> data = new List<string>();
                data = output.ToList();
                if (data.Count > 0) { return data[0]; }
                else { return ""; }
            }
        }
    }
}
