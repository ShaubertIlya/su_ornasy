using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Devir.DMS.DL.File
{
    public class MongoGridFsHelper
    {
        private readonly MongoDatabase _db;
        private readonly MongoGridFS _gridFs;

        public MongoGridFsHelper(MongoDatabase db)
        {
            _db = db;
            _gridFs = _db.GridFS;
        }

        public ObjectId AddFile(Stream fileStream, string fileName)
        {

            var fileInfo = _gridFs.Upload(fileStream, fileName);

            return (ObjectId)fileInfo.Id;
        }

        public Stream GetFile(ObjectId id)
        {
            var file = _gridFs.FindOneById(id);

            //if (file == null)
            //{
            //    return new Stream;
            //}

            return file.OpenRead();
        }


        public void DeleteFile(ObjectId id)
        {
            _gridFs.DeleteById(id);
        }

    }
}
