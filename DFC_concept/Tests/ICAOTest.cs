using Akka.Actor;
using Akka.TestKit.NUnit;
using DFC_concept.Actors;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFC_concept.Tests
{
    class ICAOTest : TestKit
    {
        IActorRef data = null;
        public ICAOTest()
        {
            data = ActorOf<AircraftDataActor>("test");
        }
        [Test]
        public void ICAOHexFail()
        {
            data.Tell(new AircraftDataActor.DataRequest("ABC12"));
            var r1 = ExpectMsg<AircraftDataActor.DataResponse>(TimeSpan.FromSeconds(20));
            Assert.That(r1.ICAOData == null);
            Assert.That(r1.Hex == "ABC12");
        }

        [Test]
        public void ICAOHexPass()
        {
            data.Tell(new AircraftDataActor.DataRequest("A0B20E"));
            var r1 = ExpectMsg<AircraftDataActor.DataResponse>(TimeSpan.FromSeconds(20));
            Assert.That(r1.ICAOData != null);
            Assert.That(r1.Hex == "A0B20E");
        }

        [Test]
        public void ICAOPlaneFail()
        {
            data.Tell(new AircraftDataActor.AircraftRequest("ABC12"));
            var r1 = ExpectMsg<AircraftDataActor.AircraftResponse>(TimeSpan.FromSeconds(20));
            Assert.That(r1.ICAOAircraft == null);
            Assert.That(r1.Craft == "ABC12");
        }

        [Test]
        public void ICAOPlanePass()
        {
            data.Tell(new AircraftDataActor.AircraftRequest("B763"));
            var r1 = ExpectMsg<AircraftDataActor.AircraftResponse>(TimeSpan.FromSeconds(20));
            Assert.That(r1.ICAOAircraft != null);
            Assert.That(r1.Craft == "B763");
            Assert.That(r1.ICAOAircraft.wtc == "H");
            Assert.That(r1.ICAOAircraft.desc== "L2J");
        }

        // 
    }
}
