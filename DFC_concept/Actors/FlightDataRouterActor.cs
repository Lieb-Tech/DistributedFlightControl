using Akka.Actor;
using DFC_concept.DataStructures;
using DFC_concept.Services;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFC_concept.Actors
{
    class FlightDataRouterActor :ReceiveActor
    {
        // mapping of flight -> actors 
        Dictionary<string, IActorRef> activeFlights = new Dictionary<string, IActorRef>();

        // requests that waiting on Directory service
        Dictionary<string, List<DataReceiveRequest>> waitingOnDirectory = new Dictionary<string, List<DataReceiveRequest>>();

        // directory service
        IActorRef directory = null;

        protected override void PreStart()
        {
            base.PreStart();
            directory = Context.System.ActorOf<DirectoryServiceActor>();
        }

        /// <summary>
        /// responsible for managing the messages from the receiver & sending to correct actor
        /// </summary>
        public FlightDataRouterActor(IMongoDatabase mongo)
        {
            var icao = Context.ActorOf<Actors.AircraftDataActor>();                        

            Receive<DirectoryServiceActor.DirectoryLookupResponse>(r =>
            {
                string key = r.Flight.Trim().ToUpper();

                if (r.ReservationPending)
                {
                    // nothing really to do but wait
                }
                else if (r.ResponsibleActor == null)
                {
                    // no reservation, so I will setup and register an actor
                    IActorRef fa = Context.System.ActorOf(FlightActor.Props(key, mongo, icao), key);
                    directory.Tell(new DirectoryServiceActor.DirectoryRegisterRequest(r.Flight, fa));
                }
                else
                {                   
                    // got actor responsible for the flight, so save
                    activeFlights.Add(key, r.ResponsibleActor);

                    // flush out the cache for this flight
                    if (waitingOnDirectory.ContainsKey(key))
                    {                        
                        // send pending requests, should be atleast 1
                        foreach (var d in waitingOnDirectory[key])
                        {
                            r.ResponsibleActor.Tell(new FlightActor.FlightRequest(d.Reading));
                        }
                        // clear out cache
                        waitingOnDirectory.Remove(key);
                    }
                }
            });            
            
            Receive<DataReceiveRequest>(r =>
            {
                // clean up flight
                string key = r.Reading.data.flight.Trim().ToUpper();

                if (activeFlights.ContainsKey(key))
                {
                    activeFlights[key].Tell(new FlightActor.FlightRequest(r.Reading));
                }
                else if (waitingOnDirectory.ContainsKey(key))
                {
                    waitingOnDirectory[key].Add(r);
                }
                else
                {
                    waitingOnDirectory.Add(key, new List<DataReceiveRequest>() { r });
                    directory.Tell(new DirectoryServiceActor.DirectoryLookupRequest(key));
                }
            });
        }

        public static Props Props(IMongoDatabase mongo) =>
            Akka.Actor.Props.Create(() => new FlightDataRouterActor(mongo));

        #region Messages
        /// <summary>
        /// Data from the ADS-B receiver via EventHub wrapper
        /// </summary>
        internal class DataReceiveRequest
        {            
            public DataReceiveRequest(FlightReading reading)
            {
                Reading = reading;
            }
            public FlightReading Reading { get; private set; }
        }        
        #endregion
    }
}
