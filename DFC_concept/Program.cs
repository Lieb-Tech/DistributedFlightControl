using Akka.Actor;
using System;

namespace DFC_concept
{
    class Program
    {
        internal static IActorRef logger = null;
        static void Main(string[] args)
        {
            using (var sys = ActorSystem.Create("DFC"))
            {
                logger = sys.ActorOf<Actors.LoggerActor>();
                var eh = new Services.EventHubService();
                // eh.StartReceivers();

                Console.WriteLine("Hello World!");
                Console.ReadLine();
            }
        }
    }
}
