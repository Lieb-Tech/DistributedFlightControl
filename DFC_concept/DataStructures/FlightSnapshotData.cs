using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFC_concept.DataStructures
{
    
    public class FlightSnapshotData
    {
        [BsonId]
        //public MongoDB.Bson.ObjectId id { get; set; }
        public string id { get; set; }

        [BsonElement]
        public double now { get; set; }
        [BsonElement]
        public long alt { get; set; }
        [BsonElement]
        public double track { get; set; }
        [BsonElement]
        public double spd { get; set; }
        [BsonElement]
        public double lat { get; set; }
        [BsonElement]
        public double lon { get; set; }
        [BsonElement]
        public double spdDelta { get; set; }
        [BsonElement]
        public double altDelta { get; set; }
    }

    

}
