
using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.Document.Route;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Linq.Expressions;
using System.Dynamic;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using Devir.DMS.DL.Extensions;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using System.Text.RegularExpressions;
using MongoDB.Bson.Serialization;
using Devir.DMS.DL.Models.Filter;
using System.Globalization;

namespace Devir.DMS.DL.Repositories
{
    public class DocumentRepository : RepositoryBase<Document>
    {
        public DocumentRepository(MongoDatabase database, string collectionName, Guid userId)
            : base(database, collectionName, userId)
        {
        }



        //Для нового грида

        public IEnumerable<dynamic> GetListTasks(int startRecord, int recordsOnPage, Guid AuthorId, string sortColumn, int sortDirection,
                                                                string groupColumn, string Period, List<Guid> foundResults)
        {

            var tasks = RepositoryFactory.GetDocumentRepository().getAllTasks();

            var data = tasks.Select(d => new DocumentTaskViewModel
            {
                Id = d.Id,
                CalculatedId = d.Id.ToString(),
                Header = d.Header,
                Name = d.DocumentSignStages.Single(rs => rs.isCurrent && rs.ControlPerformForRouteStageUserId == null && rs.RouteUsers.Any(rt => rt.IsCurent)).Name +
                    ((d.DocumentSignStages.Single(rs => rs.isCurrent && rs.ControlPerformForRouteStageUserId == null && rs.RouteUsers.Any(rt => rt.IsCurent)).RouteUsers.FirstOrDefault(ru => ru.IsCurent).Instructions != null)
                    && (d.DocumentSignStages.Single(rs => rs.isCurrent && rs.ControlPerformForRouteStageUserId == null && rs.RouteUsers.Any(rt => rt.IsCurent)).RouteUsers.FirstOrDefault(ru => ru.IsCurent).Instructions.Count() > 0) ? " (поручено)" : ""),
                Number = string.Format("{0} №{1}", d.DocumentType.Name, d.DocumentNumber),
                AuthorName = d.Author.GetFIO(),
                //Group = GetTaskGroup(d),
                //Classes = GetTasksClasses(d),
                hrefAddress = "GetDocument?DocumentId=" + d.Id.ToString(),
                Date = d.FinishDate,
                gDate = d.FinishDate.ToString("dd.MM.yyyy"),
                sDate = d.FinishDate.ToString("dd.MM.yyyy"),
                isNew = d.DocumentViewers.Any(dv => dv.Key == RepositoryFactory.GetCurrentUser().ToString()) && d.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].ViewDateTime == null,
                isExpired = DateTime.Now > d.FinishDate,
                ExpiredTimeSpan = DateTime.Now - d.FinishDate,
                isUrgent = d.isUrgent,
            }).ToList();



            var instrTasks = RepositoryFactory.GetInstructionRepository().getAllTasks();

            data.AddRange(instrTasks.Select(d => new DocumentTaskViewModel
            {
                Id = d.Id,
                CalculatedId = d.Id.ToString(),
                Header = d.Header,
                Name = d.DocumentSignStages.Single(rs => rs.isCurrent && rs.ControlPerformForRouteStageUserId == null && rs.RouteUsers.Any(rt => rt.IsCurent)).Name +
                ((d.DocumentSignStages.Single(rs => rs.isCurrent && rs.ControlPerformForRouteStageUserId == null && rs.RouteUsers.Any(rt => rt.IsCurent)).RouteUsers.Single(ru => ru.IsCurent).Instructions != null) && (d.DocumentSignStages.Single(rs => rs.isCurrent && rs.ControlPerformForRouteStageUserId == null && rs.RouteUsers.Any(rt => rt.IsCurent)).RouteUsers.Single(ru => ru.IsCurent).Instructions.Count() > 0) ? " (поручено)" : ""),
                FinishDate = d.FinishDate.ToString("dd.MM.yyyy"),
                Number = string.Format("{0} {1}", d.DocumentType.Name, d.DocumentNumber),
                AuthorName = d.Author.GetFIO(),
                //Group = GetTaskGroup(d),
                //Classes = GetTasksClasses(d),

                hrefAddress = "GetDocument?DocumentId=" + d.RootDocumentId.ToString() + "&Tab=3",
                Date = d.FinishDate,
                gDate = d.FinishDate.ToString("dd.MM.yyyy"),
                sDate = d.FinishDate.ToString("dd.MM.yyyy"),
                isNew = d.DocumentViewers.Any(dv => dv.Key == RepositoryFactory.GetCurrentUser().ToString()) && d.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].ViewDateTime == null,

                isExpired = DateTime.Now > d.FinishDate,
                ExpiredTimeSpan = DateTime.Now - d.FinishDate,
                isUrgent = d.isUrgent,
            }).ToList());


            tasks = RepositoryFactory.GetDocumentRepository().getAllTasksForConfirmingPerform();

            tasks.ForEach(m =>
            {
                if (m.DocumentSignStages.Exists(n => n.isCurrent == true && n.ControlPerformForRouteStageUserId != null && n.RouteUsers.Exists(x => x.SignUser.UserId == RepositoryFactory.GetCurrentUser())))
                {
                    m.DocumentSignStages.Where(n => n.isCurrent == true && n.ControlPerformForRouteStageUserId != null && n.RouteUsers.Exists(x => x.SignUser.UserId == RepositoryFactory.GetCurrentUser())).ToList().ForEach(k =>
                    {
                        if (k.RouteUsers.Exists(kx => kx.SignUser.UserId == RepositoryFactory.GetCurrentUser()))
                        {
                            k.RouteUsers.Where(kx => kx.SignUser.UserId == RepositoryFactory.GetCurrentUser()).ToList().ForEach(ku =>
                            {
                                data.Add(new DocumentTaskViewModel
                                {
                                    Id = ku.Id,
                                    CalculatedId = m.Id.ToString() + ":" + ku.Id.ToString(),
                                    Header = m.Header,
                                    Name = k.Name,
                                    FinishDate = m.FinishDate.ToString("dd.MM.yyyy"),
                                    Number = string.Format("{0} №{1}", m.DocumentType.Name, m.DocumentNumber),
                                    AuthorName = m.DocumentSignStages.Single(m1 => m1.RouteUsers.Any(n1 => n1.Id == k.ControlPerformForRouteStageUserId)).RouteUsers.Single(n1 => n1.Id == k.ControlPerformForRouteStageUserId).SignUser.GetFIO(),
                                    //Group = GetTaskGroup(m),
                                    //Classes = GetTasksClasses(m),
                                    hrefAddress = "GetDocument?DocumentId=" + m.Id.ToString() + "&Tab=3", // "ViewSignResult?DocId=" + m.Id.ToString() + "&routeuserId=" + k.ControlPerformForRouteStageUserId.ToString(),
                                    Date = m.FinishDate,
                                    gDate = m.FinishDate.ToString("dd.MM.yyyy"),
                                    sDate = m.FinishDate.ToString("dd.MM.yyyy"),
                                    isNew = m.DocumentViewers.Any(dv => dv.Key == RepositoryFactory.GetCurrentUser().ToString()) && m.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].ViewDateTime == null,

                                    isExpired = DateTime.Now > m.FinishDate,
                                    ExpiredTimeSpan = DateTime.Now - m.FinishDate,
                                    isUrgent = m.isUrgent,
                                });
                            });
                        }
                    });
                }
            });


            instrTasks = RepositoryFactory.GetInstructionRepository().getAllTasksForConfirmingPerform();


            instrTasks.ForEach(m =>
            {
                if (m.DocumentSignStages.Exists(n => n.isCurrent == true && n.ControlPerformForRouteStageUserId != null && n.RouteUsers.Exists(x => x.SignUser.UserId == RepositoryFactory.GetCurrentUser())))
                {
                    m.DocumentSignStages.Where(n => n.isCurrent == true && n.ControlPerformForRouteStageUserId != null && n.RouteUsers.Exists(x => x.SignUser.UserId == RepositoryFactory.GetCurrentUser())).ToList().ForEach(k =>
                    {
                        if (k.RouteUsers.Exists(kx => kx.SignUser.UserId == RepositoryFactory.GetCurrentUser()))
                        {
                            k.RouteUsers.Where(kx => kx.SignUser.UserId == RepositoryFactory.GetCurrentUser()).ToList().ForEach(ku =>
                            {
                                data.Add(new DocumentTaskViewModel
                                {
                                    Id = ku.Id,
                                    CalculatedId = m.Id.ToString() + ":" + ku.Id.ToString(),
                                    Header = m.Header,
                                    Name = k.Name,
                                    FinishDate = m.FinishDate.ToString("dd.MM.yyyy"),
                                    Number = string.Format("{0} {1}", m.DocumentType.Name, m.DocumentNumber),
                                    AuthorName = m.UserFor.GetFIO(),
                                    //Group = GetTaskGroup(m),
                                    //Classes = GetTasksClasses(m),
                                    hrefAddress = "GetDocument?DocumentId=" + m.RootDocumentId.ToString() + "&Tab=3",
                                    // hrefAddress = "ViewSignResult?DocId=" + m.Id.ToString() + "&routeuserId=" + k.ControlPerformForRouteStageUserId.ToString() + "&isForInstruction=true"
                                    Date = m.FinishDate,
                                    gDate = m.FinishDate.ToString("dd.MM.yyyy"),
                                    sDate = m.FinishDate.ToString("dd.MM.yyyy"),
                                    isNew = m.DocumentViewers.Any(dv => dv.Key == RepositoryFactory.GetCurrentUser().ToString()) && m.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].ViewDateTime == null,

                                    isExpired = DateTime.Now > m.FinishDate,
                                    ExpiredTimeSpan = DateTime.Now - m.FinishDate,
                                    isUrgent = m.isUrgent,
                                });
                            });
                        }
                    });
                }
            });


            var resultList = data.AsQueryable().OrderBy(sortColumn + (sortDirection == 0 ? " DESC" : "")).GroupByMany(groupColumn).Select(m => new
            {
                Group = m.Key,
                Visible = true,
                Values = m.Items,
            }).ToList();

            return resultList;
        }


        //Получение количства записей подходящих под условие выборки для грида. Так сказать Total.
        public long GetListForAllDocumentsGridCount(Guid AuthorId, string Owner, string Period, List<Guid> foundResults, Guid docType = default(Guid), Guid idToDynamicFieldFilter = default(Guid),
                                          DocumentFilterVM documentFilterVM = default(DocumentFilterVM)
                                            )
        {
            return getCursorForGrid(AuthorId, Owner, Period, foundResults, "", 1, docType, idToDynamicFieldFilter, documentFilterVM).Count();
        }

        //Получением собственно сами данные
        public IEnumerable<DocumentsViewM> GetListForAllDocumentsGrid(int startRecord, int recordsOnPage, Guid AuthorId, string sortColumn, int sortDirection,
                                                                string groupColumn, string Owner, string Period, List<Guid> foundResults,
                                                                Guid docType = default(Guid), Guid idToDynamicFieldFilter = default(Guid),
                                                                DocumentFilterVM documentFilterVM = default(DocumentFilterVM), bool isExcel = false, string searchPhrase = "")
        {

            //GetCollection().EnsureIndex(new IndexKeysBuilder().Ascending("DocumentViewers." + AuthorId.ToString() + ".Date"));
            GetCollection().EnsureIndex(new IndexKeysBuilder().Ascending("Author.UserId"));
            GetCollection().EnsureIndex(new IndexKeysBuilder().Ascending("docState"));
            GetCollection().EnsureIndex(new IndexKeysBuilder<Document>().Ascending(m => m.DocumentType.Id));

            //getCursorForGrid(AuthorId, Owner, Period, foundResults, sortColumn, sortDirection, docType, idToDynamicFieldFilter).SetSkip(startRecord).SetLimit(recordsOnPage).Explain();
            //

            var query = !isExcel
                ? getCursorForGrid(AuthorId, Owner, Period, foundResults, sortColumn, sortDirection, docType,
                    idToDynamicFieldFilter, documentFilterVM).SetSkip(startRecord).SetLimit(recordsOnPage)
                : getCursorForGrid(AuthorId, Owner, Period, foundResults, sortColumn, sortDirection, docType,
                    idToDynamicFieldFilter, documentFilterVM);

            query.SetFields(Fields.Include("Id", "Author.UserId", "Author.Name", "Author.FirstName", "Author.LastName", "docState", "FinishDate", "CreateDate",
                "Author.Fathername", "Header", "DocumentNumber", "DynamicFiltrationFieldValue", "DocumentViewers", "DocumentType.Name", "DocumentType.Id", "isUrgent", "DocumentSignStages"));

            var result = query.ToList();// .ToList();

            //var documentType = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == docType);

            if (RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == AuthorId).InRole("Канцелярия"))
            {
                result.ForEach(m =>
                {
                    if (!m.DocumentViewers.ContainsKey(RepositoryFactory.GetCurrentUser().ToString()))
                    {
                        m.DocumentViewers.Add(RepositoryFactory.GetCurrentUser().ToString(), m.DocumentViewers[m.Author.UserId.ToString()]);
                    }

                    if (m.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Date < DateTime.Now.AddYears(-100))
                    {
                        m.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Date = m.CreateDate.Date;
                        m.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Time = m.CreateDate;
                    }

                });
            }

            var preparedList = result.Where(m => m.Header.Contains(searchPhrase)).Select(m => new DocumentViewModelItem
            {
                Id = m.Id,
                Number = m.DocumentNumber,
                Header = (!String.IsNullOrEmpty(m.DynamicFiltrationFieldValue) ? m.DynamicFiltrationFieldValue + ", " : "") + m.Header,
                AuthorName = m.Author.GetFIO(),
                Date = m.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Date,
                innerSortDate = m.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Date.Date + m.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Time.TimeOfDay,
                gDate = getDateGrouping(m.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Date),
                sDate = m.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Date.ToString("dd.MM.yyyy") + " " + m.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Time.ToString("HH:mm:ss"),
                TypeName = m.DocumentType.Name,
                CurrentStage = m.CurrentStageCalcualted,
                gNumber = "",
                docStateColor = getDocStateColor(m),
                isNew = m.DocumentViewers.Any(dv => dv.Key == RepositoryFactory.GetCurrentUser().ToString()) && m.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].ViewDateTime == null,
                isUrgent = m.isUrgent,
                AddColumnId = m.DynamicFiltrationFieldGuid,
                AddColumn = m.DynamicFiltrationFieldValue,
            }).GroupByMany(groupColumn).Select(m => new DocumentsViewM
            {
                Group = m.Key, //.ToString("dd.MM.yyyy")
                Visible = true,
                Values = m.Items,
                DataCount = m.Count
            }).ToList();



            return preparedList;
        }

        private int getDocStateColor(Document doc)
        {
            if (doc.docState == DocumentState.FinishedOk)
            {
                return 1;
            }
            else if (doc.docState == DocumentState.FinishedWithError)
            {
                return 2;
            }
            else
            {
                if (DateTime.Now.Date > doc.FinishDate.Date)
                {
                    return 4;
                }
                else
                {
                    if (DateTime.Now.Date.AddDays(3) > doc.FinishDate.Date)
                    {
                        return 3;
                    }
                    else return 0;
                }
            }
        }

        public string getDateGrouping(DateTime date)
        {

            if (date.Date == DateTime.Now.Date)
                return "Сегодня";

            if (date.Date == DateTime.Now.AddDays(-1).Date)
                return "Вчера";

            if (date.Date == DateTime.Now.AddDays(-2).Date)
                return "Позавчера";



            return date.Date.ToString("dd.MM.yyyy");
        }

        //Получаем курсор для данных из монго. Метод принимает условия необходимые для грида. В т.ч. сортировки и фильтрации.
        //Не передаем сюда страницы. Постраничка делается не тут!
        private MongoCursor<Document> getCursorForGrid(Guid AuthorId, string Owner, string Period, List<Guid> foundResults, string sortColumn = "", int sortDirection = 1,
                                                        Guid docType = default(Guid), Guid idToDynamicFieldFilter = default(Guid),
                                                        DocumentFilterVM documentFilterVM = default(DocumentFilterVM))
        {

            if (documentFilterVM == null)
            {
                documentFilterVM = new DocumentFilterVM();
            }

            var currentUser = RepositoryFactory.GetCurrentUser().ToString();

            IMongoQuery queryAlien = null;
            IMongoQuery queryOwner = null;

            if (Owner.ToLower() == "all")
            {
                queryAlien = Query.And(
                    //Query.NE("Author.UserId", AuthorId),
                    Query.ElemMatch("NewDocumentViewers", Query.EQ("UserId", AuthorId)),
                    Query.In("docState", new BsonArray(new Object[] { DocumentState.InWork, DocumentState.FinishedOk }))
                    );
            }
            else
            {
                queryAlien = Query.And(
                    Query.NE("Author.UserId", AuthorId),
                   Query.ElemMatch("NewDocumentViewers", Query.EQ("UserId", AuthorId)),
                   Query.In("docState", new BsonArray(new Object[] { DocumentState.InWork, DocumentState.FinishedOk }))
                   );
            }

            queryOwner = Query.EQ("Author.UserId", AuthorId);

            IMongoQuery periodQuery = null;
            //switch (Period)
            //{
            //    case "oneMonth":
            //        periodQuery = Query.GTE("DocumentViewers." + AuthorId.ToString() + ".Date", new BsonDateTime(DateTime.Today.AddMonths(-1)));
            //        break;
            //    case "threeMonth":
            //        periodQuery = Query.GTE("DocumentViewers." + AuthorId.ToString() + ".Date", new BsonDateTime(DateTime.Today.AddMonths(-3)));
            //        break;
            //    case "sixMonth":
            //        periodQuery = Query.GTE("DocumentViewers." + AuthorId.ToString() + ".Date", new BsonDateTime(DateTime.Today.AddMonths(-6)));
            //        break;
            //    case "year":
            //        periodQuery = Query.GTE("DocumentViewers." + AuthorId.ToString() + ".Date", new BsonDateTime(DateTime.Today.AddYears(-1)));
            //        break;
            //    case "allPeriod":
            //        periodQuery = Query.LTE("NewDocumentViewers.0.Date", new BsonDateTime(DateTime.Today.AddYears(10)));
            //        break;
            //    default:
            //        periodQuery = Query.GTE("DocumentViewers." + AuthorId.ToString() + ".Date", new BsonDateTime(DateTime.Today.AddMonths(-1)));
            //        break;
            //}


            if (RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == AuthorId).InRole("Канцелярия")
)
            {
#warning Убрать отсюда эту индусятину!!!!!
                queryAlien = Query.Or(
                    queryAlien,

 Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e994583d-2ef8-4368-9ed5-5f4439374197")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("852ff790-6ce8-4ca9-b868-f5bedd6c90e4")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc"))
                    );


                periodQuery = Query.Or(
 Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                   Query<Document>.EQ(m => m.DocumentType.Id, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                   Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e994583d-2ef8-4368-9ed5-5f4439374197")),
                   Query<Document>.EQ(m => m.DocumentType.Id, new Guid("852ff790-6ce8-4ca9-b868-f5bedd6c90e4")),
                   Query<Document>.EQ(m => m.DocumentType.Id, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc"))
                   );
#warning Убрать отсюда эту индусятину!!!!!
            }

            IMongoQuery ownerQuery = null;

            if (Owner.ToLower() == "all")
            {
                ownerQuery = Query.Or(queryOwner, queryAlien);
            }

            else if (Owner.ToLower() == "my")
                ownerQuery = queryOwner;
            else
                ownerQuery = queryAlien;
            //}

            IMongoQuery resultQuery = null;
            if (foundResults != null)
            {
                resultQuery = Query.And(Query<Document>.In(m => m.Id, foundResults), ownerQuery);
            }
            else
                resultQuery = ownerQuery;


            //Устанавливаем сортировку, по умолчанию по Дате.
            SortByBuilder sortOrder = null;
            if (String.IsNullOrEmpty(sortColumn))
            {
                if (!RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == AuthorId).InRole("Канцелярия"))
                    sortOrder = sortDirection == 0 ? SortBy.Descending("NewDocumentViewers.Date") : SortBy.Ascending("NewDocumentViewers.Date");
                //sortOrder = sortDirection == 0 ? SortBy.Descending("DocumentViewers." + currentUser + ".Date") : SortBy.Ascending("DocumentViewers." + currentUser + ".Date");
                else sortOrder = sortDirection == 0 ? SortBy.Descending("CreateDate") : SortBy.Ascending("CreateDate");
            }
            else
                sortOrder = sortDirection == 0 ? SortBy.Descending(sortColumn) : SortBy.Ascending(sortColumn);

            //var secondSortOrder = SortBy.Descending("DocumentViewers." + AuthorId.ToString() + ".Time");

            sortOrder.Descending("NewDocumentViewers.Time");

            #region Filtering

            //фильтруем 
            List<IMongoQuery> filterQueries = new List<IMongoQuery>()
            {
                resultQuery
            };

            //по департаменту
            if (documentFilterVM.DepartmentId != null && documentFilterVM.IsSearchByDepartment == true) filterQueries.Add(Query<Document>.EQ(d => d.Author.DepartmentId, documentFilterVM.DepartmentId));

            //дата с
            if (!(String.IsNullOrEmpty(documentFilterVM.StartDate)) && documentFilterVM.IsSearchByStartDate == true)
            {
                var startDate = Convert.ToDateTime(documentFilterVM.StartDate);
                //var startDate = DateTime.ParseExact(documentFilterVM.StartDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                if (RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == AuthorId).InRole("Канцелярия"))
                    filterQueries.Add(Query.GTE("CreateDate", startDate));
                else
                    filterQueries.Add(Query.GTE("NewDocumentViewers.Date", startDate));
            }

            //дата по
            if (!(String.IsNullOrEmpty(documentFilterVM.EndDate)) && documentFilterVM.IsSearchByEndDate == true)
            {
                var endDate = Convert.ToDateTime(documentFilterVM.EndDate);
                //var endDate = DateTime.ParseExact(documentFilterVM.EndDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                if (RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == AuthorId).InRole("Канцелярия"))
                    filterQueries.Add(Query.LTE("CreateDate", endDate));
                else
                    filterQueries.Add(Query.LTE("NewDocumentViewers.0.Date", endDate));
            }


            //по заголовку на вхождение
            if (!String.IsNullOrEmpty(documentFilterVM.Header) && documentFilterVM.IsSearchByHeader == true && documentFilterVM.MethodOfSearchForHeader == "IsInclusionByHeader") // && documentFilterVM.IsSearchInHeader
                //filterQueries.Add(Query.Or(Query.Matches("Header", documentFilterVM.SearchHeader), Query.Matches("DynamicFiltrationFieldValue", documentFilterVM.SearchHeader)));
                filterQueries.Add(Query.Or(Query.Matches("Header", new BsonRegularExpression(new Regex(documentFilterVM.Header, RegexOptions.IgnoreCase))),
                                          Query.Matches("DynamicFiltrationFieldValue", new BsonRegularExpression(new Regex(documentFilterVM.Header, RegexOptions.IgnoreCase)))
                    ));
            //по заголовку точно
            if (!String.IsNullOrEmpty(documentFilterVM.Header) && documentFilterVM.IsSearchByHeader == true && documentFilterVM.MethodOfSearchForHeader == "IsExactMatchByHeader")
                filterQueries.Add(Query.Or(Query.EQ("Header", documentFilterVM.Header), Query.EQ("DynamicFiltrationFieldValue", documentFilterVM.Header)));

            if (idToDynamicFieldFilter != Guid.Empty)
                filterQueries.Add(Query<Document>.EQ(d => d.DynamicFiltrationFieldGuid, idToDynamicFieldFilter));
            //По типам документа
            if (docType != Guid.Empty)
                filterQueries.Add(Query<Document>.EQ(d => d.DocumentType.Id, docType));

            //по динамическим полям
            documentFilterVM.DynamicFields.ForEach(x =>
            {
                if (x.Value != null && x.IsSearchEnabled == true)
                {
                    //текстовые поля
                    if (x.FieldTemplate.FieldType.Id.ToString() == "8a37142c-0e29-4b40-b4a3-0a3a7d4f21d9"
                        || x.FieldTemplate.FieldType.Id.ToString() == "944388a1-b1e3-4a4d-910d-7ad9df107e20"
                        || x.FieldTemplate.FieldType.Id.ToString() == "f23165db-7c3d-49d5-bbc0-127eef90de36"
                        || x.FieldTemplate.FieldType.Id.ToString() == "a427dbfb-9cb7-4f52-9d5e-c7d0677e8103"
                        || x.FieldTemplate.FieldType.Id.ToString() == "fbdb27a4-b79c-524f-9d5e-c7d0677e8103"
                        )
                    {
                        // на вхождение
                        if (x.MethodOfSearchForDynamicValue == "IsInclusion")
                        {
                            filterQueries.Add(Query<Document>.ElemMatch<DocumentFieldValues>(y => y.FieldValues, builder =>
                                Query.And(
                                    builder.Matches(z => z.ValueToDisplay, new BsonRegularExpression(new Regex(x.Value.ToString(), RegexOptions.IgnoreCase))),
                                    builder.EQ(a => a.FieldTypeId, x.FieldTemplate.FieldType.Id)
                                )));
                        }
                        // точное совпадение
                        if (x.MethodOfSearchForDynamicValue == "IsExactMatch")
                        {
                            filterQueries.Add(Query.EQ("FieldValues.ValueToDisplay", x.Value.ToString()));
                        }
                    }

                    //даты или чекбоксы
                    if (x.FieldTemplate.FieldType.Id.ToString() == "d88f464a-ca95-4c41-ad7d-7df5adfd90d8" || x.FieldTemplate.FieldType.Id.ToString() == "2490becb-3476-43ab-8717-0f0b138a6ab2")
                    {
                        filterQueries.Add(Query<Document>.ElemMatch<DocumentFieldValues>(y => y.FieldValues, builder =>
                            Query.And(
                                builder.Matches(z => z.ValueToDisplay, new BsonRegularExpression(new Regex(x.Value.ToString(), RegexOptions.IgnoreCase))),
                                builder.EQ(a => a.FieldTypeId, x.FieldTemplate.FieldType.Id)
                            )));
                    }

                    //справочники
                    if (x.FieldTemplate.FieldType.DynamicReferenceId.ToString() != "00000000-0000-0000-0000-000000000000")
                    {
                        var recId = new Guid(x.Value.ToString());
                        filterQueries.Add(Query<Document>.ElemMatch<DocumentFieldValues>(y => y.FieldValues, builder =>
                            builder.EQ(z => z.DynamicRecordId, recId)));
                    }


                }


                //filterQueries.Add(Query.All("FieldValues.ValueToDisplay", ));
            });

            #endregion

            //if (!String.IsNullOrEmpty(dynamicValue))
            //{

            //    BsonArray bsonArr = new BsonArray();
            //   // dynamicValues.Add("123");
            //   //// dynamicValues.Add("центр");

            //   // Query.All("FieldValues.ValueToDisplay", dynamicValues);



            //    List<string> dynamicValues = new List<string>();
            //    dynamicValues.Add("мероприят");
            //    dynamicValues.Add("центр");

            //    foreach (var value in dynamicValues)
            //    {
            //        Query.Matches("FieldValues.ValueToDisplay", new BsonRegularExpression(new Regex(value, RegexOptions.IgnoreCase)));

            //        filterQueries.Add(Query.All("FieldValues.ValueToDisplay", ));
            //    }
            //}

            //if (!String.IsNullOrEmpty(dynamicValue))
            //{
            //    filterQueries.Add(Query.Matches("FieldValues.ValueToDisplay", new BsonRegularExpression(new Regex(dynamicValue, RegexOptions.IgnoreCase))));
            //}


            var endRes = Query.And(filterQueries);


            // var explaining = GetCollection().FindAs<Document>(Query.And(endRes, periodQuery)).SetSortOrder(sortOrder).Explain();

            //Возвращаем курсор   
            //GetCollection().FindAs<Document>(Query.And(resultQuery)).Explain();
            if (periodQuery != null)
            {
                // var explanation = GetCollection().FindAs<Document>(Query.And(endRes, periodQuery)).SetSortOrder(sortOrder).Explain();

                return GetCollection().FindAs<Document>(Query.And(endRes)).SetSortOrder(sortOrder);
            }
            else
            {
                // var explanation = GetCollection().FindAs<Document>(endRes).SetSortOrder(sortOrder).Explain();
                return GetCollection().FindAs<Document>(endRes).SetSortOrder(sortOrder);
            }


        }
        //!- Для нового грида

        public List<Document> getTimedOutDocument()
        {
            ////Document doc = new Document();
            //doc.
            return this.List(m => m.isDeleted == false).ToList();

        }

        public string getDocumentTypeName(Guid Id)
        {
            return GetCollection().AsQueryable().Where(m => m.Id == Id).Select(m => m.DocumentType.Name + " №" + m.DocumentNumber).FirstOrDefault();
            //return GetCollection().FindAs<Document>(Query<Document>.EQ(m => m.Id, Id)).FirstOrDefault().DocumentType.Name;
        }

        public List<Document> getSortedDocuments(bool? owner, string period, string sord, string sidx, bool usingSearch, List<Guid> foundResults)
        {
            var user = RepositoryFactory.GetCurrentUser();
            if (sidx.Contains("{UserId}"))
                sidx = sidx.Replace("{UserId}", RepositoryFactory.GetCurrentUser().ToString());
            SortByBuilder sortOrder = SortBy.Descending("DocumentViewers." + user.ToString() + ".Date");
            if (sord == "desc")
            {
                sortOrder = sortOrder.Descending(sidx);
            }
            else
            {
                sortOrder = sortOrder.Ascending(sidx);
            }



            IMongoQuery queryOwner = Query.EQ("Author.UserId", user);
            IMongoQuery queryAlien = Query.And(
                    Query.NE("Author.UserId", user),
                    Query.Exists("DocumentViewers." + user.ToString()),
                    Query.In("docState", new BsonArray(new Object[] { DocumentState.InWork, DocumentState.FinishedOk }))
                );
            IMongoQuery ownerQuery = null;

            string dateField = "DocumentViewers." + user.ToString() + ".Date";
            IMongoQuery periodQuery = null;
            switch (period)
            {
                case "oneMonth":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-1)));
                    break;
                case "threeMonth":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-3)));
                    break;
                case "sixMonth":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-6)));
                    break;
                case "year":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddYears(-1)));
                    break;
                case "allPeriod":
                    break;
                default:
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-1)));
                    break;
            }


            if (owner == null)
                ownerQuery = Query.Or(queryOwner, queryAlien);
            else
            {
                if (owner.Value)
                    ownerQuery = queryOwner;
                else
                    ownerQuery = queryAlien;
            }

            IMongoQuery query = null;
            if (periodQuery == null)
                query = ownerQuery;
            if (ownerQuery == null)
                query = periodQuery;
            if (periodQuery != null && ownerQuery != null)
                query = Query.And(ownerQuery, periodQuery);
            if (query == null)
                return null;

            if (usingSearch)
                return this.GetCollection().Find(Query.And(query, Query<Document>.In(m => m.Id, foundResults))).SetSortOrder(sortOrder).ToList();
            else
                return this.GetCollection().Find(query).SetSortOrder(sortOrder).ToList();
        }

        public long getSortedDocumentsCount(bool? owner, string period)
        {
            var user = RepositoryFactory.GetCurrentUser();


            IMongoQuery queryOwner = Query.EQ("Author.UserId", user);
            IMongoQuery queryAlien = Query.And(
                    Query.Not(Query.EQ("Author.UserId", user)),
                    Query.Exists("DocumentViewers." + user.ToString()),
                    Query.In("docState", new BsonArray(new Object[] { DocumentState.InWork, DocumentState.FinishedOk }))
                );
            IMongoQuery ownerQuery = null;

            string dateField = "DocumentViewers." + user.ToString() + ".Date";
            IMongoQuery periodQuery = null;
            switch (period)
            {
                case "oneMonth":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-1)));
                    break;
                case "threeMonth":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-3)));
                    break;
                case "sixMonth":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-6)));
                    break;
                case "year":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddYears(-1)));
                    break;
                case "allPeriod":
                    break;
                default:
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-1)));
                    break;
            }


            if (owner == null)
                ownerQuery = Query.Or(queryOwner, queryAlien);
            else
            {
                if (owner.Value)
                    ownerQuery = queryOwner;
                else
                    ownerQuery = queryAlien;
            }

            IMongoQuery query = null;
            if (periodQuery == null)
                query = ownerQuery;
            if (ownerQuery == null)
                query = periodQuery;
            if (periodQuery != null && ownerQuery != null)
                query = Query.And(ownerQuery, periodQuery);
            if (query == null)
                return 0;
            return this.GetCollection().Find(query).Count();
        }

        public List<Document> getSortedDocumentsByType(Guid docType, bool? owner, string period, string sord, string sidx, bool usingSearch, List<Guid> foundResults, Guid idToDynamicFieldFilter)
        {
            var user = RepositoryFactory.GetCurrentUser();
            if (sidx.Contains("{UserId}"))
                sidx = sidx.Replace("{UserId}", RepositoryFactory.GetCurrentUser().ToString());
            SortByBuilder sortOrder = SortBy.Descending("DocumentViewers." + user.ToString() + ".Date");
            if (sord == "desc")
            {
                sortOrder = sortOrder.Descending(sidx);
            }
            else
            {
                sortOrder = sortOrder.Ascending(sidx);
            }

            IMongoQuery queryOwner = Query.EQ("Author.UserId", user);
            IMongoQuery queryAlien = Query.And(
                    Query.Not(Query.EQ("Author.UserId", user)),
                    Query.Exists("DocumentViewers." + user.ToString()),
                    Query.In("docState", new BsonArray(new Object[] { DocumentState.InWork, DocumentState.FinishedOk }))
                );
            IMongoQuery ownerQuery = null;

            string dateField = "DocumentViewers." + user.ToString() + ".Date";
            IMongoQuery periodQuery = null;
            switch (period)
            {
                case "oneMonth":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-1)));
                    break;
                case "threeMonth":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-3)));
                    break;
                case "sixMonth":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-6)));
                    break;
                case "year":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddYears(-1)));
                    break;
                case "allPeriod":
                    break;
                default:
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-1)));
                    break;
            }


            if (owner == null)
                ownerQuery = Query.And(Query<Document>.EQ(d => d.DocumentType.Id, docType),
                                  Query.Or(queryOwner, queryAlien));
            else
            {
                if (owner.Value)
                    ownerQuery = Query.And(Query<Document>.EQ(d => d.DocumentType.Id, docType),
                            queryOwner);
                else
                    ownerQuery = Query.And(Query<Document>.EQ(d => d.DocumentType.Id, docType),
                        queryAlien);
            }


            IMongoQuery query = null;
            if (periodQuery == null)
                query = ownerQuery;
            else
                query = Query.And(ownerQuery, periodQuery);
            if (query == null)
                return null;

            if (idToDynamicFieldFilter != Guid.Empty)
            {
                query = Query.And(query, Query.EQ("DynamicFiltrationFieldGuid", idToDynamicFieldFilter));
            }

            if (usingSearch)
                return this.GetCollection().Find(Query.And(query, Query<Document>.In(m => m.Id, foundResults))).SetSortOrder(sortOrder).ToList();
            else
                return this.GetCollection().Find(query).SetSortOrder(sortOrder).ToList();

            //return this.GetCollection().Find(query).SetSortOrder(sortOrder).ToList();
        }

        public long getSortedDocumentsCountByType(Guid docType, bool? owner, string period)
        {
            var user = RepositoryFactory.GetCurrentUser();
            IMongoQuery queryOwner = Query.EQ("Author.UserId", user);
            IMongoQuery queryAlien = Query.And(
                    Query.Not(Query.EQ("Author.UserId", user)),
                    Query.Exists("DocumentViewers." + user.ToString()),
                    Query.In("docState", new BsonArray(new Object[] { DocumentState.InWork, DocumentState.FinishedOk }))
                );
            IMongoQuery ownerQuery = null;

            string dateField = "DocumentViewers." + user.ToString() + ".Date";
            IMongoQuery periodQuery = null;
            switch (period)
            {
                case "oneMonth":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-1)));
                    break;
                case "threeMonth":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-3)));
                    break;
                case "sixMonth":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-6)));
                    break;
                case "year":
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddYears(-1)));
                    break;
                case "allPeriod":
                    break;
                default:
                    periodQuery = Query.GTE(dateField, new BsonDateTime(DateTime.Today.AddMonths(-1)));
                    break;
            }


            if (owner == null)
                ownerQuery = Query.And(Query<Document>.EQ(d => d.DocumentType.Id, docType),
                                  Query.Or(queryOwner, queryAlien));
            else
            {
                if (owner.Value)
                    ownerQuery = Query.And(Query<Document>.EQ(d => d.DocumentType.Id, docType),
                            queryOwner);
                else
                    ownerQuery = Query.And(Query<Document>.EQ(d => d.DocumentType.Id, docType),
                        queryAlien);
            }


            IMongoQuery query = null;
            if (periodQuery == null)
                query = ownerQuery;
            else
                query = Query.And(ownerQuery, periodQuery);
            if (query == null)
                return 0;

            return this.GetCollection().Find(query).Count();

        }

        public object GetDocumentsCounts()
        {
            return new
            {
                all = getSortedDocumentsCount(null, "allPeriod"),
                docCounts = RepositoryFactory.GetRepository<DocumentType>().List(dt => !dt.isDeleted).Select(dt2 =>
                    new { id = dt2.Id, count = getSortedDocumentsCountByType(dt2.Id, null, "allPeriod") }).ToList()

            };
        }

        public List<Document> getAllTasks()
        {
            var user = RepositoryFactory.GetCurrentUser();


            return this.GetCollection().Find(
                  Query.ElemMatch("DocumentSignStages",
                     Query.And(
                     Query.And(
                     Query.And(
                         Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                         Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null)
                         ),
                         Query<RouteStage>.NE(rt => rt.RouteTypeId, new Guid("ACE11FAE-204E-40AF-BDD0-A686395390E6"))),
                         Query.ElemMatch("RouteUsers",
                             Query.And(
                                 Query<RouteStageUser>.EQ(u => u.SignUser.UserId, user),
                                 Query<RouteStageUser>.EQ(u => u.IsCurent, true)
                             ))
                     )
                  )
                  ).ToList();

        }

        public List<Document> getAllTasksForConfirmingPerform()
        {
            var user = RepositoryFactory.GetCurrentUser();


            return this.GetCollection().Find(
                  Query.ElemMatch("DocumentSignStages",
                     Query.And(
                     Query.And(
                         Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                         Query<RouteStage>.NE(rt => rt.ControlPerformForRouteStageUserId, null)
                         ),
                         Query.ElemMatch("RouteUsers",
                             Query.And(
                                 Query<RouteStageUser>.EQ(u => u.SignUser.UserId, user),
                                 Query<RouteStageUser>.EQ(u => u.IsCurent, true)
                             ))
                     )
                  )
                  ).ToList();

        }

        //Кол-во тасков
        public long getAllTasksCount(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();



            var query =
                Query.ElemMatch(
                    "DocumentSignStages",
                    Query.And(
                        Query.And(
                            Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                            Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null),
                            Query<RouteStage>.NE(rt => rt.RouteTypeId, new Guid("ace11fae-204e-40af-bdd0-a686395390e6"))),
                        Query.ElemMatch(
                            "RouteUsers",
                            Query.And(
                                Query<RouteStageUser>.EQ(u => u.SignUser.UserId, user),
                                Query<RouteStageUser>.EQ(u => u.IsCurent, true)))));

            if (isForReport)
            {
                query = Query.And(
                    Query.Or(
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                    )
                    , query);
            }

            return this.GetCollection().Find(query).Count();
        }

        public long getAllNewTasksCount(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();



            var query = Query.And(
                Query.ElemMatch(
                    "DocumentSignStages",
                    Query.And(
                        Query.And(
                            Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                            Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null),
                            Query<RouteStage>.NE(rt => rt.RouteTypeId, new Guid("ace11fae-204e-40af-bdd0-a686395390e6"))),
                        Query.ElemMatch(
                            "RouteUsers",
                            Query.And(
                                Query<RouteStageUser>.EQ(u => u.SignUser.UserId, user),
                                Query<RouteStageUser>.EQ(u => u.IsCurent, true))))),
                Query.EQ("DocumentViewers." + user.ToString() + ".ViewDateTime", BsonNull.Value));

            if (isForReport)
            {
                query = Query.And(
                    Query.Or(
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                    )
                    , query);
            }


            return this.GetCollection().Find(query).Count();
        }

        public long getAllTasksForConfirmingPerformCount(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();



            var query = Query.ElemMatch("DocumentSignStages",
                Query.And(
                Query.And(
                    Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                    Query<RouteStage>.NE(rt => rt.ControlPerformForRouteStageUserId, null)
                    ),
                    Query.ElemMatch("RouteUsers",
                        Query.And(
                            Query<RouteStageUser>.EQ(u => u.SignUser.UserId, user),
                            Query<RouteStageUser>.EQ(u => u.IsCurent, true)
                        ))
                )
             );


            if (isForReport)
            {
                query = Query.And(
                    Query.Or(
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                    )
                    , query);
            }
            return this.GetCollection().Find(query).Count();

        }

        public long getAllNewTasksForConfirmingPerformCount(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();



            var query = Query.And(
                Query.ElemMatch(
                    "DocumentSignStages",
                    Query.And(
                        Query.And(
                            Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                            Query<RouteStage>.NE(rt => rt.ControlPerformForRouteStageUserId, null)),
                        Query.ElemMatch(
                            "RouteUsers",
                            Query.And(
                                Query<RouteStageUser>.EQ(u => u.SignUser.UserId, user),
                                Query<RouteStageUser>.EQ(u => u.IsCurent, true))))),
                Query.ElemMatch(
                    "NewDocumentViewers",
                    Query.And(Query.EQ("UserId", user), Query.EQ("ViewDateTime", BsonNull.Value))));


            if (isForReport)
            {
                query = Query.And(
                    Query.Or(
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                    )
                    , query);
            }

            return this.GetCollection().Find(query).Count();

        }

        public long getAllBadTasksCount(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();



            var query = Query.And(
                 Query.ElemMatch(
                     "DocumentSignStages",
                     Query.And(
                         Query.And(
                             Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                             Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null),
                             Query<RouteStage>.NE(rt => rt.RouteTypeId, new Guid("ace11fae-204e-40af-bdd0-a686395390e6"))),
                         Query.ElemMatch(
                             "RouteUsers",
                             Query.And(
                                 Query<RouteStageUser>.EQ(u => u.SignUser.UserId, user),
                                 Query<RouteStageUser>.EQ(u => u.IsCurent, true))))),
                 Query.LT("FinishDate", DateTime.Now.Date));

            if (isForReport)
            {
                query = Query.And(
                    Query.Or(
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                    )
                    , query);
            }

            return this.GetCollection().Find(query).Count();
        }

        public long getAllBadTasksForConfirmingPerformCount(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();



            var query = Query.And(
                Query.ElemMatch(
                    "DocumentSignStages",
                    Query.And(
                        Query.And(
                            Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                            Query<RouteStage>.NE(rt => rt.ControlPerformForRouteStageUserId, null)),
                        Query.ElemMatch(
                            "RouteUsers",
                            Query.And(
                                Query<RouteStageUser>.EQ(u => u.SignUser.UserId, user),
                                Query<RouteStageUser>.EQ(u => u.IsCurent, true))))),
                Query.LT("FinishDate", DateTime.Now.Date));

            if (isForReport)
            {
                query = Query.And(
                    Query.Or(
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                    )
                    , query);
            }


            return this.GetCollection().Find(query).Count();

        }

        //!Кол-во тасков


        public long getworkingDocumentsForUser()
        {
            var user = RepositoryFactory.GetCurrentUser();

            return this.GetCollection().Find(Query.And(
                Query.ElemMatch("NewDocumentViewers", Query.EQ("UserId", user)),
                            //   Query.Exists("DocumentViewers." + user.ToString()),
                            Query.EQ("docState", DocumentState.InWork))
                        ).Count();
        }

        public long getnewDocumentsForUser()
        {
            var user = RepositoryFactory.GetCurrentUser();

            return this.GetCollection().Find(Query.And(
                 Query.ElemMatch("NewDocumentViewers", Query.EQ("UserId", user)),
                            //  Query.Exists("DocumentViewers." + user.ToString()),
                            //Query.EQ("DocumentViewers." + user.ToString() + ".ViewDateTime", BsonNull.Value))
                            Query.ElemMatch("NewDocumentViewers", Query.And(Query.EQ("UserId", user), Query.EQ("ViewDateTime", BsonNull.Value)))
                        )).Count();
        }

        public void SetDocumentViewDateByViewer(string documentViewer, Guid documentId)
        {

            var documentViewers = this.GetCollection().Find(Query<Document>.EQ(m => m.Id, documentId)).SetFields(Fields.Include("Id", "DocumentViewers")).Select(m => m.DocumentViewers).FirstOrDefault();

            if (documentViewers.Any(m => m.Key == documentViewer))
            {
                this.GetCollection().Update(
                    Query<Document>.EQ(m => m.Id, documentId),
                    Update.Set("DocumentViewers." + documentViewer + ".ViewDateTime", DateTime.Now)
                    );

                var document = RepositoryFactory.GetDocumentRepository().Single(m => m.Id == documentId);

                if (document.NewDocumentViewers == null)
                    document.NewDocumentViewers = new List<DocumentViewer>();

                document.NewDocumentViewers.Where(m => m.UserId == new Guid(documentViewer)).ToList().ForEach(k =>
                {
                    k.ViewDateTime = DateTime.Now;

                });
                RepositoryFactory.GetDocumentRepository().update(document);
            }

        }

        public List<Document> getAllBadTasksForStatsReport(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();
            List<Document> result = new List<Document>();

            var query = Query.And(
                Query<Document>.NE(m => m.docState, DocumentState.FinishedOk),
                Query.ElemMatch(
                    "DocumentSignStages",
                    Query.And(
                        Query.And(
                            Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                            Query<RouteStage>.NE(rt => rt.RouteTypeId, new Guid("ace11fae-204e-40af-bdd0-a686395390e6"))
                            //,Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null),                        
                            //,Query.Not(Query<RouteStage>.EQ(rt => rt.RouteTypeId, new Guid("ace11fae-204e-40af-bdd0-a686395390e6")))
                            ),
                        Query.ElemMatch(
                            "RouteUsers",
                            Query.And(
                                Query<RouteStageUser>.EQ(u => u.SignUser.UserId, user),
                                Query<RouteStageUser>.EQ(u => u.IsCurent, true))))),
                Query.LT("FinishDate", DateTime.Now.Date));


            if (isForReport)
            {
                query = Query.And(
                    Query.Or(
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                    )
                    , query);
            }


            result.AddRange(this.GetCollection().Find(
                query
                  ).ToList());

            var documentsWithInstruction = RepositoryFactory.GetInstructionRepository().getAllBadTasksDocumentsGuids(userId, isForReport);

            result.AddRange(this.List(m => documentsWithInstruction.Contains(m.Id)).ToList());
            var result1 = result.Distinct(new DocumentComparer()).ToList();
            return result1;
        }

        public IEnumerable<Document> GetInboxDocNumbersForSelect2(string term, int page, int pageLimit)
        {
            IMongoQuery query = null;

            if (String.IsNullOrEmpty(term))
            {
                query = Query.And(Query<Document>.EQ(x => x.isDeleted, false),
                    Query.Or(
                    Query<Document>.EQ(x => x.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Document>.EQ(x => x.DocumentType.Id, new Guid("e994583d-2ef8-4368-9ed5-5f4439374197"))
                    )
                    );
            }
            else
            {
                query = Query.And(
                    Query<Document>.EQ(x => x.isDeleted, false),

                    Query.Or(
                    Query<Document>.EQ(x => x.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Document>.EQ(x => x.DocumentType.Id, new Guid("e994583d-2ef8-4368-9ed5-5f4439374197"))
                    ),

                    Query.Or(
                    Query<Document>.Matches(x => x.DocumentNumber, new BsonRegularExpression(new Regex(term, RegexOptions.IgnoreCase))),
                    Query<Document>.Matches(x => x.Header, new BsonRegularExpression(new Regex(term, RegexOptions.IgnoreCase)))
                    )
                    );
            }

            var skip = pageLimit * page - pageLimit;
            return GetCollection().FindAs<Document>(query).SetSkip(skip).SetLimit(pageLimit).SetFields("Id", "DocumentNumber", "Header");

        }

        public long CountOfDocuments(string term)
        {
            IMongoQuery query = null;

            if (String.IsNullOrEmpty(term))
            {

                query = Query.And(Query<Document>.EQ(x => x.isDeleted, false),
                            Query.Or(
                        Query<Document>.EQ(x => x.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                        Query<Document>.EQ(x => x.DocumentType.Id, new Guid("e994583d-2ef8-4368-9ed5-5f4439374197"))
                        )
                    );
            }
            else
            {
                query = Query.And(
                    Query<Document>.EQ(x => x.isDeleted, false),

                    Query.Or(
                    Query<Document>.EQ(x => x.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Document>.EQ(x => x.DocumentType.Id, new Guid("e994583d-2ef8-4368-9ed5-5f4439374197"))
                    ),

                    Query.Or(
                    Query<Document>.Matches(x => x.DocumentNumber, new BsonRegularExpression(new Regex(term, RegexOptions.IgnoreCase))),
                    Query<Document>.Matches(x => x.Header, new BsonRegularExpression(new Regex(term, RegexOptions.IgnoreCase)))
                    )
                    );
            }

            return GetCollection().FindAs<Document>(query).Count();
        }

    }

    public class DocumentComparer : IEqualityComparer<Document>
    {
        // Products are equal if their names and product numbers are equal. 
        public bool Equals(Document x, Document y)
        {
            return (x.Id == y.Id || x.DocumentNumber == y.DocumentNumber);
        }


        public int GetHashCode(Document obj)
        {
            int hashDocumentCode = obj.Id.GetHashCode();
            return hashDocumentCode;
        }
    }
}
