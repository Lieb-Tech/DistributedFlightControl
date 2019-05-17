using System;
using System.Collections.Generic;
using System.Text;

namespace DFC_concept.DataStructures
{
    public class FlightSnapshotHistory
    {
        public string id { get; set; }
        public string flight { get; set; }
        public string hex { get; set; }
        public ICAOAircraft icaoAircraft { get; set; }
        public ICAOData icaoData { get; set; }
        public List<FlightSnapshotData> snapshots { get; set; }
        public FlightSnapshotHistory()
        {
            snapshots = new List<FlightSnapshotData>();
        }
    }
}
