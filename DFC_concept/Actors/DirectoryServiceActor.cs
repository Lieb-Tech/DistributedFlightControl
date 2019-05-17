using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DFC_concept.Actors
{
    class DirectoryServiceActor : ReceiveActor
    {
        public DirectoryServiceActor()
        {
            // list of active & registered  flight / actor 
            Dictionary<string, IActorRef> tracked = new Dictionary<string, IActorRef>();

            // no active processor, inital requester will become the processor (first entry), others will get notified when processor is ready
            Dictionary<string, List<IActorRef>> pendingRegister = new Dictionary<string, List<IActorRef>>();

            // register actor to handle flight
            Receive<DirectoryRegisterRequest>(r =>
            {
                var cleaned = cleanFlight(r.Flight);

                // if it's already registerd
                if (tracked.ContainsKey(cleaned))
                {
                    Sender.Tell(new DirectoryLookupResponse()
                    {
                        Flight = r.Flight,
                        ReservationPending = false,
                        ResponsibleActor = tracked[cleaned]
                    });
                }
                else
                {
                    // save Sender as the active processor
                    tracked.Add(cleaned, r.Actor);

                    var response = new DirectoryLookupResponse()
                    {
                        Flight = r.Flight,
                        ReservationPending = false,
                        ResponsibleActor = r.Actor
                    };

                    // now let other pending requests know
                    var requests = pendingRegister[cleaned].Where(z => z != Sender).ToList();
                    foreach (var request in requests)
                    {
                        request.Tell(response);
                    }
                    pendingRegister.Remove(cleaned);
                    Sender.Tell(response);
                }
            });

            // get router for flight
            Receive<DirectoryLookupRequest>(r =>
            {
                var cleaned = cleanFlight(r.Flight);

                // if in tracked list, return entry
                if (tracked.ContainsKey(cleaned))
                    Sender.Tell(new DirectoryLookupResponse() { ResponsibleActor = tracked[cleaned] });
                
                // not being processed yet
                if (pendingRegister.ContainsKey(cleaned))
                {
                    // if this sender has already requested, then let them know the reservation is still pending
                    if (pendingRegister.Any(z => z.Value != Sender))                        
                        pendingRegister[cleaned].Add(Sender);

                    Sender.Tell(new DirectoryLookupResponse() { ReservationPending = true, Flight = r.Flight });
                }
                else
                {
                    // not in pending, so mark the Sender as the processor
                    Sender.Tell(new DirectoryLookupResponse() { ReservationPending = false, Flight = r.Flight });
                    pendingRegister.Add(cleaned, new List<IActorRef>() { Sender } );
                }
            });
        }

        string cleanFlight(string flight)
        {
            return flight.ToLower().Trim();
        }

        #region Messages
        /// <summary>
        /// Get actor for the flight code
        /// </summary>
        public class DirectoryLookupRequest
        {
            /// <summary>
            /// Request actor handling this flight code
            /// </summary>
            /// <param name="flight">Flight code</param>
            public DirectoryLookupRequest(string flight)
            {
                Flight = flight;
            }
            public string Flight { get; private set; }
        }

        /// <summary>
        /// Register myself as the processor
        /// </summary>
        public class DirectoryRegisterRequest
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="flight">Flight code</param>
            /// <param name="actor">Actor reference</param>
            public DirectoryRegisterRequest(string flight, IActorRef actor)
            {
                Flight = flight;
                Actor = actor;
            }
            public string Flight { get; private set; }
            public IActorRef Actor { get; private set; }
        }

        /// <summary>
        /// Response to 
        /// </summary>
        public class DirectoryLookupResponse
        {
            /// <summary>
            /// the original requested flight code
            /// </summary>
            public string Flight { get; set; }
            /// <summary>
            /// if no actor is processing yet, but a server is starting it up
            /// </summary>
            public bool ReservationPending { get; set; }
            /// <summary>
            /// if an actor is processing, then return the reference
            /// </summary>
            public IActorRef ResponsibleActor { get; set; }
        }
        #endregion
    }
}
