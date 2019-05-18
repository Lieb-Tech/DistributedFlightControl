using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var icoa = probe.ActorOf<Actors.AircraftDataActor>();
            var cdb = new Services.CosmosDB();
            var packet = packets[0];
            var fh = probe.ActorOf(Actors.FlightActor.Props(packet.data.flight, cdb, icoa));

            // send first packet
            fh.Tell(new Actors.FlightActor.FlightRequest(packet));

            ExpectNoMsg(TimeSpan.FromSeconds(10));

            // go to DB and make sure that entry is correct
            var result = cdb.GetDocumentQuery<DataStructures.FlightSnapshotData>("flights")
                .Where(z => z.id == "activeSnap:" + packet.data.flight)
                .ToList();

            Assert.IsTrue(result.Count > 0);
            Assert.IsTrue(result[0].now == packet.now);
        }

        [Test]
        public void TestFlightActorDatas()
        {
            int tests = 25;

            var icoa = probe.ActorOf<Actors.AircraftDataActor>();
            var cdb = new Services.CosmosDB();
            var packet = packets[0];
            var fh = probe.ActorOf(Actors.FlightActor.Props(packet.data.flight, cdb, icoa));

            for (int i = 0; i < tests; i++)
            {
                packet = packets[i];
                fh.Tell(new Actors.FlightActor.FlightRequest(packet));
                System.Threading.Thread.Sleep(500);
            }

            ExpectNoMsg(TimeSpan.FromSeconds(5));

            // go to DB and make sure that entry is correct
            var result = cdb.GetDocumentQuery<DataStructures.FlightSnapshotData>("flights")
                .Where(z => z.id == "activeSnap:" + packet.data.flight)
                .ToList();

            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result[0].now == packet.now);
        }
    }
}
