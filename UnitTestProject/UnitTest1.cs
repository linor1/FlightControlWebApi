using FlightControlWeb.Model;
using System;
using Xunit;

namespace UnitTestProject
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.True(true);
        }
        [Fact]
        public void Test2()
        {
            Flight flight = new Flight();
            DateTime currentTimeAtClient = new DateTime(1588, 9, 21, 9, 0, 30);
            DateTime endOfSegment = new DateTime(1588, 9, 21, 9, 1, 0);

            //Creating a right triangle
            Segment currentSegment = new Segment();
            currentSegment.Longtitude = 4;
            currentSegment.Latitude = 3;
            currentSegment.TimespanSeconds = 60;

            Segment previousSegment = new Segment();
            previousSegment.Longtitude = 0;
            previousSegment.Latitude = 0;

            SQLiteDBAccess.linearInterpolation(currentTimeAtClient,
                                                endOfSegment,
                                                currentSegment,
                                                previousSegment,
                                                flight);

            Assert.Equal(2, flight.Longtitude, 12);
            Assert.Equal(1.5, flight.Latitude, 12);
        }
    }
}
