using Akka.Actor;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DFC_concept.Actors
{
    class MongoActor : ReceiveActor
    {        
        public MongoActor(IMongoDatabase mongo)
        {

            Receive<MongoSaveRequest<DataStructures.FlightSnapshotData>>(r =>
            {
                var col = mongo.GetCollection<DataStructures.FlightSnapshotData>("snaps");
                var obj = (r.SaveObject).ToBsonDocument();
                var res = col.ReplaceOne(z => z.id == r.SaveObject.id, r.SaveObject);
                if (res.MatchedCount == 0)
                    col.InsertOne(r.SaveObject);
            });

            Receive<MongoSaveRequest<DataStructures.FlightDataExtended>>(r =>
            {
                var col = mongo.GetCollection<DataStructures.FlightDataExtended>("flights");                
                var obj = (r.SaveObject).ToBsonDocument();
                var res = col.ReplaceOne(z => z.id == r.SaveObject.id, r.SaveObject);
                if (res.MatchedCount == 0)
                    col.InsertOne(r.SaveObject);
            });
        }

        public static Props Props(IMongoDatabase mdb) =>
            Akka.Actor.Props.Create(() => new MongoActor(mdb));

        public class MongoSaveRequest<T>
        {
            public T SaveObject;
            public MongoSaveRequest(T ObjectToSave)
            {
                SaveObject = ObjectToSave;
            }
        }    
    }
}
