using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace sharpberry.data
{
    public class DataProvider<T>
        where T : IHasObjectId
    {
        public DataProvider(IMongoClient client, string database, string collectionName)
            : this(client.GetDatabase(database), collectionName)
        { }

        public DataProvider(IMongoDatabase database, string collectionName)
            : this(database.GetCollection<T>(collectionName))
        { }

        public DataProvider(IMongoCollection<T> collection)
        {
            this.Collection = collection;
        }

        public IMongoClient Client { get { return this.Collection.Database.Client; } }
        public IMongoDatabase Database { get { return this.Collection.Database; } }
        public IMongoCollection<T> Collection { get; private set; }  

        public void Put(T data)
        {
            this.Collection.InsertOneAsync(data);
        }

        public Task<List<T>> Get()
        {
            return Get(o => true);
        }

        public async Task<List<T>> Get(Expression<Func<T, bool>> query)
        {
            var result = await this.Collection.FindAsync(query);
            return await result.ToListAsync();
        }
    }
}
