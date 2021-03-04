using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.Document.Route;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devir.DMS.DL.Extensions;

namespace Devir.DMS.DL.Repositories
{
    public class InstructionRepository : RepositoryBase<Instruction>
    {
        public InstructionRepository(MongoDatabase database, string collectionName, Guid userId)
            : base(database, collectionName, userId)
        {
        }


        public int GetInstructionOutOfDate(Instruction i)
        {
            return (DateTime.Now.Date - i.FinishDate.Date).Days;
        }

        public string GetInstructionStatus(Instruction i)
        {
            if (i.docState == DocumentState.InWork
                && i.DocumentSignStages.Any(dst => dst.isCurrent && dst.ControlPerformForRouteStageUserId != null))
                return "Контроль исполнения";
            if (i.docState == DocumentState.InWork && i.FinishDate.Date < DateTime.Now)
                return "Просроченный";
            if (i.docState == DocumentState.InWork)
                return "На исполнении";

            if (i.docState == DocumentState.FinishedOk)
                return "Выполненый";
            return "";
        }

        public IEnumerable<dynamic> GetInstructionsNew(string type, int startRecord, int recordsOnPage, string sortColumn, int sortDirection, string groupColumn)
        {
            IMongoQuery query = GetInstructionsQuery(type);

            if (query == null)
                return null;

            sortColumn = "FinishDate";
            var sortOrder = sortDirection == 0 ? SortBy.Descending(sortColumn) : SortBy.Ascending(sortColumn);
            //var sortOrder = "FinishDate";

            var instrList = GetCollection().Find(query).SetSortOrder(sortOrder).SetSkip(startRecord).Take(recordsOnPage).ToList();

            var rusultList = instrList.Select(i => new DocumentInstructionViewModel
            {
                Id = i.Id,
                Status = type == "all" ? GetInstructionStatus(i) : "",
                DocumentType = RepositoryFactory.GetDocumentRepository().getDocumentTypeName(i.RootDocumentId),
                Header = i.Header,
                RootDocumentId = i.RootDocumentId,
                UserFor = i.UserFor.GetFIO(),
                CreateDate = i.CreateDate.ToString("dd.MM.yyyy"),
                OutOfDate = type == "outOfDate" ? GetInstructionOutOfDate(i) : 0,
                FinishDate = i.FinishDate.ToString("dd.MM.yyyy"),
                CompletedDate = i.DocumentSignStages[0].RouteUsers[i.DocumentSignStages[0].RouteUsers.Count() - 1].SignResult != null ? i.DocumentSignStages[0].RouteUsers[i.DocumentSignStages[0].RouteUsers.Count() - 1].SignResult.Date.ToString("dd.MM.yyyy") : "В работе",
                ApproveDate = getInstructionPerformDate(i),
                Classes = "",
                //hrefAddress = "GetDocument?DocumentId=" + i.Id,

                Date = i.FinishDate,
                gDate = i.FinishDate.ToString("dd.MM.yyyy"),
                sDate = i.FinishDate.ToString("dd.MM.yyyy"),
                isNew = i.DocumentViewers.Any(dv => dv.Key == RepositoryFactory.GetCurrentUser().ToString()) && i.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].ViewDateTime == null,
                isUrgent = false,
                isExpired = getInstructionIsExpired(i),
                ExpiredTimeSpan = getInstructionExpireDays(i),

            }).GroupByMany(groupColumn).Select(m => new
            {
                Group = m.Key, //.ToString("dd.MM.yyyy")
                Visible = true,
                Values = m.Items,
            }).ToList();

            return rusultList;
        }

        private string getInstructionPerformDate(Instruction i)
        {

            var controlPerformStage = i.DocumentSignStages.LastOrDefault(rs => rs.ControlPerformForRouteStageUserId != null);
            if (controlPerformStage != null)
            {
                if (controlPerformStage.RouteUsers.Any(m => m.SignResult != null && m.SignResult.ActionId == new Guid("7ac2f1da-01ff-4fb8-a338-74046f3d25ec")))
                    return "";

                return (controlPerformStage.FinishDate != null ? controlPerformStage.FinishDate.Value.ToString("dd.MM.yyyy") : "");
            }
            else
                return "";

            // i.DocumentSignStages.SingleOrDefault(rs => rs.ControlPerformForRouteStageUserId != null).FinishDate != null ? i.DocumentSignStages.SingleOrDefault(rs => rs.ControlPerformForRouteStageUserId != null).FinishDate.Value.ToString("dd.MM.yyyy") : "")\\
        }

        private bool getInstructionIsExpired(Instruction i)
        {
            //bool result = false;
            var controlPerformStage = i.DocumentSignStages.LastOrDefault(rs => rs.ControlPerformForRouteStageUserId != null);
            if (controlPerformStage != null)
                return (controlPerformStage.FinishDate != null ? controlPerformStage.FinishDate : DateTime.Now) > i.FinishDate;
            else
                return DateTime.Now > i.FinishDate;
        }

        private TimeSpan getInstructionExpireDays(Instruction i)
        {
            var controlPerformStage = i.DocumentSignStages.LastOrDefault(rs => rs.ControlPerformForRouteStageUserId != null);
            if (controlPerformStage != null)
                return (controlPerformStage.FinishDate != null ? controlPerformStage.FinishDate.Value : DateTime.Now) - i.FinishDate;
            else
                return DateTime.Now - i.FinishDate;
        }


        public List<Instruction> GetInstructions(string type, string sord, string sidx, int start, int rows)
        {

            IMongoQuery query = GetInstructionsQuery(type);

            if (query == null)
                return null;
            var sortOrder = sord == "desc" ? SortBy.Descending(sidx) : SortBy.Ascending(sidx);

            return GetCollection().Find(query).SetSortOrder(sortOrder).Skip(start).Take(rows).ToList();
        }

        public List<Instruction> getAllTasks()
        {
            var user = RepositoryFactory.GetCurrentUser();

            return this.GetCollection().Find(
                Query.And(Query<Instruction>.EQ(m => m.isDeleted, false),
                  Query.ElemMatch("DocumentSignStages",
                     Query.And(

                     Query.And(
                         Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                         Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null)
                         ),
                         Query.ElemMatch("RouteUsers",
                             Query.And(
                                 Query<RouteStageUser>.EQ(u => u.SignUser.UserId, user),
                                 Query<RouteStageUser>.EQ(u => u.IsCurent, true)
                             ))
                     )
                  ))
                  ).ToList();

        }

        //Кол-во тасков
        public long getAllTasksCount(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();



            var query = Query.And(
                Query<Instruction>.EQ(m => m.isDeleted, false),
                Query.ElemMatch(
                    "DocumentSignStages",
                    Query.And(
                        Query.And(
                            Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                            Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null)),
                        Query.ElemMatch(
                            "RouteUsers",
                            Query.And(
                                Query<RouteStageUser>.EQ(u => u.SignUser.UserId, user),
                                Query<RouteStageUser>.EQ(u => u.IsCurent, true))))));

            if (isForReport)
            {
                query = Query.And(
                    Query.Or(
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                    )
                    , query);
            }

                  return this.GetCollection().Find(query).Count();
        }

        public long getAllNewTasksCount(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();



           var query =  Query.And(
                Query<Instruction>.EQ(m => m.isDeleted, false),
                Query.ElemMatch(
                    "DocumentSignStages",
                    Query.And(
                        Query.And(
                            Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                            Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null)),
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
                   Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                   Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                   Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                   Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                   )
                   , query);
           }

                  return this.GetCollection().Find(query).Count();
        }


        public long getAllTasksForConfirmingPerformCount(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();



            var query = Query.And(
                Query<Instruction>.EQ(m => m.isDeleted, false),
                Query.ElemMatch(
                    "DocumentSignStages",
                    Query.And(
                        Query.And(
                            Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                            Query.Not(Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null))),
                        Query.ElemMatch(
                            "RouteUsers",
                            Query.And(
                                Query<RouteStageUser>.EQ(u => u.SignUser.UserId, user),
                                Query<RouteStageUser>.EQ(u => u.IsCurent, true))))));


            if (isForReport)
            {
                query = Query.And(
                    Query.Or(
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                    )
                    , query);
            }

                   return this.GetCollection().Find(query).Count();

        }

        public long getAllNewTasksForConfirmingPerformCount(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();



            var query = Query.And(
                Query<Instruction>.EQ(m => m.isDeleted, false),
                Query.ElemMatch(
                    "DocumentSignStages",
                    Query.And(
                        Query.And(
                            Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                            Query.Not(Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null))),
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
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                    )
                    , query);
            }


                   return this.GetCollection().Find(query).Count();

        }

        public long getAllBadTasksCount(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();



          var query =   Query.And(
                Query<Instruction>.EQ(m => m.isDeleted, false),
                Query.ElemMatch(
                    "DocumentSignStages",
                    Query.And(
                        Query.And(
                            Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                            Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null)),
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
                  Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                  Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                  Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                  Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                  )
                  , query);
          }


                  return this.GetCollection().Find(query).Count();
        }

        public List<Instruction> getAllBadTasks(DateTime createDateTimeBegin, DateTime createDateTimeEnd,
            String sortColumn = "", int sortDirection = 0, String groupColumn = "",
            Guid departmentId = default(Guid), Guid userId = default(Guid), Guid docTypeId = default(Guid))
        {

            List<IMongoQuery> queries = new List<IMongoQuery>();

            queries.Add(Query.And(
                 Query<Instruction>.EQ(m => m.isDeleted, false),
                  Query.ElemMatch("DocumentSignStages",
                     Query.And(
                     Query.And(
                         Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                         Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null)
                         )
                     )
                     ),
                     Query.LT("FinishDate", DateTime.Now),
                     Query.LT("CreateDate", createDateTimeEnd),
                     Query.GT("CreateDate", createDateTimeBegin)
                  ));


            //Устанавливаем сортировку, по умолчанию по кол-ву дней просрочки.
            SortByBuilder sortOrder = null;
            if (String.IsNullOrEmpty(sortColumn))                            
                sortOrder = sortDirection == 0 ? SortBy.Descending("FinishDate") : SortBy.Ascending("FinishDate");            
            else
                sortOrder = sortDirection == 0 ? SortBy.Descending(sortColumn) : SortBy.Ascending(sortColumn);


            if (departmentId != Guid.Empty)
                queries.Add(Query<Instruction>.EQ(x => x.UserFor.DepartmentId, departmentId));

            if (userId != Guid.Empty)
                queries.Add(Query<Instruction>.EQ(x => x.UserFor.UserId, userId));
            
            if (docTypeId != Guid.Empty)
                queries.Add(Query<Instruction>.EQ(x => x.DocumentType.Id, docTypeId));

            return this.GetCollection().FindAs<Instruction>(Query.And(queries)).SetSortOrder(sortOrder).ToList();
        }

        public long getAllBadTasksForConfirmingPerformCount(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();



           var query = Query.And(
                Query<Instruction>.EQ(m => m.isDeleted, false),
                Query.ElemMatch(
                    "DocumentSignStages",
                    Query.And(
                        Query.And(
                            Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                            Query.Not(Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null))),
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
                   Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                   Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                   Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                   Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                   )
                   , query);
           }

                   return this.GetCollection().Find(query).Count();

        }

        //!Кол-во тасков

        public List<Instruction> getAllTasksForConfirmingPerform()
        {
            var user = RepositoryFactory.GetCurrentUser();


            return this.GetCollection().Find(
                  Query.And(
                 Query<Instruction>.EQ(m => m.isDeleted, false),
                  Query.ElemMatch("DocumentSignStages",
                     Query.And(
                     Query.And(
                         Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                         Query.Not(Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null))
                         ),
                         Query.ElemMatch("RouteUsers",
                             Query.And(
                                 Query<RouteStageUser>.EQ(u => u.SignUser.UserId, user),
                                 Query<RouteStageUser>.EQ(u => u.IsCurent, true)
                             ))
                     )
                  ))
                  ).ToList();

        }

        public long GetInstructionsCount(string type)
        {
            IMongoQuery query = GetInstructionsQuery(type);
            if (query == null)
                return 0;
            return GetCollection().Find(query).Count();
        }




        public IMongoQuery GetInstructionsQuery(string type)
        {



            var user = RepositoryFactory.GetCurrentUser();

            //GetCollection().EnsureIndex(new IndexKeysBuilder().Ascending("DocumentViewers." + user.ToString() + ".Date"));
            GetCollection().EnsureIndex(new IndexKeysBuilder().Ascending("Author.UserId"));
            GetCollection().EnsureIndex(new IndexKeysBuilder().Ascending("docState"));
            GetCollection().EnsureIndex(new IndexKeysBuilder<Document>().Ascending(m => m.DocumentType.Id));


            IMongoQuery query = null;
            if (type == "all")
                query = Query.And(Query<Instruction>.EQ(m => m.isDeleted, false), Query.EQ("Author.UserId", user),
                       Query.Not(Query.EQ("docState", DocumentState.FinishedOk)));
            if (type == "outOfDate")
                query = Query.And(
                    Query<Instruction>.EQ(m => m.isDeleted, false),
                            Query.EQ("Author.UserId", user),
                            Query<Instruction>.LT(i => i.FinishDate, DateTime.Now.Date),
                            Query.EQ("docState", DocumentState.InWork),
                            Query.Not(Query.ElemMatch("DocumentSignStages",
                                Query.And(
                                    Query.EQ("isCurrent", BsonBoolean.True),
                                    Query.Not(Query.EQ("ControlPerformForRouteStageUserId", BsonNull.Value))
                                ))
                            )
                        );
            if (type == "inComplete")
                query = Query.And(
                    Query<Instruction>.EQ(m => m.isDeleted, false),
                            Query.EQ("Author.UserId", user),
                    //Query<Instruction>.GTE(i => i.FinishDate, DateTime.Now),
                            Query.EQ("docState", DocumentState.InWork),
                            Query.Not(Query.ElemMatch("DocumentSignStages",
                                Query.And(
                                    Query.EQ("isCurrent", BsonBoolean.True),
                                    Query.Not(Query.EQ("ControlPerformForRouteStageUserId", BsonNull.Value))
                                ))
                            )
                        );
            if (type == "control")
                query = Query.And(
                    Query<Instruction>.EQ(m => m.isDeleted, false),
                            Query.EQ("Author.UserId", user),
                            Query.ElemMatch("DocumentSignStages",
                                Query.And(
                                    Query<RouteStage>.EQ(rt => rt.isCurrent, true),
                                    Query.Not(Query.EQ("ControlPerformForRouteStageUserId", BsonNull.Value))
                                )
                           ),
                           Query.EQ("docState", DocumentState.InWork)
                        );
            if (type == "completed")
                query = Query.And(
                    Query<Instruction>.EQ(m => m.isDeleted, false),
                            Query.EQ("Author.UserId", user),
                            Query.EQ("docState", DocumentState.FinishedOk)
                        );
            return query;
        }

        public object GetInstructionsCounts()
        {

            return new
            {
                instCountAll = GetInstructionsCount("all"),
                instCountOutOfDate = GetInstructionsCount("outOfDate"),
                instCountInComplete = GetInstructionsCount("inComplete"),
                instCountControl = GetInstructionsCount("control"),
                instCountCompleted = GetInstructionsCount("completed")
            };
        }

        public void SetDocumentViewDateByViewer(string documentViewer, Guid documentId)
        {
            this.GetCollection().Update(
                Query<Document>.EQ(m => m.Id, documentId),
                Update.Set("DocumentViewers." + documentViewer + ".ViewDateTime", DateTime.Now)
                );
            var document = RepositoryFactory.GetInstructionRepository().Single(m => m.Id == documentId);

            if (document.NewDocumentViewers == null)
                document.NewDocumentViewers = new List<DocumentViewer>();


            document.NewDocumentViewers.Where(m => m.UserId == new Guid(documentViewer)).ToList().ForEach(k =>
            {
                k.ViewDateTime = DateTime.Now;

            });
            RepositoryFactory.GetInstructionRepository().update(document);
        }


        public List<Guid> getAllBadTasksDocumentsGuids(Guid? userId = null, bool isForReport = false)
        {
            var user = userId ?? RepositoryFactory.GetCurrentUser();


            var query = Query.And(
                Query<Instruction>.NE(m => m.docState, DocumentState.FinishedOk),
                Query<Instruction>.EQ(m => m.isDeleted, false),
                Query.ElemMatch(
                    "DocumentSignStages",
                    Query.And(
                        Query.And(
                            Query<RouteStage>.EQ(rt => rt.isCurrent, true)
                            //,Query<RouteStage>.EQ(rt => rt.ControlPerformForRouteStageUserId, null)
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
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc")),
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                    Query<Instruction>.EQ(m => m.RootDocumentTypeId, new Guid("43f81f3e-1539-419e-8463-19b14495f575"))
                    )
                    , query);
            }

            return this.GetCollection().Find(
                 query
                  ).SetFields(Fields.Include("Id", "RootDocumentId")).ToList().Select(m => m.RootDocumentId).ToList();
        }

    }
}
