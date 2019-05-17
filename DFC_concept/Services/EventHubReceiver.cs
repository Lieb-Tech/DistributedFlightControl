using Akka.Actor;
using DFC_concept.DataStructures;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DFC_concept.Services
{
    class EventHubReceiver : IEventProcessor
    {        
        public EventHubReceiver()
        {
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine($"SimpleEventProcessor initialized. Partition: '{context.PartitionId}'");
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            Console.WriteLine($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var eventData in messages)
            {
                var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                var info = JsonConvert.DeserializeObject<FlightReading>(data);
                Program.logger.Tell(info);

                var diff = DateTimeOffset.FromUnixTimeSeconds((long)info.now).ToLocalTime();
                Console.WriteLine($"Part: '{context.PartitionId}', {diff} : {info.data.flight} ");
            }

            return context.CheckpointAsync();
        }
    }
}
