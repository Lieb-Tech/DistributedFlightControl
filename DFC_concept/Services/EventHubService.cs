using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DFC_concept.Services
{   
    class EventHubService
    {
        private string StorageConnectionString = "";
        private configSettings config = null;
        public EventHubService()
        {
            var json = File.ReadAllText(Environment.CurrentDirectory + "\\eventHub.json");
            config = JsonConvert.DeserializeObject<configSettings>(json);

            StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", config.storageAccountName, config.storageAccountKey);
        }

        public void StartReceivers()
        { 
            var eventProcessorHost = new EventProcessorHost(
                    config.eventHubName,
                    PartitionReceiver.DefaultConsumerGroupName,
                    config.eventHubConnectionString,
                    StorageConnectionString,
                    config.storageContainerName);

            // Registers the Event Processor Host and starts receiving messages
            eventProcessorHost.RegisterEventProcessorAsync<EventHubReceiver>().Wait();
        }

        public class configSettings
        {
            public string eventHubConnectionString { get; set; }
            public string eventHubName { get; set; }

            public string storageContainerName { get; set; }
            public string storageAccountName { get; set; }
            public string storageAccountKey { get; set; }
        }
    }
}
