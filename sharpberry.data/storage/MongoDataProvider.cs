using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using sharpberry.data.types;

namespace sharpberry.data.storage
{
    public class MongoDataProvider : IDataProvider
    {
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
            this.Collection.InsertOneAsync(trip);
            return trip;
        }
        public void EndTrip(Trip trip)
        {
            trip.End = DateTime.Now;
            this.Collection.UpdateOneAsync(
                t => t.Id == trip.Id, 
                Builders<Trip>.Update.Set(f => f.End, trip.End)
                );
        }

        public void WriteObdSample(Trip trip, ObdSample sample)
        {
            trip.Obd.Add(sample);
            this.Collection.UpdateOneAsync(
                t => t.Id == trip.Id,
                Builders<Trip>.Update.Push(f => f.Obd, sample)
                );
        }

        public void WriteGpsSample(Trip trip, GpsSample sample)
        {
            trip.Gps.Add(sample);
            this.Collection.UpdateOneAsync(
                t => t.Id == trip.Id,
                Builders<Trip>.Update.Push(f => f.Gps, sample)
                );
        }
    }
}
