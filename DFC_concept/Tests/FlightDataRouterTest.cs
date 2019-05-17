using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using DFC_concept.Actors;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DFC_concept.Tests
{
    class FlightDataRouterTest : TestKit
    {
        List<DataStructures.FlightReading> packets = new List<DataStructures.FlightReading>();
        TestProbe probe = null;
        IActorRef sender = null;
        IActorRef router;


        public FlightDataRouterTest()
        {
            probe = this.CreateTestProbe();
            sender = probe.TestActor;

            var lines = File.ReadAllLines("AAL2833.txt");

            foreach( var l in lines)
            {
                packets.Add(JsonConvert.DeserializeObject<DataStructures.FlightReading>(l));
            }
        }

        [Test]
        public void testIt()
        {
            var dir = ActorOf<FlightDataRouterActor>("router");

            dir.Tell(new FlightDataRouterActor.DataReceiveRequest(packets[0]));
            var r1 = ExpectMsg<DirectoryServiceActor.DirectoryLookupResponse>(TimeSpan.FromSeconds(60));
        }
    }
}