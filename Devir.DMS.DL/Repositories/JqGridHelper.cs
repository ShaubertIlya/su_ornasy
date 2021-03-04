using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;
using Devir.DMS.DL.Models;
using MongoDB.Driver;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;

namespace Devir.DMS.DL.Repositories
{

    public class JqGridData
    {
        public int page { get; set; }
        public int total { get; set; }
        public long records { get; set; }
        public IQueryable rows { get; set; }
        public object userdata { get; set; }
    }

    public class JqGridHelper<T> where T : ModelBase
    {

        //public JsonResult GetDataForDataGrid(Expression<Func<T, bool>> exp, int page, int rows, string sort, string order)
        //{

        //    var start = rows * page - rows;
        //    if (start < 0) start = 0;

        //    var ret = new
        //    {

        //        total = RepositoryFactory.GetRepository<T>().GetListCount(exp),

        //        rows = RepositoryFactory.GetRepository<T>().List(exp, string.Format("{0} {1}", sort, order == "desc" ? order : ""), start, rows).AsQueryable()
        //    };


        //    JsonResult j = new JsonResult();
        //    j.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        //    j.Data = ret;

        //    return j;

        //}



        public JsonResult GetGridResult(Expression<Func<T, bool>> exp, int page, int rows, string sidx, string sord)
        {

            var allRows = RepositoryFactory.GetRepository<T>().GetListCount(exp);

            int totalPages = 0;

            if (allRows > 0)
                totalPages = (int)Math.Ceiling((double)allRows / rows);

            if (page > totalPages) page = totalPages;

            if (rows < 0) rows = 0;

            var start = rows * page - rows;
            if (start < 0) start = 0;

            //var result = orderedResult.Skip(start).Take(rows).ToList();

            var ret = new JqGridData
            {
                page = page,
                total = totalPages,
                records = allRows,
                rows = RepositoryFactory.GetRepository<T>().List(exp, string.Format("{0} {1}", sidx, sord == "desc" ? sord : ""), start, rows).AsQueryable()
            };

            JsonResult j = new JsonResult();
            j.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            j.Data = ret;

            return j;
        }



        public JsonResult GetGridResultForDynamicRef(Expression<Func<T, bool>> exp, int page, int rows, string sidx, string sord)
        {
            var allRows = RepositoryFactory.GetRepository<T>().GetListCount(exp);

            int totalPages = 0;

            if (allRows > 0)
                totalPages = (int)Math.Ceiling((double)allRows / rows);

            if (page > totalPages) page = totalPages;

            if (rows < 0) rows = 0;

            var start = rows * page - rows;
            if (start < 0) start = 0;

            //var result = orderedResult.Skip(start).Take(rows).ToList();

            var ret = new JqGridData
            {
                page = page,
                total = totalPages,
                records = allRows,
                rows = RepositoryFactory.GetRepository<T>().List(exp, string.Format("{0} {1}", sidx, sord == "desc" ? sord : ""), start, rows).AsQueryable().
                GroupBy(m => m.Id).Select(m => new { Id = m.Key, value = m.ToList() })
            };

            JsonResult j = new JsonResult();
            j.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            j.Data = ret;

            return j;
        }

        

    }

    public class JqGridHelper
    {
        public JsonResult GetGridResult(IEnumerable collection, int page, int rows, string sidx, string sord)
        {
            var allRows = collection.AsQueryable().Count();

            int totalPages = 0;

            if (allRows > 0)
                totalPages = (int)Math.Ceiling((double)allRows / rows);

            if (page > totalPages) page = totalPages;

            if (rows < 0) rows = 0;

            var start = rows * page - rows;
            if (start < 0) start = 0;

            //var result = orderedResult.Skip(start).Take(rows).ToList();

            var ret = new JqGridResult
            {
                page = page,
                total = totalPages,
                records = allRows,
                rows = collection.AsQueryable().OrderBy(string.Format("{0} {1}", sidx, sord == "desc" ? sord : "")).Skip(start).Take(rows).Cast<object>().AsQueryable<object>()
            };

            JsonResult j = new JsonResult();
            j.Data = ret;

            return j;
        }

        public IQueryable GridSort(IEnumerable collection, string sidx, string sord)
        {
            return collection.AsQueryable().OrderBy(string.Format("{0} {1}", sidx, sord == "desc" ? sord : ""));
        }

        public JsonResult GetGridResult(IEnumerable collection, int page, int rows)
        {
            var allRows = collection.AsQueryable().Count();

            int totalPages = 0;

            if (allRows > 0)
                totalPages = (int)Math.Ceiling((double)allRows / rows);

            if (page > totalPages) page = totalPages;

            if (rows < 0) rows = 0;

            var start = rows * page - rows;
            if (start < 0) start = 0;

            //var result = orderedResult.Skip(start).Take(rows).ToList();

            if (totalPages == 0) 
                totalPages = 1;
            var ret = new JqGridResult
            {
                page = page,
                total = totalPages,
                records = allRows,
                rows = collection.AsQueryable().Skip(start).Take(rows).Cast<object>().AsQueryable<object>()
            };

            JsonResult j = new JsonResult();
            j.Data = ret;

            return j;
        }

        public JsonResult GetDocumentsGridResult(bool? owner, string period, string sidx, string sord, bool usingSearch, List<Guid> foundResults)
        {

            //var allRows = RepositoryFactory.GetDocumentRepository().getSortedDocumentsCount(owner);

            //int totalPages = 0;

            //if (allRows > 0)
            //    totalPages = (int)Math.Ceiling((double)allRows / rows);

            //if (page > totalPages) page = totalPages;

            //if (rows < 0) rows = 0;

            //var start = rows * page - rows;
            //if (start < 0) start = 0;

            //var result = orderedResult.Skip(start).Take(rows).ToList();
            string sidx2 = "";
            string sord1 = "asc";
            if (sidx.Contains("d,"))
                sord1 = "desc";//sidx = sidx.Replace("d,", "DESC,");
            if (sidx.Contains(','))
            {
                string[] str = sidx.Split(',');
                sidx = str[0].Split(' ')[0].Trim();
                sidx2 = str[1].Trim();
            }

            var ret = new JqGridData
            {
                //page = page,
                //total = totalPages,
                //records = allRows,
                rows = RepositoryFactory.GetDocumentRepository().getSortedDocuments(owner, period, sord, sidx2, usingSearch, foundResults).AsQueryable(),
                userdata = RepositoryFactory.GetDocumentRepository().GetDocumentsCounts()

            };

            JsonResult j = new JsonResult();
            j.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            j.Data = ret;

            return j;
        }

        public JsonResult GetDocumentsByTypeGridResult(Guid docType, bool? owner, string period, string sidx, string sord, bool usingSearch, List<Guid> foundResults, Guid idToDynamicFieldFilter)
        {


            var allRows = RepositoryFactory.GetDocumentRepository().getSortedDocumentsCountByType(docType, owner, period);

            //int totalPages = 0;

            //if (allRows > 0)
            //    totalPages = (int)Math.Ceiling((double)allRows / rows);

            //if (page > totalPages) page = totalPages;

            //if (rows < 0) rows = 0;

            //var start = rows * page - rows;
            //if (start < 0) start = 0;

            //var result = orderedResult.Skip(start).Take(rows).ToList();
            string sidx2 = "";
            string sord1 = "asc";
            if (sidx.Contains("d,"))
                sord1 = "desc";//sidx = sidx.Replace("d,", "DESC,");
            if (sidx.Contains(','))
            {
                string[] str = sidx.Split(',');
                sidx = str[0].Split(' ')[0].Trim();
                sidx2 = str[1].Trim();
            }

            var ret = new JqGridData
            {
                //page = page,
                //total = totalPages,
                //records = allRows,
                rows = RepositoryFactory.GetDocumentRepository().getSortedDocumentsByType(docType, owner, period, sord, sidx2, usingSearch, foundResults, idToDynamicFieldFilter).AsQueryable(),
                userdata = RepositoryFactory.GetDocumentRepository().GetDocumentsCounts()

            };

            JsonResult j = new JsonResult();
            j.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            j.Data = ret;

            return j;
        }

        public JsonResult GetInstructionsGridResult(string type, int page, int rows, string sidx, string sord)
        {
            var allRows = RepositoryFactory.GetInstructionRepository().GetInstructionsCount(type);

            int totalPages = 0;

            if (allRows > 0)
                totalPages = (int)Math.Ceiling((double)allRows / rows);

            if (page > totalPages) page = totalPages;

            if (rows < 0) rows = 0;

            var start = rows * page - rows;
            if (start < 0) start = 0;



            var ret = new JqGridData
            {
                page = page,
                total = totalPages,
                records = allRows,
                rows = RepositoryFactory.GetInstructionRepository().GetInstructions(type, sord, sidx, start, rows).AsQueryable(),
                userdata = RepositoryFactory.GetInstructionRepository().GetInstructionsCounts()
            };

            JsonResult j = new JsonResult();
            j.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            j.Data = ret;

            return j;
        }

        
    }

    public class JqGridResult
    {
        public int page { get; set; }
        public int total { get; set; }
        public int records { get; set; }
        public IQueryable<object> rows { get; set; }

        public JqGridResult()
        {
        }
    }

}
