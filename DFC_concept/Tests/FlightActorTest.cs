using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace DFC_concept.Tests
{
    class FlightActorTest : TestKit
    {
        List<DataStructures.FlightReading> packets = new List<DataStructures.FlightReading>();
        TestProbe probe => this.CreateTestProbe();
        IActorRef sender => probe.TestActor;

        public FlightActorTest()
        {
            var lines = File.ReadAllLines("AAL2833.txt");
            foreach (var l in lines)
            {
                packets.Add(JsonConvert.DeserializeObject<DataStructures.FlightReading>(l));
            }
        }

        [Test]
        public void TestFlightActor()
        {
            var client = new MongoClient(Services.MongoService.ConnectionString);
            var database = client.GetDatabase("liebfeeds");

            var icoa = probe.ActorOf<Actors.AircraftDataActor>();
            var packet = packets[0];
            var fh = probe.ActorOf(Actors.FlightActor.Props(packet.data.flight, database, icoa));

            // send first packet
            fh.Tell(new Actors.FlightActor.FlightRequest(packet));

            ExpectNoMsg(TimeSpan.FromSeconds(30));
            
            var col = database.GetCollection<BsonDocument>("snaps");
            var result = col.FindSync<BsonDocument>(new BsonDocument("_id", "activeSnap:" + packet.data.flight)).ToList();
            Assert.IsTrue(result.Count > 0);
            Assert.IsTrue(result[0].Where(z => z.Name == "now").First().Value == packet.now);          
        }

        [Test]
        public void TestFlightActorDatas()
        {
            int tests = 25;
            var client = new MongoClient(Services.MongoService.ConnectionString);
            var database = client.GetDatabase("liebfeeds");

            var icoa = probe.ActorOf<Actors.AircraftDataActor>();
            var cdb = new Services.CosmosDB();
            var packet = packets[0];
            var fh = probe.ActorOf(Actors.FlightActor.Props(packet.data.flight, database, icoa));

            // run several items through the processor
            for (int i = 0; i < tests; i++)
            {
                packet = packets[i];
                fh.Tell(new Actors.FlightActor.FlightRequest(packet));
                System.Threading.Thread.Sleep(750);
            }

            ExpectNoMsg(TimeSpan.FromSeconds(3));

            var col = database.GetCollection<BsonDocument>("snaps");
            // direct ID lookup
            var result = col.FindSync<BsonDocument>(new BsonDocument("_id", "activeSnap:" + packet.data.flight)).ToList();
            Assert.IsTrue(result.Count > 0);
            Assert.IsTrue(result[0].Where(z => z.Name == "now").First().Value == packet.now);

            var col2 = database.GetCollection<DataStructures.FlightDataExtended>("flights");
            // "like" match ; with sorting 
            var id = $"flight:{packet.data.flight}:{packet.data.hex}";
            var result2 = col2.Find(z => z.id.Contains(id))
                .SortByDescending(z => z.seen)
                .FirstOrDefault();
            // ensure a match was found
            Assert.IsNotNull(result2);
            // make sure the correct item was returned
            Assert.IsTrue(result2.seen == packet.data.seen);
            Assert.IsTrue(result2.lat == packet.data.lat);
            Assert.IsTrue(result2.lon == packet.data.lon);

        }
    }
}
