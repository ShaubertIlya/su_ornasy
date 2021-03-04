using Devir.DMS.DL.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Devir.DMS.DL.Models.Document;
using MongoDB.Bson;

namespace Devir.DMS.DL.Repositories
{
    // simple repository
    public class RepositoryBaseNoAudit<T> where T : ModelBase
    {
        private Guid _userId;
        private MongoDatabase _db;
        private string _collectionName;
        private string _auditCollection;

        public RepositoryBaseNoAudit(MongoDatabase database, string collectionName, Guid userId)
        {
            _userId = userId;
            _db = database;
            _collectionName = collectionName;
            _auditCollection = String.Format("{0}_{1}", collectionName, "audit");
        }

        public MongoCollection<T> GetCollectionByDynamicFields()
        {
            //Returns collection for dynamic filtration
            return null;
        }

        public MongoCollection<T> GetCollection(bool isAudit = false)
        {
            if (!isAudit)
                return _db.GetCollection<T>(_collectionName);
            else
                return _db.GetCollection<T>(_auditCollection);
        }

        public IEnumerable<T> List()
        {            
            return GetCollection().AsQueryable<T>().ToList();
        }

        public int GetListCount(Expression<Func<T, bool>> exp)
        {
            return GetCollection().AsQueryable<T>().Where(exp).Count();
        }
        
        public IEnumerable<T> List(Expression<Func<T, bool>> exp)
        {            
            return GetCollection().AsQueryable<T>().Where(exp).ToList();
        }

        public IEnumerable<T> List(Expression<Func<T, bool>> exp, string orderQuery, int start, int rows)
        {
            return GetCollection().AsQueryable<T>().Where(exp).OrderBy(orderQuery).Skip(start).Take(rows).ToList();
        }

        public IEnumerable<T> List(Expression<Func<T, bool>> exp, string sidx1, string sord1, string sidx2, string sord2, int start, int rows)
        {
            SortByBuilder sortOrder = new SortByBuilder();
            if (sord1 == "desc")
            {
                if (sord2 == "desc")
                {
                    sortOrder = SortBy.Descending(sidx1).Descending(sidx2);
                }
                else
                {
                    sortOrder = SortBy.Descending(sidx1).Ascending(sidx2);
                }
            }
            else
            {
                if (sord2 == "desc")
                {
                    sortOrder = SortBy.Ascending(sidx1).Descending(sidx2);
                }
                else
                {
                    sortOrder = SortBy.Ascending(sidx1).Ascending(sidx2);
                }
            }
            return GetCollection().FindAll().SetSortOrder(sortOrder).AsQueryable<T>().Where(exp).Skip(start).Take(rows).ToList();
        }

        public T Single(Expression<Func<T, bool>> exp)
        {
            return List(exp).FirstOrDefault();            
        }

        public Guid Insert(T entity)
        {
            if(entity.Id==Guid.Empty)
            entity.Id = Guid.NewGuid();

            entity.CreateDate = DateTime.Now;
            entity.CreatorGuid = _userId;
            GetCollection().Insert<T>(entity);
            //GetCollection(true).Insert<T>(entity);
            return entity.Id;
        }

        //public void Insert(ICollection<T> entities)
        //{
        //    GetCollection().InsertBatch(entities);
        //    GetCollection(true).InsertBatch(entities);
        //}

        public void Delete(Guid Id)
        {
            GetCollection().Update(
                Query.And(
                Query<T>.EQ(m=>m.Id, Id),
                Query<T>.EQ(m=>m.isDeleted, false)),

                Update.Combine(
                Update<T>.Set(m=>m.DeleteDate, DateTime.Now),
                Update<T>.Set(m=>m.DeletedByUserGuid, _userId),
                Update<T>.Set(m=>m.isDeleted, true))
                );

            //var rec = Single(m => m.Id == Id && m.isDeleted == false);
            //rec.DeleteDate = DateTime.Now;
            //rec.DeletedByUserGuid = _userId;
            //rec.isDeleted = true;
            //GetCollection().Save(rec);
        }

        public void update(T recordToUpdate)
        {
            GetCollection().Save(recordToUpdate);

            //Task saveToAuditTask = new Task(() =>
            //{
            //    try
            //    {
            //        var rec = GetCollection(true).FindOne(Query<T>.Where(m => m.BaseId == recordToUpdate.Id && m.isDeleted == false));
            //        if (rec == null)
            //        {
            //            rec = GetCollection(true).FindOne(Query<T>.Where(m => m.Id == recordToUpdate.Id && m.isDeleted == false));
            //            recordToUpdate.BaseId = rec.Id;
            //        }

            //        rec.DeleteDate = DateTime.Now;
            //        rec.DeletedByUserGuid = _userId;
            //        recordToUpdate.Id = Guid.NewGuid();
            //        recordToUpdate.CreatorGuid = _userId;
            //        recordToUpdate.CreateDate = DateTime.Now;
            //        GetCollection(true).Save(rec);
            //        GetCollection(true).Insert(recordToUpdate);
            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //});
            //saveToAuditTask.Start();
        }
    }
}
