using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Repositories;
using Devir.DMS.Web.Models.Document;
using Devir.DMS.Web.Models.Reference;
using Devir.DMS.DL.Models.Document.StorageTypes;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.BL.DocumentRouting;
using Devir.DMS.DL.Models.Document.DocumentNotifications;
using Devir.DMS.Web.Helpers;
using Devir.DMS.DL.Models.WebNotifications;
using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Models.Document.Route;
using System.Reflection;
using Devir.DMS.Web.Models;
using Devir.DMS.DL.Models.References.DynamicReferences;
using System.Threading;
using Devir.DMS.DL.Models.DocumentTemplates;
using Devir.DMS.DL.Models.Filter;


namespace Devir.DMS.Web.Controllers
{
    public class DocumentController : Base.BaseController
    {
        //
        // GET: /Document/

        public ActionResult Documents(string type = "", string id = "")
        {
            var tmpDocumentTypes = RepositoryFactory.GetRepository<DocumentType>().List(m => !m.isDeleted && m.RouteTemplates != null && m.RouteTemplates.Any() && m.DocumentVisualTemplate != null && m.DocumentVisualTemplate.Any()).ToList();
            ViewBag.DocumentTypes = tmpDocumentTypes;
            tmpDocumentTypes.ForEach(dt =>
            {
                var tmp = GetTemplates(dt.Id);
                ViewData.Add(dt.Id.ToString(), tmp);
            });
            return View();
        }

        public ActionResult DocumentsView(string type)
        {
            if (type != "All")
            {
                var documentType = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == new Guid(type));

                DocumentDynamicFieldFiltration documentDynamicFieldFiltration = null;
                if (documentType.FieldForAdditionalMenuFiltering != Guid.Empty)
                {

                    //var dynRef = RepositoryFactory.GetRepository<DynamicRecord>



                    var field = documentType.PrimaryFiltrationField == Guid.Empty ? documentType.FieldTemplates.Single(m => m.Id == documentType.FieldForAdditionalMenuFiltering) : documentType.FieldTemplates.Single(m => m.Id == documentType.PrimaryFiltrationField);
                    var fieldType = RepositoryFactory.GetRepository<FieldType>().Single(m => m.Id == field.FieldType.Id);



                    var dynRef = RepositoryFactory.GetRepository<DynamicReference>().Single(m => m.Id == fieldType.DynamicReferenceId && !m.isDeleted);
                    var tmpList = new List<SelectListItem>();
                    if (dynRef != null)
                    {
                        var fieldToDisplay = dynRef.FieldTemplates.Single(m => m.isDisplay);


                        tmpList.Add(new SelectListItem() { Value = Guid.Empty.ToString(), Text = "Все", Selected = true });
                        tmpList.AddRange(RepositoryFactory.GetRepository<DynamicRecord>().List(m => !m.isDeleted && m.DynamicReferenceId == dynRef.Id).
                      GroupBy(m => m.RecordId).
                      Select(m => new SelectListItem() { Value = m.Key.ToString(), Text = m.Single(n => n.DynamicReferenceFieldTemplateId == fieldToDisplay.Id).Value.Value.ValueToDisplay }).ToList());
                    }
                    documentDynamicFieldFiltration = new DocumentDynamicFieldFiltration()
                    {
                        FieldTemplateId = documentType.FieldForAdditionalMenuFiltering,
                        FiledName = field.Header,
                        Options = tmpList
                    };

                }

                ViewBag.documentDynamicFieldFiltration = documentDynamicFieldFiltration;

            }
            if (string.IsNullOrEmpty(type))
                return View();
            if (type == "All")
            {
                return View("AllDocuments");
            }
            else
            {
                ViewBag.DocType = type;
                return View("DocumentsByType");
            }
        }


        public ActionResult DocumentFiltration()
        {
            return View();
        }



        public ActionResult GetAllDocumentsNew(int page, int recordsOnPage, string sortColumn, string groupColumn, int sortDirection, string owner, string period,
            string searchPhrase, DocumentFilterVM documentFilterVM)
        {

            List<FullTextSearch.ServiceFTSFoundDocuments> searchResults = new List<FullTextSearch.ServiceFTSFoundDocuments>();
            List<Guid> searchResultGuids = null;

            if (!String.IsNullOrEmpty(searchPhrase))
                using (FullTextSearch.ServiceFTSClient clnt = new FullTextSearch.ServiceFTSClient())
                {
                    searchResultGuids = new List<Guid>();

                    if (RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == RepositoryFactory.GetCurrentUser()).InRole("Канцелярия"))
                    {
                        searchResults = clnt.SearchDocuments(searchPhrase, "", "");
                    }
                    else
                    {
                        searchResults = clnt.SearchDocuments(searchPhrase, RepositoryFactory.GetCurrentUser().ToString(), "");
                    }

                    searchResultGuids = searchResults != null ? searchResults.Select(m => new Guid(m.DocId)).ToList() : null; //.Where(m=>m.Score>0.1)
                }

            var total = RepositoryFactory.GetDocumentRepository().GetListForAllDocumentsGridCount(RepositoryFactory.GetCurrentUser(), owner, period, searchResultGuids, Guid.Empty, Guid.Empty,

                documentFilterVM);
            documentFilterVM = documentFilterVM ?? new DocumentFilterVM();
            documentFilterVM.Header = "по открытию л/счета";

            List<DocumentsViewM> list = RepositoryFactory.GetDocumentRepository().GetListForAllDocumentsGrid((page * recordsOnPage), recordsOnPage, RepositoryFactory.GetCurrentUser(),
                                                                sortColumn, sortDirection, groupColumn, owner, period, searchResultGuids, Guid.Empty, Guid.Empty,
                                                               documentFilterVM, false, searchPhrase).ToList();
            var query = MongoDB.Driver.Builders.Query.EQ("Header", MongoDB.Bson.BsonValue.Create("по открытию л/счета"));
            var test = RepositoryFactory.GetRepository<Document>().GetCollection().Find(query).ToList();
            //var list = RepositoryFactory.GetDocumentRepository().GetListForAllDocumentsGrid((page * recordsOnPage), recordsOnPage, RepositoryFactory.GetCurrentUser(),
            //                                                 sortColumn, sortDirection, groupColumn, owner, period, searchResultGuids, Guid.Empty, Guid.Empty,
            //                                                documentFilterVM).ToList();


            //var asd = RepositoryFactory.GetRepository<Document>().Single(s=>s.Id == Guid.Empty).DocumentViewers.Select(dw=>dw.Key == RepositoryFactory.GetCurrentUser().ToString()).;
            var cnt = 0;
            list.ForEach(item =>
            {
                cnt += item.DataCount;
            });

            return Json(new
            {
                Data = list,
                DataCount = cnt,
                More = total >= (page * recordsOnPage) + recordsOnPage,
                isCancelyaria = RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == RepositoryFactory.GetCurrentUser()).InRole("Канцелярия")
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetAllDocuments(string owner, string searchText, string period, string sidx, string sord)
        {

            bool? isOwner = null;
            if (owner == "my")
                isOwner = true;
            if (owner == "alien")
                isOwner = false;
            List<FullTextSearch.ServiceFTSFoundDocuments> searchResults = new List<FullTextSearch.ServiceFTSFoundDocuments>();
            List<Guid> searchResultGuids = new List<Guid>();

            if (!String.IsNullOrEmpty(searchText))
                using (FullTextSearch.ServiceFTSClient clnt = new FullTextSearch.ServiceFTSClient())
                {
                    searchResults = clnt.SearchDocuments(searchText, RepositoryFactory.GetCurrentUser().ToString(), "");
                    searchResultGuids = searchResults.Select(m => new Guid(m.DocId)).ToList();
                }

            JsonResult result = new JqGridHelper().GetDocumentsGridResult(isOwner, period, sidx, sord, !String.IsNullOrEmpty(searchText), searchResultGuids);
            var data = (JqGridData)result.Data;

            var user = RepositoryFactory.GetCurrentUser();


            if (!String.IsNullOrEmpty(searchText))
                data.rows = data.rows.Cast<Document>().Select(d => new
                {
                    Id = d.Id,
                    Header = d.Header,
                    DocumentType = d.DocumentType,
                    DocumentNumber = d.DocumentNumber,
                    CreateDate = d.DocumentViewers[user.ToString()].Date,
                    CreateTime = new { Hours = d.DocumentViewers[user.ToString()].Time.TimeOfDay.Hours, Minutes = d.DocumentViewers[user.ToString()].Time.TimeOfDay.Minutes },
                    Stage = d.CurrentStageCalcualted,
                    Classes = GetDocumentClasses(d),
                    Order = searchResults.Any() ? searchResults.Single(m => m.DocId == d.Id.ToString()).Score : 0
                }).OrderByDescending(m => m.Order);
            else
                data.rows = data.rows.Cast<Document>().Select(d => new
                {
                    Id = d.Id,
                    Header = d.Header,
                    DocumentType = d.DocumentType,
                    DocumentNumber = d.DocumentNumber,
                    CreateDate = d.DocumentViewers[user.ToString()].Date,
                    CreateTime = new { Hours = d.DocumentViewers[user.ToString()].Time.TimeOfDay.Hours, Minutes = d.DocumentViewers[user.ToString()].Time.TimeOfDay.Minutes },
                    Stage = d.CurrentStageCalcualted,
                    Classes = GetDocumentClasses(d),
                    Order = searchResults.Any() ? searchResults.Single(m => m.DocId == d.Id.ToString()).Score : 0
                });



            result.Data = data;
            result.MaxJsonLength = Int32.MaxValue;
            return result;
        }

        [ValidateInput(false)]
        public ActionResult SaveReportToSession(string str)
        {
            Session.Remove("reportExcel");
            Session.Add("reportExcel", str);
            return null;
        }

        public FileContentResult FormReport()
        {
            var fr = new FileContentResult(Encoding.Default.GetBytes(Session["reportExcel"].ToString()), "application/vnd.ms-excel") { FileDownloadName = "Report.xls" };
            Session.Remove("reportExcel");
            return fr;
        }




        public ActionResult GetDocumentsByTypeNew(Guid docType, Guid idToDynamicFieldFilter, int page, int recordsOnPage, string sortColumn,
                                                    string groupColumn, int sortDirection, string owner, string period, string searchPhrase,
                                     DocumentFilterVM documentFilterVM, bool isExcel = false)
        {

            List<FullTextSearch.ServiceFTSFoundDocuments> searchResults = new List<FullTextSearch.ServiceFTSFoundDocuments>();
            List<Guid> searchResultGuids = null;

            if (documentFilterVM == null)
            {
                documentFilterVM = new DocumentFilterVM();
            }


            if (!String.IsNullOrEmpty(searchPhrase))
                using (FullTextSearch.ServiceFTSClient clnt = new FullTextSearch.ServiceFTSClient())
                {
                    searchResultGuids = new List<Guid>();
                    if (RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == RepositoryFactory.GetCurrentUser()).InRole("Канцелярия"))
                    {
                        searchResults = clnt.SearchDocuments(searchPhrase, "", "");
                    }
                    else
                    {
                        searchResults = clnt.SearchDocuments(searchPhrase, RepositoryFactory.GetCurrentUser().ToString(), "");
                    }
                    searchResultGuids = searchResults.Select(m => new Guid(m.DocId)).ToList();
                }

            //var documentType = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == docType);

            // вычисление тотал работает не правильно. Нужно чинить. пока испоьзуем костыль ниже:
            //var total = RepositoryFactory.GetDocumentRepository().GetListForAllDocumentsGridCount(RepositoryFactory.GetCurrentUser(), owner, period, searchResultGuids, docType, idToDynamicFieldFilter, documentFilterVM);

            var totalDocsList = RepositoryFactory.GetDocumentRepository().GetListForAllDocumentsGrid(0, 1000, RepositoryFactory.GetCurrentUser(),
                                                                                            sortColumn, sortDirection, groupColumn, owner, period, searchResultGuids,
                                                                                            docType, idToDynamicFieldFilter,
                                                                                            documentFilterVM, isExcel
                                                                                            );
            var total = 0;
            foreach (var item in totalDocsList)
            {
                total = total + item.Values.Count;
            }

            var list = RepositoryFactory.GetDocumentRepository().GetListForAllDocumentsGrid((page * recordsOnPage), recordsOnPage, RepositoryFactory.GetCurrentUser(),
                                                                                            sortColumn, sortDirection, groupColumn, owner, period, searchResultGuids,
                                                                                            docType, idToDynamicFieldFilter,
                                                                                            documentFilterVM, isExcel
                                                                                            ).ToList();



            return Json(new
            {
                Data = list,
                More = total >= (page * recordsOnPage) + recordsOnPage,
                canViewNumber = CanViewNumber(docType),
                Total = total
            }, JsonRequestBehavior.AllowGet);

        }



        public bool CanViewNumber(Guid docType)
        {
            bool canViewNumber = false;
            if (docType != new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a"))
            {
                canViewNumber = true;
            }

            if (RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == RepositoryFactory.GetCurrentUser()).InRole("Канцелярия")
                              && docType == new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a"))
            {
                canViewNumber = true;
            }

            return canViewNumber;
        }

        //public ActionResult GetDocumentsByType(Guid docType, string searchText, string owner, string period, string sidx, string sord, Guid idToDynamicFieldFilter)
        //{
        //    bool? isOwner = null;
        //    if (owner == "my")
        //        isOwner = true;
        //    if (owner == "alien")
        //        isOwner = false;

        //    var documentType = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == docType);

        //    List<FullTextSearch.ServiceFTSFoundDocuments> searchResults = new List<FullTextSearch.ServiceFTSFoundDocuments>();
        //    List<Guid> searchResultGuids = new List<Guid>();

        //    if (!String.IsNullOrEmpty(searchText))
        //        using (FullTextSearch.ServiceFTSClient clnt = new FullTextSearch.ServiceFTSClient())
        //        {
        //            searchResults = clnt.SearchDocuments(searchText, RepositoryFactory.GetCurrentUser().ToString(), docType.ToString());
        //            searchResultGuids = searchResults.Select(m => new Guid(m.DocId)).ToList();
        //        }


        //    JsonResult result = new JqGridHelper().GetDocumentsByTypeGridResult(docType, isOwner, period, sidx, sord, !String.IsNullOrEmpty(searchText), searchResultGuids, idToDynamicFieldFilter);
        //    var data = (JqGridData)result.Data;

        //    if (!String.IsNullOrEmpty(searchText))
        //        data.rows = data.rows.Cast<Document>().Select(d => new
        //        {
        //            Id = d.Id,
        //            Header = d.Header,
        //            DocumentNumber = d.DocumentNumber,
        //            CreateDate = d.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Date,
        //            CreateTime = new { Hours = d.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Time.TimeOfDay.Hours, Minutes = d.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Time.TimeOfDay.Minutes },
        //            Stage = d.CurrentStageCalcualted,
        //            Classes = GetDocumentClasses(d),
        //            Order = searchResults.Any() ? searchResults.Single(m => m.DocId == d.Id.ToString()).Score : 0,
        //            AddColumn = documentType.FieldForAdditionalMenuFiltering == Guid.Empty ? "" : d.DynamicFiltrationFieldValue,
        //        }).OrderByDescending(m => m.Order);
        //    else
        //        data.rows = data.rows.Cast<Document>().Select(d => new
        //        {
        //            Id = d.Id,
        //            Header = d.Header,
        //            DocumentNumber = d.DocumentNumber,
        //            CreateDate = d.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Date,
        //            CreateTime = new { Hours = d.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Time.TimeOfDay.Hours, Minutes = d.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].Time.TimeOfDay.Minutes },
        //            Stage = d.CurrentStageCalcualted,
        //            Classes = GetDocumentClasses(d),
        //            Order = searchResults.Any() ? searchResults.Single(m => m.DocId == d.Id.ToString()).Score : 0,
        //            AddColumn = documentType.FieldForAdditionalMenuFiltering == Guid.Empty ? "" : d.DynamicFiltrationFieldValue,
        //        });

        //    result.Data = data;
        //    result.MaxJsonLength = Int32.MaxValue;
        //    return result;
        //}



        public ActionResult Tasks()
        {
            return View();
        }

        public ActionResult GetTasksNew(int page, int recordsOnPage, string sortColumn,
                                                    string groupColumn, int sortDirection, string period, string searchPhrase)
        {
            List<FullTextSearch.ServiceFTSFoundDocuments> searchResults = new List<FullTextSearch.ServiceFTSFoundDocuments>();
            List<Guid> searchResultGuids = null;

            if (!String.IsNullOrEmpty(searchPhrase))
                using (FullTextSearch.ServiceFTSClient clnt = new FullTextSearch.ServiceFTSClient())
                {
                    searchResultGuids = new List<Guid>();
                    searchResults = clnt.SearchDocuments(searchPhrase, RepositoryFactory.GetCurrentUser().ToString(), "");
                    searchResultGuids = searchResults.Select(m => new Guid(m.DocId)).ToList();
                }


            var total = RepositoryFactory.GetDocumentRepository().getAllNewTasksCount();
            var data = RepositoryFactory.GetDocumentRepository().GetListTasks((page * recordsOnPage), recordsOnPage, RepositoryFactory.GetCurrentUser(),
                                                                                            sortColumn, sortDirection, groupColumn, period, searchResultGuids);

            return Json(new
            {
                Data = data,
                More = total >= (page * recordsOnPage) + recordsOnPage,
            }, JsonRequestBehavior.AllowGet);

        }


        //public ActionResult Tasks()
        //{
        //    return View();
        //}


        //public ActionResult GetTasks(string sidx, string sord)
        //{
        //    var tasks = RepositoryFactory.GetDocumentRepository().getAllTasks();
        //    string sidx2 = "";
        //    if (sidx.Contains(','))
        //    {
        //        string[] str = sidx.Split(',');
        //        sidx = str[0].Split(' ')[0].Trim();
        //        sidx2 = str[1].Trim();
        //    }

        //    var data = (from d in tasks
        //                select new TaskViewModel
        //    {
        //        Id = d.Id,
        //        CalculatedId = d.Id.ToString(),
        //        Header = d.Header,
        //        Name = d.DocumentSignStages.Single(rs => rs.isCurrent && rs.ControlPerformForRouteStageUserId == null && rs.RouteUsers.Any(rt => rt.IsCurent)).Name +
        //        ((d.DocumentSignStages.Single(rs => rs.isCurrent && rs.ControlPerformForRouteStageUserId == null && rs.RouteUsers.Any(rt => rt.IsCurent)).RouteUsers.Single(ru => ru.IsCurent).Instructions != null) && (d.DocumentSignStages.Single(rs => rs.isCurrent && rs.ControlPerformForRouteStageUserId == null && rs.RouteUsers.Any(rt => rt.IsCurent)).RouteUsers.Single(ru => ru.IsCurent).Instructions.Count() > 0) ? " (поручено)" : ""),
        //        FinishDate = d.FinishDate.ToString("dd.MM.yyyy"),
        //        Number = string.Format("{0} №{1}", d.DocumentType.Name, d.DocumentNumber),
        //        Author = d.Author.GetFIO(),
        //        Group = GetTaskGroup(d),
        //        Classes = GetTasksClasses(d),
        //        hrefAddress = "GetDocument?DocumentId=" + d.Id.ToString()
        //    }).ToList();



        //    var instrTasks = (RepositoryFactory.GetInstructionRepository().getAllTasks());

        //    data.AddRange((from d in instrTasks
        //                   select new TaskViewModel
        //                   {
        //                       Id = d.Id,
        //                       CalculatedId = d.Id.ToString(),
        //                       Header = d.Header,
        //                       Name = d.DocumentSignStages.Single(rs => rs.isCurrent && rs.ControlPerformForRouteStageUserId == null && rs.RouteUsers.Any(rt => rt.IsCurent)).Name +
        //                       ((d.DocumentSignStages.Single(rs => rs.isCurrent && rs.ControlPerformForRouteStageUserId == null && rs.RouteUsers.Any(rt => rt.IsCurent)).RouteUsers.Single(ru => ru.IsCurent).Instructions != null) && (d.DocumentSignStages.Single(rs => rs.isCurrent && rs.ControlPerformForRouteStageUserId == null && rs.RouteUsers.Any(rt => rt.IsCurent)).RouteUsers.Single(ru => ru.IsCurent).Instructions.Count() > 0) ? " (поручено)" : ""),
        //                       FinishDate = d.FinishDate.ToString("dd.MM.yyyy"),
        //                       Number = string.Format("{0} {1}", d.DocumentType.Name, d.DocumentNumber),
        //                       Author = d.Author.GetFIO(),
        //                       Group = GetTaskGroup(d),
        //                       Classes = GetTasksClasses(d),
        //                       hrefAddress = "GetDocument?DocumentId=" + d.RootDocumentId.ToString() + "&Tab=3"
        //                   }).ToList());



        //    tasks = RepositoryFactory.GetDocumentRepository().getAllTasksForConfirmingPerform();



        //    tasks.ForEach(m =>
        //    {
        //        if (m.DocumentSignStages.Exists(n => n.isCurrent == true && n.ControlPerformForRouteStageUserId != null && n.RouteUsers.Exists(x => x.SignUser.UserId == RepositoryFactory.GetCurrentUser())))
        //        {
        //            m.DocumentSignStages.Where(n => n.isCurrent == true && n.ControlPerformForRouteStageUserId != null && n.RouteUsers.Exists(x => x.SignUser.UserId == RepositoryFactory.GetCurrentUser())).ToList().ForEach(k =>
        //            {
        //                if (k.RouteUsers.Exists(kx => kx.SignUser.UserId == RepositoryFactory.GetCurrentUser()))
        //                {
        //                    k.RouteUsers.Where(kx => kx.SignUser.UserId == RepositoryFactory.GetCurrentUser()).ToList().ForEach(ku =>
        //                    {
        //                        data.Add(new TaskViewModel
        //                        {
        //                            Id = ku.Id,
        //                            CalculatedId = m.Id.ToString() + ":" + ku.Id.ToString(),
        //                            Header = m.Header,
        //                            Name = k.Name,
        //                            FinishDate = m.FinishDate.ToString("dd.MM.yyyy"),
        //                            Number = string.Format("{0} №{1}", m.DocumentType.Name, m.DocumentNumber),
        //                            Author = m.DocumentSignStages.Single(m1 => m1.RouteUsers.Any(n1 => n1.Id == k.ControlPerformForRouteStageUserId)).RouteUsers.Single(n1 => n1.Id == k.ControlPerformForRouteStageUserId).SignUser.GetFIO(),
        //                            Group = GetTaskGroup(m),
        //                            Classes = GetTasksClasses(m),
        //                            hrefAddress = "ViewSignResult?DocId=" + m.Id.ToString() + "&routeuserId=" + k.ControlPerformForRouteStageUserId.ToString()
        //                        });
        //                    });
        //                }
        //            });
        //        }
        //    });

        //    instrTasks = RepositoryFactory.GetInstructionRepository().getAllTasksForConfirmingPerform();


        //    instrTasks.ForEach(m =>
        //    {
        //        if (m.DocumentSignStages.Exists(n => n.isCurrent == true && n.ControlPerformForRouteStageUserId != null && n.RouteUsers.Exists(x => x.SignUser.UserId == RepositoryFactory.GetCurrentUser())))
        //        {
        //            m.DocumentSignStages.Where(n => n.isCurrent == true && n.ControlPerformForRouteStageUserId != null && n.RouteUsers.Exists(x => x.SignUser.UserId == RepositoryFactory.GetCurrentUser())).ToList().ForEach(k =>
        //            {
        //                if (k.RouteUsers.Exists(kx => kx.SignUser.UserId == RepositoryFactory.GetCurrentUser()))
        //                {
        //                    k.RouteUsers.Where(kx => kx.SignUser.UserId == RepositoryFactory.GetCurrentUser()).ToList().ForEach(ku =>
        //                    {
        //                        data.Add(new TaskViewModel
        //                        {
        //                            Id = ku.Id,
        //                            CalculatedId = m.Id.ToString() + ":" + ku.Id.ToString(),
        //                            Header = m.Header,
        //                            Name = k.Name,
        //                            FinishDate = m.FinishDate.ToString("dd.MM.yyyy"),
        //                            Number = string.Format("{0} {1}", m.DocumentType.Name, m.DocumentNumber),
        //                            Author = m.UserFor.GetFIO(),
        //                            Group = GetTaskGroup(m),
        //                            Classes = GetTasksClasses(m),
        //                            hrefAddress = "GetDocument?DocumentId=" + m.RootDocumentId.ToString() + "&Tab=3"
        //                            // hrefAddress = "ViewSignResult?DocId=" + m.Id.ToString() + "&routeuserId=" + k.ControlPerformForRouteStageUserId.ToString() + "&isForInstruction=true"
        //                        });
        //                    });
        //                }
        //            });
        //        }
        //    });


        //    if (sord == "desc")
        //        data = data.OrderBy(d => d.FinishDate).ThenByDescending(d2 => d2.GetType().GetProperty(sidx2).GetValue(d2)).ToList();
        //    else
        //        data = data.OrderBy(d => d.FinishDate).ThenBy(d2 => d2.GetType().GetProperty(sidx2).GetValue(d2)).ToList();

        //    var retData = new JqGridData
        //    {
        //        page = 1,
        //        total = 1,
        //        rows = data.AsQueryable(),
        //        records = data.Count()
        //    };
        //    return Json(retData);
        //}

        public ActionResult Instructions()
        {
            return View("Instructions/InstructionsNew");
        }

        public ActionResult GetInstructionsCount()
        {
            return Json(RepositoryFactory.GetInstructionRepository().GetInstructionsCounts(), JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetIstructionsNew(string type, int page, int recordsOnPage, string sortColumn, string groupColumn, int sortDirection, string owner, string period)
        {

            //List<FullTextSearch.ServiceFTSFoundDocuments> searchResults = new List<FullTextSearch.ServiceFTSFoundDocuments>();
            //List<Guid> searchResultGuids = null;

            //if (!String.IsNullOrEmpty(searchPhrase))
            //    using (FullTextSearch.ServiceFTSClient clnt = new FullTextSearch.ServiceFTSClient())
            //    {
            //        searchResultGuids = new List<Guid>();
            //        searchResults = clnt.SearchDocuments(searchPhrase, RepositoryFactory.GetCurrentUser().ToString(), "");
            //        searchResultGuids = searchResults.Select(m => new Guid(m.DocId)).ToList();
            //    }

            var total = RepositoryFactory.GetInstructionRepository().GetInstructionsCount(type);
            var list = RepositoryFactory.GetInstructionRepository().GetInstructionsNew(type, (page * recordsOnPage), recordsOnPage, sortColumn, sortDirection, groupColumn).ToList();

            return Json(new
            {
                Data = list,
                More = total >= (page * recordsOnPage) + recordsOnPage,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InstructionsOld()
        {
            return View("Instructions/Instructions");
        }



        public ActionResult InstructionsView(string type)
        {
            if (type == "all")
                return View("~/Views/Document/Instructions/AllInstructions.cshtml");
            if (type == "outOfDate")
                return View("~/Views/Document/Instructions/OutOfDateInstructions.cshtml");
            if (type == "inComplete")
                return View("~/Views/Document/Instructions/InCompleteInstructions.cshtml");
            if (type == "control")
                return View("~/Views/Document/Instructions/ControlInstructions.cshtml");
            if (type == "completed")
                return View("~/Views/Document/Instructions/CompletedInstructions.cshtml");
            return View("~/Views/Document/Instructions/AllInstructions.cshtml");
        }

        public ActionResult GetInstructions(string type, int page, int rows, string sidx, string sord)
        {
            var ret = new JqGridHelper().GetInstructionsGridResult(type, page, rows, sidx, sord);
            var records = (JqGridData)ret.Data;
            var data = records.rows.Cast<Instruction>().Select(i => new
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
                CompletedDate = type == "control" || type == "completed" ? i.DocumentSignStages[0].RouteUsers[0].SignResult.Date.ToString("dd.MM.yyyy") : "",
                ApproveDate = type == "completed" ? (i.DocumentSignStages.SingleOrDefault(rs => rs.ControlPerformForRouteStageUserId != null).FinishDate != null ? i.DocumentSignStages.SingleOrDefault(rs => rs.ControlPerformForRouteStageUserId != null).FinishDate.Value.ToString("dd.MM.yyyy") : "") : "",
                Classes = "",
            });
            records.rows = data;
            ret.Data = records;
            return ret;
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

        public int GetInstructionOutOfDate(Instruction i)
        {
            return (DateTime.Now.Date - i.FinishDate.Date).Days;
        }

        public string GetOutOfDate(Document d)
        {
            //var finishDate = d.FinishDate;
            //if (finishDate == null)
            //    finishDate = DateTime.Now; // заглушка
            if (d.FinishDate.Date < DateTime.Now.Date)
            {
                return (DateTime.Now.Date - d.FinishDate.Date).Days.ToString();
            }
            return "";
        }

        public int GetTaskGroup(Document d)
        {

            if (d.FinishDate.Date < DateTime.Now.Date)
            {
                return 0;
            }
            if (d.FinishDate.Date == DateTime.Now.Date)
            {
                return 1;
            }
            if (d.FinishDate.Date == DateTime.Now.Date.AddDays(1))
            {
                return 2;
            }
            if (d.FinishDate.Date > DateTime.Now.Date)
            {
                return 3;
            }
            return 3;
        }

        public string GetDocumentClasses(Document d)
        {
            string ret = "";
            if (d.DocumentViewers.Any(dv => dv.Key == RepositoryFactory.GetCurrentUser().ToString()) && d.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].ViewDateTime == null)
            {
                ret += " notViewed";
            }
            return ret;
        }

        public string GetTasksClasses(Document d)
        {
            string ret = "";
            if (d.FinishDate.Date < DateTime.Now.Date)
            {
                ret += " error";
            }
            if (d.DocumentViewers.Any(dv => dv.Key == RepositoryFactory.GetCurrentUser().ToString()) && d.DocumentViewers[RepositoryFactory.GetCurrentUser().ToString()].ViewDateTime == null)
            {
                ret += " notViewed";
            }
            return ret;
        }

        public ActionResult Document()
        {
            return View();
        }

        public ActionResult GetDataForDocument()
        {
            var tmp = RepositoryFactory.GetRepository<DocumentType>().List(m => m.isDeleted == false).ToList();
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddDocument(Guid Id, Guid? rootDocumentId, Guid? rootInstructionId, Guid? userForRouteId)
        {
            ViewBag.isForUpdate = false;

            var tmpDynamicTemplate = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == Id);

            ViewBag.DocumentType = tmpDynamicTemplate.Name;

            DocumentViewModel doc = new DocumentViewModel();

            doc.VisualFieldsTemplate = tmpDynamicTemplate.DocumentVisualTemplate;
            doc.AutoShiftDays = tmpDynamicTemplate.AutoShiftDate.HasValue ? tmpDynamicTemplate.AutoShiftDate.Value : 0;
            doc.FinishDate = DateTime.Now.AddDays(doc.AutoShiftDays);

            doc.DocumenType = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == Id);
            doc.DocumentTypeId = Id;
            // drVm.ReferenceId = tmpDynamicTemplate.Id;



            doc.Version = 1;


            doc.Fields = tmpDynamicTemplate.FieldTemplates.Select(m => new DynamicRecordFieldViewModel()
            {
                DynamicFieldTemplateId = m.Id,
                Header = m.Header,
                isRequired = m.isRequired,
                TypeOfTheFieldId = m.FieldType.Id,
                Value = m.FieldType.Id == new Guid("2490becb-3476-43ab-8717-0f0b138a6ab2") ? "False" : "",
                DynamicReferenceId = m.FieldType.DynamicReferenceId,


            }).ToList();

            if (Id == new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a"))
            {


                //var res =
                //RepositoryFactory.GetDocumentRepository()
                //    .List(
                //        x =>
                //            !x.isDeleted &&
                //            (x.DocumentType.Id == new Guid("e993583d-2ef8-4368-9ed5-5f4439374174") ||
                //             x.DocumentType.Id == new Guid("e994583d-2ef8-4368-9ed5-5f4439374197")))
                //    .Select(s => new
                //    {
                //        Value = s.DocumentNumber,
                //        Text = "№ " + s.DocumentNumber + " " + "Заголовок: " + s.Header
                //    }).ToList();

                //res.Add(new {Value = "", Text = ""});

                //ViewData["inputDocNumbers"] = 
            }


            if (rootDocumentId.HasValue)
            {
                ViewBag.isForRoot = true;
                var tmpDoc = RepositoryFactory.GetDocumentRepository().Single(m => m.Id == rootDocumentId.Value);

                ViewBag.rootDocumentNumber = "Входящий документ №" + tmpDoc.DocumentNumber;
                ViewBag.rootInstructionId = rootInstructionId;
                ViewBag.rootDocumentId = rootDocumentId;
                ViewBag.userForRouteId = userForRouteId;
            }
            else
            {
                ViewBag.isForRoot = false;
            }




            return View(doc);


        }


        [HttpGet]
        public JsonResult GetInboxDocNumbersForSelect2(string term, int page, int pageLimit)
        {
            var res = RepositoryFactory.GetDocumentRepository().GetInboxDocNumbersForSelect2(term, page, pageLimit).Select(x => new
            {
                id = x.Id,
                text = "№ " + x.DocumentNumber + " " + x.Header
            }).ToList();

            return Json(new
            {
                rows = res,
                total = RepositoryFactory.GetDocumentRepository().CountOfDocuments(term)
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult AddDocument(DocumentViewModel model, int isForTemplate = 0, string templateName = "")
        {
            #region Проверка на дублирование входящего номера

            var incomingNumber = model.Fields
                .FirstOrDefault(x => x.DynamicFieldTemplateId == Guid.Parse("587033ff-6574-4241-9c94-775a9cadb029"))
                ?.Value;
            if (!string.IsNullOrWhiteSpace(incomingNumber) && incomingNumber!="б/н")
            {
                var foundDocs = RepositoryFactory.GetDocumentRepository().GetDocsByIncomingNumber(incomingNumber);
                if (foundDocs.Count > 0)
                {
                    var lastDoc = foundDocs.OrderByDescending(x => x.CreateDate).FirstOrDefault();
                    var errorMessage = "";
                    if (foundDocs.Count > 1)
                    {
                        errorMessage = $"Входящий документ по номеру исходящего документа №{incomingNumber} уже зарегистрирован в системе {foundDocs.Count} раз(а). Последний раз был зарегистрирован пользователем {lastDoc?.Author.FirstName} {lastDoc?.Author.LastName} {lastDoc?.CreateDate:dd.MM.yyyy}";
                    }
                    else
                    {
                        errorMessage = $"Входящий документ по номеру исходящего документа №{incomingNumber} уже зарегистрирован в системе пользователем {lastDoc?.Author.FirstName} {lastDoc?.Author.LastName} {lastDoc?.CreateDate:dd.MM.yyyy} ";
                    }
                    for (var i = 0; i < model.Fields.Count; i++)
                    {
                        if (model.Fields[i].DynamicFieldTemplateId == Guid.Parse("587033ff-6574-4241-9c94-775a9cadb029"))
                        {
                            ModelState.AddModelError($"Fields[{i}].Value", errorMessage);
                        }
                    }
                }
            }

            #endregion

            ViewBag.isForUpdate = false;

            model.DocumenType = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == model.DocumentTypeId);

            for (int i = 0; i < model.Fields.Count(); i++)
            {
                //Проверка на Required
                if (model.Fields[i].TypeOfTheFieldId == BL.DocumentRouting.DocumentRouting.settings.ListUsersFieldType)
                {
                    if (model.Fields[i].ValueUsersKo.Any(m => m.UserGuid == Guid.Empty))
                        ModelState.AddModelError(String.Format("Fields[{0}].Value", i), "Поле должно содержать значение");
                }
                else
                {
                    if (model.Fields[i].ValueUsersKo.Any() && model.Fields[i].isRequired)
                        ModelState.AddModelError(String.Format("Fields[{0}].Value", i), "Необходимо заполнить поле");
                }



                if (model.NegotiatorsStage.Any())
                {
                    if (model.NegotiatorsStage.Any(m => m.UsersForNegotiationStage.Any(n => n.userId == Guid.Empty)))
                    {
                        ModelState.AddModelError(String.Format("NegotiatorsStage", i), "Поле должно содержать значение");
                    }
                    if ((model.NegotiatorsStage.Any(m => m.UsersForNegotiationStage.Count() < 1)))
                        ModelState.AddModelError(String.Format("NegotiatorsStage", i), "Этап согласования не может быть пустым");
                }


                if (model.Fields[i].TypeOfTheFieldId.ToString() != "e3224442-d53a-47e9-b1bb-495c034b10d8")
                    if (model.Fields[i].TypeOfTheFieldId != Devir.DMS.BL.DocumentRouting.DocumentRouting.settings.ListUsersFieldType)
                        if ((String.IsNullOrEmpty(model.Fields[i].Value) && model.Fields[i].isRequired) || (model.Fields[i].isRequired && model.Fields[i].Value == Guid.Empty.ToString()))
                        {
                            ModelState.AddModelError(String.Format("Fields[{0}].Value", i), "Необходимо заполнить поле");
                        }
                        else
                        {
                            ////Проверка на тип данных
                            //if (!BL.DynamicRecords.DataTypeHelper.CheckFieldForDataType(model.ReferenceId, model.Fields[i].DynamicFieldTemplateId, model.Fields[i].Value))
                            //{
                            //    ModelState.AddModelError(String.Format("Fields[{0}].Value", i), "Значение поля не соответсвует формату");
                            //}
                        }

            }

            // если входящий или исходящий 
            if (model.DocumenType.Id == new Guid("e993583d-2ef8-4368-9ed5-5f4439374174") || model.DocumenType.Id == new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a"))
            {
                if (model.attachment.Count == 0)
                    ModelState.AddModelError("attachment", "Необходимо прикрепить вложения");
            }


            if (ModelState.IsValid && isForTemplate == 0)
            {

                Document doc = new Document();
                doc.Version = model.Version;
                doc.Id = Guid.NewGuid();
                doc.docState = DocumentState.Draft;
                if (!model.isNewVersion)
                {
                }
                else
                    doc.DocumentNumber = model.DocumentNumber;

                if (model.isNewVersion)
                    doc.ParentDocumentId = model.ParentDoc;

                doc.isUrgent = model.isUrgent;

                doc.Header = model.Header;
                doc.Body = model.Body;

                DateTime? preDateTime = null;
                if (model.instructions.Count > 0)
                    preDateTime = model.instructions.Max(m => DateTime.Parse(m.DateBefore));

                if (preDateTime.HasValue)
                    doc.FinishDate = preDateTime.Value;
                else
                    doc.FinishDate = model.FinishDate < DateTime.Now.AddDays(-1) ? DateTime.Now.AddDays(model.AutoShiftDays) : model.FinishDate;

                doc.DocumentType = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == model.DocumentTypeId);
                doc.Attachments = model.attachment;

                //В документе имеются согласующие
                //var tmpField = model. Fields.Where(m => m.TypeOfTheFieldId.ToString() == "e3224442-d53a-47e9-b1bb-495c034b10d8").FirstOrDefault();
                if (model.NegotiatorsStage != null && model.NegotiatorsStage.Count() > 0)
                {
                    //Сохраняем их в модель БД
                    var Negotiators = new List<DL.Models.Document.NegotiatorsRoutes.NegotiationStage>();
                    model.NegotiatorsStage.ForEach(m =>
                    {

                        var tmpUserList = new List<UserForNegotiation>();

                        m.UsersForNegotiationStage.ForEach(n =>
                        {
                            tmpUserList.Add(new UserForNegotiation()
                            {
                                UserId = n.userId,
                                Order = n.order,
                                IsMustSign = n.isMust,
                                DateBeforeSign = !String.IsNullOrEmpty(n.signBefore) ? (DateTime?)DateTime.Parse(n.signBefore) : null
                            });
                        });

                        Negotiators.Add(new DL.Models.Document.NegotiatorsRoutes.NegotiationStage()
                        {
                            Order = m.order,
                            StageType = m.StageType == "Параллельное" ? DL.Models.Document.NegotiatorsRoutes.NegotiationStageTypes.Parallel : DL.Models.Document.NegotiatorsRoutes.NegotiationStageTypes.Sequenced,
                            UsersForNegotiation = tmpUserList
                        });
                    });
                    doc.Negotiators = Negotiators;
                }


                //Сохраняем все остальные Динамические поля в модель БД
                var records = new List<DocumentFieldValues>();
                model.Fields.ForEach(m =>
                {


                    var tmpUserForRouteListValue = new List<UsersForRoute>();
                    m.ValueUsersKo.ForEach(l => tmpUserForRouteListValue.Add(new UsersForRoute()
                    {
                        Order = l.Order,
                        UserId = l.UserGuid
                    }));

                    records.Add(new DocumentFieldValues()
                    {
                        StringValue = m.Value,
                        Id = Guid.NewGuid(),
                        FieldTypeId = m.TypeOfTheFieldId,
                        DynamicReferenceId = m.DynamicReferenceId,
                        DynamicReferenceFieldTemplateId = m.DynamicFieldTemplateId,
                        UserForRouteListValue = tmpUserForRouteListValue,
                        GuidValue = m.ValueId,
                        FieldTemplateId = m.DynamicFieldTemplateId,
                        Header = m.Header
                    });
                });

                records.ForEach(m => BL.DynamicRecords.DataTypeHelper.AddDynamicFieldValue(m, m.FieldTypeId));
                doc.FieldValues = records;

                List<WebNotifications> webNotifications = new List<WebNotifications>();
                var notifications = DocumentRouting.CreateRouteStagesForDocument(doc, RepositoryFactory.GetRepository<User>().Single(m => m.UserId == RepositoryFactory.GetCurrentUser()), webNotifications);

                webNotifications.ForEach(m => { SignalRWebNotifierHelper.SendToRefreshMainMenu(m.message, m.userName); });

                notifications.ForEach(m =>
                {
                    BL.SMTP.SMTPHelper.sendMessage(m);
                    RepositoryFactory.GetRepository<Notifications>().Insert(m);
                    SignalRWebNotifierHelper.SendNotifyToClient(m.ForWho.Name, m.Id);
                });


                if (doc.DocumentType.FiledTypeForNumbering != Guid.Empty)
                {
                    var tmpFieldName = doc.FieldValues.Single(m => m.FieldTemplateId == doc.DocumentType.FiledTypeForNumbering).ValueToDisplay;
                    doc.DocumentType.Name = doc.DocumentType.Name + " " + tmpFieldName;
                }


                if (doc.DocumentType.FieldForAdditionalMenuFiltering != Guid.Empty)
                {
                    doc.DynamicFiltrationFieldGuid = doc.FieldValues.Where(m => m.FieldTemplateId == doc.DocumentType.FieldForAdditionalMenuFiltering).First().DynamicRecordId;
                    doc.DynamicFiltrationFieldValue = doc.FieldValues.Where(m => m.FieldTemplateId == doc.DocumentType.FieldForAdditionalMenuFiltering).First().ValueToDisplay;
                }

                if (model.ForRootDocumentId.HasValue)
                {
                    doc.ForRootDocumentId = model.ForRootDocumentId;
                    doc.ForRootInstructionId = model.ForRootInstructionId;


                }





                //Сохраняем модель в БД :)
                RepositoryFactory.GetRepository<Document>().Insert(doc);

                model.DocumentNumber = doc.DocumentNumber;
                model.ParentDoc = doc.Id;

                if (!model.isNewVersion)
                    model.RealDocNumber = doc.DocumentNumber;


                RepositoryFactory.GetRepository<DocumentViewModelDB>().Insert(new DocumentViewModelDB() { docId = doc.Id, ViewModel = model });

                try
                {
                    SaveDocumentForFTS(doc.Id);
                }
                catch (Exception ex)
                {
                }



                return RedirectToAction("GetDocument", new { DocumentId = doc.Id, isModal = false });
                //return RedirectToAction("GetDocument", new { DocumentId = doc.Id });
            }
            else
            {



                if (isForTemplate == 1)
                {
                    RepositoryFactory.GetRepository<DocumentTemplate>().Insert(new DocumentTemplate()
                    {
                        ViewModel = model,
                        UserId = RepositoryFactory.GetRepository<User>().Single(m => m.UserId == RepositoryFactory.GetCurrentUser()).InRole("Шаблонизатор") ? RepositoryFactory.GetCurrentUser() : Guid.Empty,
                        Name = templateName
                    });
                }





                var tmpField = model.Fields.Where(m => m.TypeOfTheFieldId.ToString() == "e3224442-d53a-47e9-b1bb-495c034b10d8").FirstOrDefault();
                if (tmpField != null)
                    tmpField.ModelHelper = model.NegotiatorsStage;


                var tmpDynamicTemplate = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == model.DocumentTypeId);


                ViewBag.DocumentType = tmpDynamicTemplate.Name;
                model.VisualFieldsTemplate = tmpDynamicTemplate.DocumentVisualTemplate;



                if (model.ForRootDocumentId.HasValue)
                {
                    ViewBag.isForRoot = true;
                    var tmpDoc = RepositoryFactory.GetDocumentRepository().Single(m => m.Id == model.ForRootDocumentId.Value);

                    ViewBag.rootDocumentNumber = "Входящий документ №" + tmpDoc.DocumentNumber;
                    ViewBag.rootInstructionId = model.ForRootInstructionId;
                    ViewBag.rootDocumentId = model.ForRootDocumentId;
                    ViewBag.userForRouteId = model.ForUserForRouteId;
                }
                else
                {
                    ViewBag.isForRoot = false;
                }

                //if (model.DocumenType.Id == new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a"))
                //{

                //    var res =
                //        RepositoryFactory.GetDocumentRepository()
                //            .List(
                //                x =>
                //                    !x.isDeleted &&
                //                    (x.DocumentType.Id == new Guid("e993583d-2ef8-4368-9ed5-5f4439374174") ||
                //                     x.DocumentType.Id == new Guid("e994583d-2ef8-4368-9ed5-5f4439374197")))
                //            .Select(s => new
                //            {
                //                Value = s.DocumentNumber,
                //                Text = "№ " + s.DocumentNumber + " " + "Заголовок: " + s.Header
                //            }).ToList();

                //    res.Add(new { Value = "", Text = "" });

                //    ViewData["inputDocNumbers"] = res;
                //}


                return View(model);
            }

        }


        public ActionResult AddDocumentByTemplate(Guid TemplateId)
        {
            var model = RepositoryFactory.GetRepository<DocumentTemplate>().Single(m => m.Id == TemplateId).ViewModel;

            ViewBag.isForUpdate = false;

            var tmpDynamicTemplate = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == model.DocumentTypeId);



            model.VisualFieldsTemplate = tmpDynamicTemplate.DocumentVisualTemplate;
            model.AutoShiftDays = tmpDynamicTemplate.AutoShiftDate.HasValue ? tmpDynamicTemplate.AutoShiftDate.Value : 0;
            model.FinishDate = DateTime.Now.AddDays(model.AutoShiftDays);


            ViewBag.isForRoot = false;

            ViewBag.DocumentType = tmpDynamicTemplate.Name;


            return View("AddDocument", model);
        }

        public ActionResult AddDocumentNewVersion(Guid DocumentId)
        {
            var model = RepositoryFactory.GetRepository<DocumentViewModelDB>().Single(m => m.docId == DocumentId).ViewModel;

            model.Version++;
            ViewBag.isForUpdate = false;

            var tmpDynamicTemplate = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == model.DocumentTypeId);

            model.DocumentNumber = model.RealDocNumber + " Версия " + model.Version;

            if (model.ForRootDocumentId.HasValue)
            {
                ViewBag.isForRoot = true;
                var tmpDoc = RepositoryFactory.GetDocumentRepository().Single(m => m.Id == model.ForRootDocumentId.Value);

                ViewBag.rootDocumentNumber = "Входящий документ №" + tmpDoc.DocumentNumber;
                ViewBag.rootInstructionId = model.ForRootInstructionId;
                ViewBag.rootDocumentId = model.ForRootDocumentId;
                ViewBag.userForRouteId = model.ForUserForRouteId;
            }
            else
            {
                ViewBag.isForRoot = false;
            }


            //RepositoryFactory.GetRepository<Document>().Single(m => m.ParentDocumentId == model.ParentDoc);


            model.isNewVersion = true;
            model.VisualFieldsTemplate = tmpDynamicTemplate.DocumentVisualTemplate;
            model.AutoShiftDays = tmpDynamicTemplate.AutoShiftDate.HasValue ? tmpDynamicTemplate.AutoShiftDate.Value : 0;
            model.FinishDate = DateTime.Now.AddDays(model.AutoShiftDays);

            ViewBag.DocumentType = tmpDynamicTemplate.Name;


            return View("AddDocument", model);
        }




        public ActionResult GetDocument(Guid DocumentId, int Tab = 1, Guid? InstructionRedirectedId = null, bool isModal = false)
        {

            ViewBag.ActiveTabPane = Tab;
            ViewBag.isCancelyaria = RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == RepositoryFactory.GetCurrentUser()).InRole("Канцелярия");

            RepositoryFactory.GetNotificationRepository().List(m => m.DocumentId == DocumentId && m.ForWho.UserId == RepositoryFactory.GetCurrentUser()).ToList().ForEach(m =>
            {
                m.ViewDateTime = DateTime.Now;
                RepositoryFactory.GetNotificationRepository().update(m);
                SignalRWebNotifierHelper.UpdateNotifyAtClient(m.ForWho.Name, m.Id);
            }
                );




            var doc = RepositoryFactory.GetRepository<Document>().Single(m => m.Id == DocumentId);
            if (doc == null)
            {
                return View("DocumentNotFound");
            }
            else
            {
                if (!RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == RepositoryFactory.GetCurrentUser()).InRole("Канцелярия"))
                    if (!doc.DocumentViewers.ContainsKey(RepositoryFactory.GetCurrentUser().ToString()))
                    {
                        return View("DocumentNotFound");
                    }
            }


            if (doc.ForRootDocumentId.HasValue)
            {
                ViewBag.isForRoot = true;
                var tmpDoc = RepositoryFactory.GetDocumentRepository().Single(m => m.Id == doc.ForRootDocumentId.Value);

                ViewBag.rootDocumentNumber = "Входящий документ №" + tmpDoc.DocumentNumber;
                ViewBag.rootInstructionId = doc.ForRootInstructionId;
                ViewBag.rootDocumentId = doc.ForRootDocumentId;
                ViewBag.userForRouteId = doc.ForUserForRouteId;
            }

            else
            {
                ViewBag.isForRoot = false;
            }

            doc.FieldValues.ForEach(m =>
            {
                m.Header = doc.DocumentType.FieldTemplates.SingleOrDefault(n => n.Id == m.FieldTemplateId).Header;
            });

            if (doc.DocumentSignStages.Exists(m => m.isCurrent))
            {
                //Если есть этапы на подтверждение исполнения
                if (doc.DocumentSignStages.Exists(m => m.ControlPerformForRouteStageUserId != null && m.isCurrent && m.RouteUsers.Exists(x => x.SignUser.UserId == RepositoryFactory.GetCurrentUser())))
                {
                    var tmpUserForConfirm = doc.DocumentSignStages.FirstOrDefault(m => m.ControlPerformForRouteStageUserId != null && m.isCurrent && m.RouteUsers.Exists(x => x.SignUser.UserId == RepositoryFactory.GetCurrentUser())).RouteUsers.FirstOrDefault(m => m.SignUser.UserId == RepositoryFactory.GetCurrentUser());
                    if (tmpUserForConfirm != null)
                    {
                        ViewBag.UserActions = tmpUserForConfirm.UsersActions;
                    }
                    else
                    {
                        ViewBag.UserActions = new List<RouteAction>();
                    }
                }
                else
                {
                    var tmpRoute = doc.DocumentSignStages.FirstOrDefault(m => m.isCurrent && m.ControlPerformForRouteStageUserId == null);
                    if (tmpRoute != null)
                    {
                        var tmpUser = tmpRoute.RouteUsers.FirstOrDefault(n => n.SignUser.UserId == RepositoryFactory.GetCurrentUser() && n.IsCurent == true);
                        if (tmpUser != null)
                        {
                            ViewBag.UserActions = tmpUser.UsersActions;
                        }
                        else
                        {
                            ViewBag.UserActions = new List<RouteAction>();
                        }
                    }
                }
            }
            else { ViewBag.UserActions = new List<RouteAction>(); }

            var tmpViewModel = RepositoryFactory.GetRepository<DocumentViewModelDB>().Single(m => m.docId == doc.Id);

            var tmpInstructions = new List<Devir.DMS.Web.Models.InstructionsKO.InstructionKO>();
            if (tmpViewModel != null)
                tmpInstructions = tmpViewModel.ViewModel.instructions;

            if (doc.DocumentType.StageAfterSendInstructions != Guid.Empty)
                if (!doc.DocumentSignStages.Any(m => m.RouteUsers.Any(n => n.Instructions != null && n.Instructions.Any())))
                    if (tmpInstructions == null)
                        tmpInstructions = new List<Models.InstructionsKO.InstructionKO>();
            //if (tmpInstructions != null)
            //    if (!tmpInstructions.Any())
            //        tmpInstructions = null;
            ViewBag.instructions = tmpInstructions;

            RepositoryFactory.GetDocumentRepository().SetDocumentViewDateByViewer(RepositoryFactory.GetCurrentUser().ToString(), doc.Id);

            //doc.FieldValues.RemoveAll(m => doc.DocumentType.RouteTemplates.Select(n => n.DocumentFieldTemplate).Contains(m.FieldTemplateId));

            ViewBag.isModal = isModal;

            //if (isModal)
            //    return View("GetDocumentLight", doc);
            //else 
            return View("GetDocument", doc);
            //
        }

        public ActionResult GetInstruction(Guid InstructionId)
        {


            var doc = RepositoryFactory.GetRepository<Instruction>().Single(m => m.Id == InstructionId);

            return GetDocument(doc.RootDocumentId, 3, doc.Id, false);

            if (doc == null)
            {
                return View("DocumentNotFound");
            }
            else
            {
                if (!doc.DocumentViewers.ContainsKey(RepositoryFactory.GetCurrentUser().ToString()))
                {
                    return View("DocumentNotFound");
                }
            }



            if (doc.DocumentSignStages.Exists(m => m.isCurrent))
            {
                //Если есть этапы на подтверждение исполнения
                if (doc.DocumentSignStages.Exists(m => m.ControlPerformForRouteStageUserId != null && m.isCurrent && m.RouteUsers.Exists(x => x.SignUser.UserId == RepositoryFactory.GetCurrentUser())))
                {
                    var tmpUserForConfirm = doc.DocumentSignStages.FirstOrDefault(m => m.ControlPerformForRouteStageUserId != null && m.isCurrent && m.RouteUsers.Exists(x => x.SignUser.UserId == RepositoryFactory.GetCurrentUser())).RouteUsers.FirstOrDefault(m => m.SignUser.UserId == RepositoryFactory.GetCurrentUser());
                    if (tmpUserForConfirm != null)
                    {
                        ViewBag.UserActions = tmpUserForConfirm.UsersActions;
                    }
                    else
                    {
                        ViewBag.UserActions = new List<RouteAction>();
                    }
                }
                else
                {
                    var tmpRoute = doc.DocumentSignStages.FirstOrDefault(m => m.isCurrent && m.ControlPerformForRouteStageUserId == null);
                    if (tmpRoute != null)
                    {
                        var tmpUser = tmpRoute.RouteUsers.FirstOrDefault(n => n.SignUser.UserId == RepositoryFactory.GetCurrentUser() && n.IsCurent == true);
                        if (tmpUser != null)
                        {
                            ViewBag.UserActions = tmpUser.UsersActions;
                        }
                        else
                        {
                            ViewBag.UserActions = new List<RouteAction>();
                        }
                    }
                }
            }
            else { ViewBag.UserActions = new List<RouteAction>(); }

            //RepositoryFactory.GetDocumentRepository().SetDocumentViewDateByViewer(RepositoryFactory.GetCurrentUser().ToString(), doc.Id);


            RepositoryFactory.GetInstructionRepository().SetDocumentViewDateByViewer(RepositoryFactory.GetCurrentUser().ToString(), doc.Id);

            if (doc.FieldValues != null)
                doc.FieldValues.RemoveAll(m => doc.DocumentType.RouteTemplates.Select(n => n.DocumentFieldTemplate).Contains(m.FieldTemplateId));

            return View(doc);
        }


        public ActionResult AddSignResult(Guid DocumentId, Guid ActionId, Guid? UserForRouteId, bool isForPerformControl = false, bool isForInstruction = false)
        {
            ViewBag.UserForRoute = UserForRouteId;
            ViewBag.isForPerformControl = isForPerformControl;
            ViewBag.isForInstruction = isForInstruction;

            Document doc = null;
            if (!isForInstruction)
                doc = RepositoryFactory.GetRepository<Document>().Single(m => m.Id == DocumentId);
            else
                doc = RepositoryFactory.GetRepository<Instruction>().Single(m => m.Id == DocumentId);


            if (doc != null)
            {
                ViewBag.DocumentId = DocumentId;

                if (isForInstruction && ActionId == new Guid("b25f05fc-c36b-4663-b2d1-3ab7b42ce04b"))// && Action)
                {
                    var nowIshod = RepositoryFactory.GetDocumentRepository().Single(m => m.ForRootInstructionId == doc.Id && m.docState == DocumentState.InWork);
                    if (nowIshod != null)
                    {
                        ViewBag.nowIshod = true;
                        ViewBag.nowIshodNumber = "Исполняется Исходящий документ №" + (String.IsNullOrEmpty(nowIshod.DocumentNumber) ? " Нет номера" : nowIshod.DocumentNumber);
                        ViewBag.nowIshodNumberLink = String.Format(
                            "/Document/GetDocument?DocumentId={0}&isModal=False",
                            nowIshod.Id);

                    }
                    else
                    {
                        ViewBag.nowIshod = false;
                    }

                    var someDocType =
                        RepositoryFactory.GetDocumentRepository()
                            .Single(m => m.Id == (doc as Instruction).RootDocumentId);


                    if (
                        someDocType.DocumentType.Id == new Guid("e993583d-2ef8-4368-9ed5-5f4439374174") || someDocType.DocumentType.Id == new Guid("E994583D-2EF8-4368-9ED5-5F4439374197"))
                    {

                        if (isForInstruction) ViewBag.RootDocumentId = ((Instruction)doc).RootDocumentId;
                        ViewBag.isIshod = true;
                    }
                    else
                    {
                        ViewBag.isIshod = false;
                    }
                }
                else
                {
                    ViewBag.nowIshod = false;
                    ViewBag.isIshod = false;
                }


                return View(new UserSignResult() { ActionId = ActionId, Action = RepositoryFactory.GetRepository<RouteAction>().Single(m => m.Id == ActionId), Date = DateTime.Now });
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        public ActionResult AddSignResult(Guid DocumentId, UserSignResult model, Guid? UserForRouteId, bool isForPerformControl = false, bool isForInstruction = false, Guid? userForAutoClosing = null)
        {

            bool fireAddViewverRecursively = false;
            List<Guid> tmpInstructions = new List<Guid>();

            model.Action = RepositoryFactory.GetRepository<RouteAction>().Single(m => m.Id == model.ActionId);
            ViewBag.DocumentId = DocumentId;

            if (ModelState.IsValid)
            {
                Document doc = null;
                if (!isForInstruction)
                    doc = RepositoryFactory.GetRepository<Document>().Single(m => m.Id == DocumentId);
                else
                    doc = RepositoryFactory.GetRepository<Instruction>().Single(m => m.Id == DocumentId);

                List<WebNotifications> notify = new List<WebNotifications>();


                if (doc.TempInstructionStorage != null)
                    if (doc.TempInstructionStorage.Any())
                        fireAddViewverRecursively = true;

                if (fireAddViewverRecursively)
                    tmpInstructions = doc.TempInstructionStorage.Select(m => m.Id).ToList();

                if (doc != null)
                {
                    if (!isForPerformControl)
                    {
                        BL.DocumentRouting.DocumentRouting.MoveNext(doc, RepositoryFactory.GetRepository<User>().Single(n => n.UserId == (userForAutoClosing ?? RepositoryFactory.GetCurrentUser())), model, notify).ForEach(m =>
                        {
                            m.Id = Guid.NewGuid();
                            RepositoryFactory.GetRepository<Notifications>().Insert(m);
                            SignalRWebNotifierHelper.SendNotifyToClient(m.ForWho.Name.ToLower(), m.Id);
                        });
                    }
                    else
                    {
                        BL.DocumentRouting.DocumentRouting.SetControPerformResult(UserForRouteId.Value, doc, RepositoryFactory.GetRepository<User>().Single(n => n.UserId == RepositoryFactory.GetCurrentUser()), model, notify).ForEach(m =>
                        {
                            m.Id = Guid.NewGuid();
                            RepositoryFactory.GetRepository<Notifications>().Insert(m);
                            SignalRWebNotifierHelper.SendNotifyToClient(m.ForWho.Name.ToLower(), m.Id);
                        });
                    }


                    if (doc.ForRootDocumentId.HasValue)
                    {

                        var tmpInstruction =
                            RepositoryFactory.GetInstructionRepository()
                                .Single(m => m.Id == doc.ForRootInstructionId.Value);


                        if (doc.DocumentSignStages.All(m => m.FinishDate.HasValue))
                        {
                            AddSignResult(doc.ForRootInstructionId.Value, new UserSignResult()
                            {
                                ActionId =
                                    Devir.DMS.BL.DocumentRouting.DocumentRouting.settings.OkPerformAction,
                                Action = RepositoryFactory.GetRepository<RouteAction>().Single(m => m.Id == Devir.DMS.BL.DocumentRouting.DocumentRouting.settings.OkPerformAction),
                                Date = DateTime.Now,
                                Comment = "Закрыто исходящим документом №" + doc.DocumentNumber
                            }, doc.ForUserForRouteId, false, true, tmpInstruction.UserFor.UserId);
                        }
                    }

                    if (!isForInstruction)
                        RepositoryFactory.GetRepository<Document>().update(doc);
                    else
                        RepositoryFactory.GetRepository<Instruction>().update((doc as Instruction));

                    if (fireAddViewverRecursively)
                    {
                        tmpInstructions.ForEach(inst =>
                        {
                            List<WebNotifications> webNotifications = new List<WebNotifications>();
                            var tmpInstr = RepositoryFactory.GetRepository<Instruction>().Single(m => m.Id == inst);
                            DocumentRouting.AddToViewersRecursivelyForInstruction(tmpInstr, tmpInstr.UserFor, webNotifications);
                            webNotifications.ForEach(m =>
                            {
                                SignalRWebNotifierHelper.SendToRefreshMainMenu(m.message, m.userName);
                            });
                        });
                    }

                    notify.ForEach(m =>
                    {
                        SignalRWebNotifierHelper.SendToRefreshMainMenu(m.message, m.userName);
                    });
                }

                if (!isForInstruction)
                    SaveDocumentForFTS(doc.Id);

                if (!isForInstruction)
                    return JavaScript("$('#ModalSignResult').modal('hide'); $('#ModalSignResul').empty(); refreshCurrentView();");
                else
                    return JavaScript("$('#ModalSignResult').modal('hide'); $('#ModalSignResul').empty(); refreshCurrentView();");

            }
            else
            {
                return View(model);
            }
        }


        public ActionResult AddInstructionForDocument(Guid DocumentId, Guid RouteStageId, Guid RootDocumentId, Guid RouteStageUserId, bool isForInstruction = false)
        {
            InstructionViewModel model = new InstructionViewModel();
            model.DocumentId = DocumentId;

            var doc = RepositoryFactory.GetDocumentRepository().Single(m => m.Id == RootDocumentId);

            if (doc.TempInstructionStorage.Any())
                model.maxDate = doc.TempInstructionStorage.Max(m => m.FinishDate);
            else
                model.maxDate = doc.FinishDate;

            model.DateBefore = model.maxDate;
            model.RouteStageId = RouteStageId;
            model.RouteStageUserId = RouteStageUserId;
            model.RootDocumentId = RootDocumentId;

            ViewBag.isForInstruction = isForInstruction;

            return View(model);
        }

        [HttpPost]
        public ActionResult AddInstructionForDocument(InstructionViewModel model, bool isForInstruction = false)
        {

            var mainDoc = RepositoryFactory.GetDocumentRepository().Single(m => m.Id == model.RootDocumentId);

            if (mainDoc.TempInstructionStorage.Any())
                model.maxDate = mainDoc.TempInstructionStorage.Max(m => m.FinishDate);
            else
                model.maxDate = mainDoc.FinishDate;

            ViewBag.isForInstruction = isForInstruction;
            if (model.DateBefore > model.maxDate)
            {
                ModelState.AddModelError("DateBefore", "Дата превышает дату документа. Пожалуйста укажите другую дату.");
            }



            if (ModelState.IsValid)
            {

                Instruction doc = new Instruction();
                doc.DocumentType = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Id == new Guid("8d15c85a-703e-44da-b040-94ed045c4781"));
                doc.Id = Guid.NewGuid();
                doc.Header = model.Header;
                doc.Attachments = model.attachment;
                doc.Body = model.Body;
                doc.FinishDate = model.DateBefore;
                doc.UserFor = RepositoryFactory.GetRepository<User>().Single(m => m.UserId == model.UserForWho);
                doc.Author = RepositoryFactory.GetRepository<User>().Single(m => m.UserId == RepositoryFactory.GetCurrentUser());
                doc.CreateDate = DateTime.Now;
                doc.CreatorGuid = RepositoryFactory.GetCurrentUser();
                doc.ParentDocumentId = model.DocumentId;
                doc.isUseConteroller = true;
                doc.Controller = doc.Author;
                doc.isForInstruction = isForInstruction;
                doc.FieldValues = new List<DocumentFieldValues>();
                doc.RootDocumentId = model.RootDocumentId;
                doc.RootDocumentTypeId =
                    RepositoryFactory.GetDocumentRepository()
                        .List(m => m.Id == model.RootDocumentId)
                        .FirstOrDefault()
                        .DocumentType.Id;
                //if (!isForInstruction)
                //    doc.RootDocumentId = doc.ParentDocumentId;

                List<Notifications> tmpListNotify = new List<Notifications>();

                BL.DocumentRouting.DocumentRouting.SendInstruction(doc.ParentDocumentId, doc, model.RootDocumentId, tmpListNotify, isForInstruction);

                tmpListNotify.ForEach(m =>
                {
                    SignalRWebNotifierHelper.SendNotifyToClient(m.ForWho.Name, m.Id);
                });

                if (!isForInstruction)
                    return JavaScript("$('#ModalInstructionResult').modal('hide'); refreshCurrentView();");
                else
                    return JavaScript("refreshCollapseThree();  $('#ModalInstructionResult').modal('hide');");
            }
            else
            {
                return View(model);
            }
        }


        public ActionResult ViewSignResult(Guid RouteUserId, Guid docId, bool isForInstruction = false, bool isDialog = false)
        {
            Document doc = null;
            ViewBag.isForInstruction = isForInstruction;
            if (!isForInstruction)
                doc = RepositoryFactory.GetRepository<Document>().Single(m => m.Id == docId);
            else
                doc = RepositoryFactory.GetRepository<Instruction>().Single(m => m.Id == docId);

            if (doc != null)
            {
                if (doc.DocumentSignStages.Exists(m => m.RouteUsers.Exists(n => n.Id == RouteUserId)))
                {
                    var tmpRouteUser = doc.DocumentSignStages.SingleOrDefault(m => m.RouteUsers.Exists(n => n.Id == RouteUserId)).RouteUsers.SingleOrDefault(m => m.Id == RouteUserId);
                    if (tmpRouteUser != null)
                    {
                        RouteStageUser tmpRealRouteUser = tmpRouteUser;

                        //Нашли текущий этап
                        if (tmpRouteUser.SecondChanceForId != null)
                        {
                            tmpRealRouteUser = doc.DocumentSignStages.SingleOrDefault(m => m.RouteUsers.Exists(n => n.Id == tmpRouteUser.SecondChanceForId)).RouteUsers.SingleOrDefault(m => m.Id == tmpRouteUser.SecondChanceForId);
                            while (tmpRealRouteUser.SecondChanceForId != null)
                            {
                                tmpRealRouteUser = doc.DocumentSignStages.SingleOrDefault(m => m.RouteUsers.Exists(n => n.Id == tmpRealRouteUser.SecondChanceForId)).RouteUsers.SingleOrDefault(m => m.Id == tmpRealRouteUser.SecondChanceForId);
                            }
                        }
                        else
                        {
                            tmpRealRouteUser = tmpRouteUser;
                        }

                        if (!isDialog)
                            return View(new Tuple<Document, RouteStageUser, RouteStageUser>(doc, tmpRealRouteUser, tmpRouteUser));
                        else
                            return View("ViewSignResult_dialog", new Tuple<Document, RouteStageUser, RouteStageUser>(doc, tmpRealRouteUser, tmpRouteUser));
                    }
                }
            }
            return null;
        }

        public ActionResult GetSignResultsForControl(Guid docId, bool isForInstruction = false)
        {
            Document doc = null;
            if (!isForInstruction)
                doc = RepositoryFactory.GetRepository<Document>().Single(m => m.Id == docId);
            else
                doc = RepositoryFactory.GetRepository<Instruction>().Single(m => m.Id == docId);

            ViewBag.DocId = doc.Id;
            ViewBag.isForInstruction = isForInstruction;

            if (doc != null)
            {
                return View(new Tuple<Document, List<RouteStage>>(doc, doc.DocumentSignStages.Where(m => m.ControlPerformForRouteStageUserId != null && m.isCurrent && m.RouteUsers.Exists(x => x.IsCurent && x.SignUser.UserId == RepositoryFactory.GetCurrentUser())).ToList()));
            }

            return View(new Tuple<Document, List<RouteStage>>(doc, new List<RouteStage>()));
        }


        public ActionResult GetInstructionsForDocument(Guid DocId, bool isForInstruction = false)
        {
            Document doc = null;
            if (!isForInstruction)
                doc = RepositoryFactory.GetRepository<Document>().Single(m => m.Id == DocId);
            else
                doc = RepositoryFactory.GetRepository<Instruction>().Single(m => m.Id == DocId);
            ViewBag.ParentDocId = DocId;
            return View(GetInstructions(doc));
        }

        public ActionResult GetInstructionsForMainWindowDocument(Guid DocId, bool isForInstruction = false)
        {
            Document doc = null;
            if (!isForInstruction)
                doc = RepositoryFactory.GetRepository<Document>().Single(m => m.Id == DocId);
            else
                doc = RepositoryFactory.GetRepository<Instruction>().Single(m => m.Id == DocId);
            ViewBag.ParentDocId = DocId;

            var instructionList = GetInstructions(doc);

            instructionList.ForEach(m =>
            {
                RepositoryFactory.GetInstructionRepository().SetDocumentViewDateByViewer(RepositoryFactory.GetCurrentUser().ToString(), m.Id);
            });

            return View(instructionList);
        }


        private List<InstructionForDocumentViewModel> GetInstructions(Document Document, int level = 0)
        {
            var result = new List<InstructionForDocumentViewModel>();

            foreach (var docSignStage in Document.DocumentSignStages)
            {
                foreach (var routeStageUser in docSignStage.RouteUsers)
                {
                    if (routeStageUser.Instructions != null)
                        if (routeStageUser.Instructions.Any())
                        {
                            foreach (var instructionId in routeStageUser.Instructions)
                            {
                                var instruction = RepositoryFactory.GetRepository<Instruction>().Single(i => i.Id == instructionId);
                                if (instruction != null)
                                {
                                    var comment = string.Empty;
                                    var att = new List<Guid>();

                                    var stage = instruction.DocumentSignStages.LastOrDefault(h => h.RouteTypeId != BL.DocumentRouting.DocumentRouting.settings.ControlPerformStage);
                                    if (stage != null)
                                    {
                                        var user = stage.RouteUsers.Where(mn => mn.SignResult != null).OrderByDescending(mn => mn.SignResult.Date).FirstOrDefault();
                                        if (user != null)
                                        {
                                            var signResult = user.SignResult;
                                            if (signResult != null)
                                            {
                                                comment = signResult.Comment;
                                                att = signResult.attachment;
                                            }
                                        }
                                    }

                                    var isPerformActionStage = instruction.IsCurrentPerformationStageId;
                                    var isFinishedOk = instruction.docState == DocumentState.FinishedOk;
                                    var finishedRouteUserId =
                                        instruction.DocumentSignStages[0].RouteUsers
                                            .FirstOrDefault(mn => mn.SignResult != null) != null
                                            ? instruction.DocumentSignStages[0].RouteUsers
                                                .Where(mn => mn.SignResult != null)
                                                .OrderByDescending(mn => mn.SignResult.Date).FirstOrDefault().Id
                                            : Guid.Empty;
                                    result.Add(new InstructionForDocumentViewModel()
                                    {
                                        Id = instruction.Id,
                                        FinishedRouteUserId = finishedRouteUserId,
                                        ParentId = instruction.ParentDocumentId,
                                        isPerformControlAction = isPerformActionStage,
                                        Body = instruction.Body,
                                        RouteStageId = isPerformActionStage ? Guid.Empty : instruction.CurentStageId,
                                        RouteStageUserId = isPerformActionStage ? instruction.StageUserIdForCurentPerformationStage : instruction.CurrentStageUserId,
                                        CurrentSigner = isPerformActionStage ? instruction.CurrentUserIdForPerformation : instruction.UserFor.UserId,
                                        CurrentSignerFIO = RepositoryFactory.GetRepository<User>().Single(x1 => x1.UserId == (isPerformActionStage ? instruction.CurrentStagePerformRealUserId : instruction.UserFor.UserId)).GetFIO(),
                                        UserNameFor = instruction.UserFor.GetFIO(),
                                        UserNameFrom = instruction.Author.GetFIO(),
                                        DateBefore = instruction.FinishDate,
                                        Header = instruction.Header,
                                        DocState = instruction.docState,
                                        level = level,
                                        SignResult = comment,
                                        FinishDate = stage?.FinishDate,
                                        Attachments = att,
                                        CreateDate = instruction.CreateDate
                                    });

                                    result.AddRange(GetInstructions(instruction, level + 1));
                                }
                            }
                        }
                }
            }

            foreach (var m in result)
            {
                RepositoryFactory.GetNotificationRepository().List(m1 => m1.DocumentId == m.Id && m1.ForWho.UserId == RepositoryFactory.GetCurrentUser()).ToList().ForEach(x =>
                    {
                        x.ViewDateTime = DateTime.Now;
                        RepositoryFactory.GetNotificationRepository().update(x);
                        SignalRWebNotifierHelper.UpdateNotifyAtClient(x.ForWho.Name, x.Id);
                    }
                );
            }

            return result;
        }

        public List<DocumentTemplate> GetTemplates(Guid documentType)
        {
            var user = RepositoryFactory.GetCurrentUser();
            var templates = RepositoryFactory.GetRepository<DocumentTemplate>().List(dt => !dt.isDeleted
                                && dt.ViewModel.DocumentTypeId == documentType
                                && (dt.UserId == Guid.Empty || dt.UserId == user)).ToList();
            return templates;
        }

        private void SaveDocumentForFTS(Guid docId)
        {
            using (FullTextSearch.ServiceFTSClient clnt = new FullTextSearch.ServiceFTSClient())
            {
                try
                {
                    clnt.SaveDocument(docId);
                }
                catch (Exception ex)
                {

                }
            }
        }

        public ActionResult getComboboxForDynamicReference(DynamicReferenceEditorTemplateViewModel model)
        {

            var dynRef = RepositoryFactory.GetRepository<DynamicReference>().Single(m => m.Id == model.ReferenceId && !m.isDeleted);

            var fieldToDisplay = dynRef.FieldTemplates.Single(m => m.isDisplay);

            var tmpList = new List<SelectListItem>();

            tmpList.AddRange(RepositoryFactory.GetRepository<DynamicRecord>().List(m => !m.isDeleted && m.DynamicReferenceId == dynRef.Id).
          GroupBy(m => m.RecordId).
          Select(m => new SelectListItem() { Value = m.Key.ToString(), Text = m.Single(n => n.DynamicReferenceFieldTemplateId == fieldToDisplay.Id).Value.Value.ValueToDisplay, Selected = m.Key == model.SelectedItemGuid }).ToList());
            ViewBag.list = tmpList;

            return View(model);


        }


        public ActionResult GetDynamicReferenceForSelect2(Guid referenceId)
        {
            var dynRef = RepositoryFactory.GetRepository<DynamicReference>().Single(m => m.Id == referenceId && !m.isDeleted);

            var fieldToDisplay = dynRef.FieldTemplates.Single(m => m.isDisplay);

            List<dynamic> list = new List<dynamic>();

            list.AddRange(RepositoryFactory.GetRepository<DynamicRecord>().List(x => !x.isDeleted && x.DynamicReferenceId == dynRef.Id).
                GroupBy(m => m.RecordId).
                Select(m => new { id = m.Key.ToString(), text = m.Single(n => n.DynamicReferenceFieldTemplateId == fieldToDisplay.Id).Value.Value.ValueToDisplay }).ToList());

            var total = list.Count();

            return Json(new
            {
                rows = list,

            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DocumentDynamicFilter(Guid docTypeId, DocumentFilterVM documentFilterVM, bool isNewFilter = true)
        {
            //ViewBag.isForUpdate = false;
            //ViewBag.DocTypeId = docTypeId;

            if (documentFilterVM == null)
            {
                documentFilterVM = new DocumentFilterVM();
            }

            ViewBag.isNewFilter = isNewFilter;

            if (isNewFilter == true)
            {
                documentFilterVM.DocTypeId = docTypeId;
                documentFilterVM.StartDate = DateTime.Now.ToString("dd.MM.yyyy");
                documentFilterVM.EndDate = DateTime.Now.ToString("dd.MM.yyyy");
                documentFilterVM.MethodOfSearchForHeader = "IsInclusionByHeader";

                if (docTypeId != Guid.Empty)
                {
                    var res = RepositoryFactory.GetRepository<DocumentType>().Single(x => x.Id == docTypeId);

                    //LUUID("b0e57b9b-693e-6a46-a876-eae0402ebbe7") - пользователь
                    //m.FieldType.Id.ToString().ToLower() != "a427dbfb-9cb7-4f52-9d5e-c7d0677e8103" &&
                    //m.FieldType.Id.ToString().ToLower() != "9b7be5b0-3e69-466a-a876-eae0402ebbe7"
                    res.FieldTemplates.Where(m => m.FieldType.Id.ToString().ToLower() != "b0e57b9b-693e-6a46-a876-eae0402ebbe7").ToList().

                    ForEach(fieldTemplate =>
                    {
                        documentFilterVM.DynamicFields.Add(new DynamicFields
                        {
                            FieldTemplate = fieldTemplate,
                            Value = fieldTemplate.FieldType.Id.ToString() == "d88f464a-ca95-4c41-ad7d-7df5adfd90d8" ? DateTime.Now.ToString("dd.MM.yyyy") : null,
                            MethodOfSearchForDynamicValue = "IsInclusion"
                        });
                    });
                }

                return View(documentFilterVM);
            }
            else
            {
                return View(documentFilterVM);
            }
        }
    }
}
