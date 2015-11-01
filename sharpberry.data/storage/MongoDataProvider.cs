using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using sharpberry.data.types;
using log4net;

namespace sharpberry.data.storage
{
    public class MongoDataProvider : IDataProvider
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MongoDataProvider));

        public MongoDataProvider(IMongoClient client, string database, string collectionName)
            : this(client.GetDatabase(database), collectionName)
        { }

        public MongoDataProvider(IMongoDatabase database, string collectionName)
            : this(database.GetCollection<Trip>(collectionName))
        { }

        public MongoDataProvider(IMongoCollection<Trip> collection)
        {
            this.Collection = collection;
        }

        public IMongoClient Client { get { return this.Collection.Database.Client; } }
        public IMongoDatabase Database { get { return this.Collection.Database; } }
        public IMongoCollection<Trip> Collection { get; private set; }

        public Trip StartTrip()
        {
            var trip = new Trip
                {
                    Id = Guid.NewGuid(),
                    Start = DateTime.Now
                };
            logger.Debug($"Starting trip {trip.Id}");
            this.Collection.InsertOneAsync(trip).Wait();
            return trip;
        }
        public void EndTrip(Trip trip)
        {
            trip.End = DateTime.Now;
            logger.Debug($"Ending trip {trip.Id}");
            this.Collection.UpdateOneAsync(
                t => t.Id == trip.Id,
                Builders<Trip>.Update.Set(f => f.End, trip.End)
                ).ContinueWith(this.WriteOperationCompleted, trip, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void WriteObdSample(Trip trip, ObdSample sample)
        {
            trip.Obd.Add(sample);
            logger.Debug($"Writing OBD sample to trip");
            this.Collection.UpdateOneAsync(
                t => t.Id == trip.Id,
                Builders<Trip>.Update.Push(f => f.Obd, sample)
                ).ContinueWith(this.WriteOperationCompleted, trip, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void WriteGpsSample(Trip trip, GpsSample sample)
        {
            trip.Gps.Add(sample);
            logger.Debug($"Writing GPS sample to trip");
            this.Collection.UpdateOneAsync(
                t => t.Id == trip.Id,
                Builders<Trip>.Update.Push(f => f.Gps, sample)
                ).ContinueWith(this.WriteOperationCompleted, trip, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void WriteOperationCompleted(Task origin, object state)
        {
            var trip = (Trip)state;
            if (origin.IsFaulted)
                logger.Error($"Write operation failed for trip {trip.Id}: {origin.Exception}");
        }
    }
}
