using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
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
        void TestActor()
        {
            var fh = probe.ActorOf(Actors.FlightActor.Props("", null));

            fh.Tell(new Actors.FlightActor.FlightRequest(packets[0]));
        }
    }
}
