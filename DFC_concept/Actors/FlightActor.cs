using Akka.Actor;
using DFC_concept.DataStructures;
using DFC_concept.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DFC_concept.Actors
{
    class FlightActor : ReceiveActor
    {
        string flightID;
        
        IActorRef cosmosActor = null;
        IActorRef router = null;
        FlightSnapshotHistory snaps = new FlightSnapshotHistory();
        FlightSnapshotData currentSnapshot;

        protected override void PreStart()
        {
            base.PreStart();

            // go to DB and get current data so we can append to it
            // (incase this is a crash restart)            
        }

        public FlightActor(string flightID, CosmosDB cosmos)
        {
            this.flightID = flightID;
            cosmosActor = Context.ActorOf(CosmosSaveActor.Props(cosmos));            

            Receive<FlightRequest>(r =>
            {
                bool isOutOrder = false;
                if (router == null)
                    router = Sender;

                if (currentSnapshot != null && currentSnapshot.now > r.Reading.now)
                {
                    Console.WriteLine("not new came in " + snaps.flight);
                    isOutOrder = true;
                }

                buildSnapshot(r.Reading.now, r.Reading.data);
                snaps.snapshots.Add(currentSnapshot);
                // make sure in order
                snaps.snapshots = snaps.snapshots.OrderBy(z => z.now).ToList();

                if (!isOutOrder)
                {
                    currentSnapshot.id = "activeSnap:" + snaps.flight;
                    cosmosActor.Tell(new CosmosSaveRequest("flights", currentSnapshot));
                }                

                // full object archive
                var ex = new FlightDataExtended(r.Reading.data)
                {
                    altDelta = currentSnapshot.altDelta,
                    spdDelta = currentSnapshot.spdDelta,
                    icoaAircraft = snaps.icaoAircraft,
                    icoaData = snaps.icaoData,
                };
                cosmosActor.Tell(new CosmosSaveRequest("flights", ex));
            });

        }

        private void buildSnapshot(double now, FlightData entry)
        {
            if (currentSnapshot == null)
            {
                currentSnapshot = new FlightSnapshotData()
                {
                    now = now,
                    spdDelta = 0,
                    altDelta = 0,
                    spd = entry.gs ?? -1,
                    lat = entry.lat ?? -9999,
                    lon = entry.lon ?? -9999,
                    alt = entry.alt_baro,
                    track = entry.track.Value,
                };
            }
            else
            {
                currentSnapshot = new FlightSnapshotData()
                {
                    now = now,
                    spdDelta = (entry.gs.HasValue ? entry.gs.Value - currentSnapshot.spd : -1),
                    altDelta = entry.alt_baro - currentSnapshot.alt,
                    spd = entry.gs ?? -1,
                    lat = entry.lat ?? -9999,
                    lon = entry.lon ?? -9999,
                    alt = entry.alt_baro,
                    track = entry.track.Value,
                };
            }
        }

        public static Props Props(string flightId, CosmosDB cosmos) =>
            Akka.Actor.Props.Create(() => new FlightActor(flightId, cosmos));

        #region
        internal class FlightRequest
        {
            public FlightRequest(FlightReading reading)
            {
                Reading = reading;
            }
            public FlightReading Reading { get; private set; }
        }
        #endregion
    }
}
