using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;

namespace Devir.DMS.DL.MongoHelpers
{
    public static class MongoHelper
    {
        private static MongoDatabase _db;
        private static bool DateTimeSerializerRegistered = false;
        public static MongoClient client { get;set;}
        public static MongoDatabase Database { get { return _db; } private set { value = null; } }
        public static void connect(string connectionString, string dataBaseName)
        {
            //DateTimeSerializationOptions.Defaults = DateTimeSerializationOptions.LocalInstance;
            if (!DateTimeSerializerRegistered)
            {
                MongoDB.Bson.Serialization.Options.DateTimeSerializationOptions options = MongoDB.Bson.Serialization.Options.DateTimeSerializationOptions.LocalInstance;
                var serializer = new MongoDB.Bson.Serialization.Serializers.DateTimeSerializer(options);
                try
                {
                    MongoDB.Bson.Serialization.BsonSerializer.RegisterSerializer(typeof (DateTime), serializer);
                }
                catch
                {
                }

                DateTimeSerializerRegistered = true;
            }


            var settings = new MongoClientSettings
            {
                Server = new MongoServerAddress(connectionString),
                MaxConnectionPoolSize = 1500,
                ConnectionMode = ConnectionMode.Automatic,
                WaitQueueSize = 1500,
                WaitQueueTimeout = new TimeSpan(0, 1, 0)
            };

            client = new MongoClient(settings);
            var server = client.GetServer();
            
            _db = server.GetDatabase(dataBaseName);   
            
        }
        public static bool IsConnected { get { return _db == null ? false : _db.Server.State == MongoServerState.Connected; } }
    }
}
