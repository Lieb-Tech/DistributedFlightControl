using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DFC_concept.Services
{
    public class MongoService
    {
        static MongoSettings settings = null;

        public static string ConnectionString
        {
            get
            {
                if (settings == null)
                {
                    var info = File.ReadAllText("mongo.json");
                    settings = JsonConvert.DeserializeObject<MongoSettings>(info);
                }
                return settings.connectionString;
            }
        }
        private class MongoSettings
        {
            public string connectionString { get; set; }
        }

    }
}
