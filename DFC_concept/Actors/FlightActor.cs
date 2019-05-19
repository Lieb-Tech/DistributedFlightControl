using Akka.Actor;
using DFC_concept.DataStructures;
using DFC_concept.Services;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DFC_concept.Actors
{
    class FlightActor : ReceiveActor
    {
        string flightID;
        
        // for saving to db
        IActorRef mongoActor = null;

        // when flight finished tracking, store history of snaphots
        FlightSnapshotHistory snaps;
        // most recent update
        FlightSnapshotData currentSnapshot;

        protected override void PreStart()
        {
            base.PreStart();

            // go to DB and get current data so we can append to it
            // (incase this is a crash restart)            
        }

        public FlightActor(string flightID, IMongoDatabase mongo, IActorRef icao)
        {
            this.flightID = flightID;
            mongoActor = Context.ActorOf(MongoActor.Props(mongo));

            snaps = new FlightSnapshotHistory()
            {
                flight = this.flightID,
                id = "snap:" + this.flightID,
            };

            Receive<Actors.AircraftDataActor.AircraftResponse>(r =>
            {
                snaps.icaoAircraft = r.ICAOAircraft;
            });

            Receive<Actors.AircraftDataActor.DataResponse>(r =>
            {
                snaps.hex = r.Hex;
                snaps.icaoData = r.ICAOData;
                if (!string.IsNullOrWhiteSpace(snaps.icaoData.t))
                    icao.Tell(new Actors.AircraftDataActor.AircraftRequest(r.ICAOData.t));               
            });

            // this is the 
            Receive<FlightRequest>(r =>
            {
                if (string.IsNullOrWhiteSpace(snaps.hex))
                {
                    icao.Tell(new Actors.AircraftDataActor.DataRequest(r.Reading.data.hex));
                }

                // ensure that current reading is newer than previous
                // as some packets come in out of order
                bool isOutOrder = false;
                if (currentSnapshot != null && currentSnapshot.now > r.Reading.now)
                {
                    Console.WriteLine("not new came in " + snaps.flight);
                    isOutOrder = true;
                }

                // create snapshot for basic web view 
                buildSnapshot(r.Reading.now, r.Reading.data);

                snaps.snapshots.Add(currentSnapshot);

                // make sure in time order
                snaps.snapshots = snaps.snapshots.OrderBy(z => z.now).ToList();

                // only update status if this is newer than previous
                if (!isOutOrder)
                {
                    currentSnapshot.id = "activeSnap:" + this.flightID;
                    mongoActor.Tell(new MongoActor.MongoSaveRequest<FlightSnapshotData>(currentSnapshot));
                }

                // once I get my 
                // full object info - incase web user wants to see 
                var ex = new FlightDataExtended(r.Reading.data)
                {
                    altDelta = currentSnapshot.altDelta,
                    spdDelta = currentSnapshot.spdDelta,
                    icoaAircraft = snaps.icaoAircraft,
                    icoaData = snaps.icaoData,
                };
                mongoActor.Tell(new MongoActor.MongoSaveRequest<FlightDataExtended>(ex));                
            });

        }

        private void buildSnapshot(double now, FlightData entry)
        {
            if (currentSnapshot == null)
            {
                // create initial snapshot
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
                // use previous snaphsot to get delta values
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

        public static Props Props(string flightId, IMongoDatabase mongo, IActorRef icao) =>
            Akka.Actor.Props.Create(() => new FlightActor(flightId, mongo, icao));

        #region
        /// <summary>
        /// Flight data ADS-B reading 
        /// </summary>
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
