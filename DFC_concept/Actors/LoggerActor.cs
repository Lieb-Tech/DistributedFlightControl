using Akka.Actor;
using DFC_concept.DataStructures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DFC_concept.Actors
{
    class LoggerActor : ReceiveActor
    {
        public LoggerActor()
        {
            Receive<FlightReading>(r =>
            {
                File.AppendAllText(r.data.flight + ".txt", JsonConvert.SerializeObject(r) + "\r\n");
            });
        }
    }
}
