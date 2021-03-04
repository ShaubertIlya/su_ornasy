using Devir.DMS.DL.Models;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.TestMongoConnection.Model
{
    public class Doc:ModelBase
    {
        public string Name { get; set; }        
        public dynamic Value { get; set; }
    }
}
