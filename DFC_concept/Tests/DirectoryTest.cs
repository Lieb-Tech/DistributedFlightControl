using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using DFC_concept.Actors;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFC_concept.Tests
{
    [TestFixture]
    public class DirectoryTest : TestKit
    {
        TestProbe probe = null;
        IActorRef sender = null;
        public DirectoryTest()
        {
            probe = this.CreateTestProbe();
            sender = probe.TestActor;
        }

        /// <summary>
        /// ensure that constructor perameters are being set
        /// </summary>
        [Test]       
        public void TestMessages()
        {
            var msg1 = new DirectoryServiceActor.DirectoryLookupRequest("ABC123");
            Assert.That(msg1.Flight == "ABC123");

            var msg2 = new DirectoryServiceActor.DirectoryRegisterRequest("ABC123", sender);
            Assert.That(msg2.Flight == "ABC123");
            Assert.That(msg2.Actor == sender);            
        }

        /// <summary>
        /// 1 actor checking if active actor , and then doing a registration
        /// </summary>
        [Test]
        public void TestRegistration()
        {
            var dir = ActorOf<DirectoryServiceActor>("test");

            dir.Tell(new DirectoryServiceActor.DirectoryLookupRequest("ABC123"));
            var r1 = ExpectMsg<DirectoryServiceActor.DirectoryLookupResponse>(TimeSpan.FromSeconds(10));
            Assert.That(!r1.ReservationPending);
            Assert.That(r1.ResponsibleActor == null);

            dir.Tell(new DirectoryServiceActor.DirectoryRegisterRequest("ABC123", sender));
            var r2 = ExpectMsg<DirectoryServiceActor.DirectoryLookupResponse>(TimeSpan.FromSeconds(10));
            Assert.That(r2.ReservationPending == false);
            Assert.That(r2.Flight == "ABC123");
            Assert.IsNotNull(r2.ResponsibleActor);
            Assert.That(r2.ResponsibleActor.Path == sender.Path);
        }

        /// <summary>
        /// 1 actor checks (thus reserving the flight), another actor checks -- should get a registration in progress
        /// </summary>
        [Test]
        public void TestRegistrationInProcess()
        {
            var dir = ActorOf<DirectoryServiceActor>("test");
            var sender2 = probe.TestActor;

            dir.Tell(new DirectoryServiceActor.DirectoryLookupRequest("ABC123"));
            var r1 = ExpectMsg<DirectoryServiceActor.DirectoryLookupResponse>(TimeSpan.FromSeconds(10));
            Assert.That(!r1.ReservationPending);
            Assert.That(r1.ResponsibleActor == null);

            dir.Tell(new DirectoryServiceActor.DirectoryLookupRequest("ABC123"));
            var r2 = ExpectMsg<DirectoryServiceActor.DirectoryLookupResponse>(TimeSpan.FromSeconds(10));
            Assert.That(r2.ReservationPending);
            Assert.That(r2.ResponsibleActor == null);
        }

        /// <summary>
        /// 1 actor checks (thus reserving the flight) and 2nd actor checks -- should get a registration in progress
        /// then 1st actor sends registration, and 2nd actor should get inprogress = fals
        /// </summary>
        [Test]
        public void TestRegistrationInProcessAndThenRegistered()
        {
            var dir = ActorOf<DirectoryServiceActor>("test");
            var sender2 = probe.TestActor;

            dir.Tell(new DirectoryServiceActor.DirectoryLookupRequest("ABC123"));
            var r1 = ExpectMsg<DirectoryServiceActor.DirectoryLookupResponse>(TimeSpan.FromSeconds(20));
            Assert.That(!r1.ReservationPending);
            Assert.That(r1.ResponsibleActor == null);

            dir.Tell(new DirectoryServiceActor.DirectoryLookupRequest("ABC123"));
            var r2 =  ExpectMsg<DirectoryServiceActor.DirectoryLookupResponse>(TimeSpan.FromSeconds(20)); 
            Assert.That(r2.ReservationPending);
            Assert.That(r2.ResponsibleActor == null);

            dir.Tell(new DirectoryServiceActor.DirectoryRegisterRequest("ABC123", sender));
            var r3 = ExpectMsg<DirectoryServiceActor.DirectoryLookupResponse>(TimeSpan.FromSeconds(20));
            Assert.That(!r3.ReservationPending);
            Assert.That(r3.ResponsibleActor == sender);

            dir.Tell(new DirectoryServiceActor.DirectoryLookupRequest("ABC123"));
            var r4 = ExpectMsg<DirectoryServiceActor.DirectoryLookupResponse>(TimeSpan.FromSeconds(20));
            Assert.That(!r4.ReservationPending);
            Assert.That(r4.ResponsibleActor == r3.ResponsibleActor);
            Assert.That(r4.ResponsibleActor == sender);
        }
    }
}
