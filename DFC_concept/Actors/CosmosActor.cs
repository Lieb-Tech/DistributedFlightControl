using Akka.Actor;
using DFC_concept.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFC_concept.Actors
{
    class CosmosSaveActor : ReceiveActor
    {
        CosmosDB cdb; 
        public CosmosSaveActor(CosmosDB cosmos)
        {
            cdb = cosmos;
            Receive<CosmosSaveRequest>(r =>
            {
                // cdb.UpsertDocument(r.SaveObject, r.Collection).Wait();
            });
        }

        public static Props Props(CosmosDB cosmos) =>
            Akka.Actor.Props.Create(() => new CosmosSaveActor(cosmos));
    }

    class CosmosSaveRequest
    {
        public string Collection;
        public object SaveObject;

        public CosmosSaveRequest()
        {

        }
        public CosmosSaveRequest(string collection, object saveObject)
        {
            Collection = collection;
            SaveObject = saveObject;
        }
    }
}
