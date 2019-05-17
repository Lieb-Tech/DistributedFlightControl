using System;
using System.Collections.Generic;
using System.Text;

namespace DFC_concept.DataStructures
{
    public class FlightSnapshotDataEx : FlightSnapshotData
    {
        public string id { get; set; }
        public string _self { get; set; }

        public FlightSnapshotDataEx() { }
        public FlightSnapshotDataEx(FlightSnapshotData data)
        {
            base.now = data.now;
            base.alt = data.alt;
            base.altDelta = data.altDelta;
            base.lat = data.lat;
            base.lon = data.lon;
            base.spd = data.spd;
            base.spdDelta = data.spdDelta;
            base.track = data.track;
        }
    }

    public class FlightSnapshotData
    {
        public string id { get; set; }
        public double now { get; set; }
        public long alt { get; set; }
        public double track { get; set; }
        public double spd { get; set; }

        public double lat { get; set; }
        public double lon { get; set; }

        public double spdDelta { get; set; }
        public double altDelta { get; set; }
    }

    

}
