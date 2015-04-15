using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace sharpberry.data
{
    public interface IHasObjectId
    {
        ObjectId Id { get; set; }
    }
}
