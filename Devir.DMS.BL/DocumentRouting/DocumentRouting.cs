using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.Document.DocumentNotifications;
using Devir.DMS.DL.Models.Document.Route;
using Devir.DMS.DL.Models.DocumentTemplates;
using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Models.Settings;
using Devir.DMS.DL.Models.WebNotifications;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;

using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.BL.DocumentRouting
{
    public class DocumentRouting
    {
        public static Settings settings { get; set; }

        //Создание маршрута для документа
        public static List<Notifications> CreateRouteStagesForDocument(Document doc, User currentUser, List<WebNotifications> webRefreshNotifications)
        {
            //Если поручение то добавляем пользователя в читатели основного документа
            if (doc is Instruction)
            {
               //doc.ParentDocumentId

               
               
            }

            List<Notifications> result = new List<Notifications>();

            if(doc.DocumentViewers == null)
            doc.DocumentViewers = new Dictionary<String, DocumentViewers>();

            doc.Author = currentUser;
            AddToViewers(doc, currentUser, webRefreshNotifications, true);

            if (doc.DocumentType.RouteTemplates != null && doc.DocumentType.RouteTemplates.Count() > 0)
            {
                doc.DocumentSignStages = new List<RouteStage>();
            }



            //ПРобег по всем динамическим маршрутам типа документа
            doc.DocumentType.RouteTemplates.OrderBy(m => m.FieldOrder).ToList().ForEach(m =>
            {

                //Определяем тип этапа маршрута документа
                var tmpRouteTypeId = m.TypeOfTheRoute.Id;

               

                if (!String.IsNullOrEmpty(m.RouteCondition))
                {

                    var s = System.Linq.Dynamic.DynamicQueryable.Where(doc.FieldValues.AsQueryable(), m.RouteCondition);

                    if (s.Count() < 1)
                    {
                        return;
                    }
                }


                var d = doc.FieldValues;

                //Если тип этапа "согласование"
                if (tmpRouteTypeId == settings.NegotiationStage) //"b932b534-29f4-4099-bf92-020c271ef18c"
                {

                    //Определяем есть ли в документе этап согласования
                    if (doc.Negotiators != null && doc.Negotiators.Count() > 0)
                    {
                        //doc.DocumentNegotiationStages = new Negotiation() { Stages = new List<NegotiationRouteStage>() };
                        //Согласования есть!
                        doc.Negotiators.ForEach(neg =>
                            {
                                List<RouteStageUser> negotiationStageSignUserList = new List<RouteStageUser>();

                                neg.UsersForNegotiation.ForEach(n =>
                                {
                                    negotiationStageSignUserList.Add(new RouteStageUser()
                                    {
                                        Id = Guid.NewGuid(),
                                        IsCurent = false,
                                        Order = n.Order,
                                        SignResult = null,
                                        SignUser = RepositoryFactory.GetRepository<User>().Single(k => k.UserId == n.UserId),
                                        UsersActions = RepositoryFactory.GetRepository<RouteType>().Single(k => k.Id == settings.NegotiationStage).Actions, //new Guid("b932b534-29f4-4099-bf92-020c271ef18c")
                                    });
                                });

                                //Обработаемс
                                doc.DocumentSignStages.Add(new RouteStage()
                                {
                                    Id = Guid.NewGuid(),
                                    Name = m.TypeOfTheRoute.Name,
                                    Order = doc.DocumentSignStages.Count() > 0 ? doc.DocumentSignStages.Max(j => j.Order + 1) : 0,
                                    StageType = doc.DocumentSignStages.Count() > 0 ? doc.DocumentSignStages.Max(o => o.Order) + 1 + neg.StageType : neg.StageType,
                                    RouteUsers = negotiationStageSignUserList,
                                    isCurrent = false,
                                    RouteTypeId = tmpRouteTypeId,
                                    RouteTemplateId = m.Id
                                });
                            });

                    }
                    /* Закончили с согласованием  переходим к сладкому :)*/



                }
                else if (tmpRouteTypeId == settings.PerformInstructionWaiting)
                {
                    doc.DocumentSignStages.Add(new RouteStage()
                    {
                        Id = Guid.NewGuid(),
                        Name = m.TypeOfTheRoute.Name,
                        Order = doc.DocumentSignStages.Count() > 0 ? doc.DocumentSignStages.Max(j => j.Order + 1) : 0,
                        isCurrent = false,
                        RouteTypeId = m.TypeOfTheRoute.Id,
                        RouteUsers = new List<RouteStageUser>(),
                        NeedSignResultConfirmation = false,
                        RouteTemplateId = m.Id

                    });
                }
                else
                {


                    //Получаем пользователей для этапа
                    List<RouteStageUser> tmpUsersForStage = new List<RouteStageUser>();
                    if (doc is Instruction)
                    {
                        tmpUsersForStage.Add(new RouteStageUser()
                                                                {
                                                                    Id = Guid.NewGuid(),
                                                                    Order = 1,
                                                                    IsCurent = false,
                                                                    UsersActions = null,
                                                                    SignUser = (doc as Instruction).UserFor,
                                                                    SignResult = null
                                                                });
                    }
                    else
                    {
                        if (!m.isAuthor)
                            if (m.UserByDefault == null)
                            {
                                var tmpFieldValue = doc.FieldValues.Single(k => k.FieldTemplateId == m.DocumentFieldTemplate);
                                tmpUsersForStage = GetRouteStageUsersFromDynamicField(tmpFieldValue);
                            }
                            else
                            {
                                //Сделано с целью использования пользователя по умолчанию
                                tmpUsersForStage = new List<RouteStageUser>();
                                tmpUsersForStage.Add(new RouteStageUser()
                                {
                                    Id = Guid.NewGuid(),
                                    IsCurent = false,
                                    Order = 0,
                                    SignResult = null,
                                    SecondChanceForId = null,
                                    SignUser = RepositoryFactory.GetRepository<User>().Single(n => n.UserId == m.UserByDefault.Value),
                                    UsersActions = m.TypeOfTheRoute.Actions
                                });
                            }
                        else
                        { // дЛЯ возврата к автору по маршруту
                            tmpUsersForStage = new List<RouteStageUser>();
                            tmpUsersForStage.Add(new RouteStageUser()
                            {
                                Id = Guid.NewGuid(),
                                IsCurent = false,
                                Order = 0,
                                SignResult = null,
                                SecondChanceForId = null,
                                SignUser = RepositoryFactory.GetRepository<User>().Single(n => n.UserId == doc.Author.UserId),
                                UsersActions = m.TypeOfTheRoute.Actions
                            });
                        }

                        
                    }

                    var tmpActionList = RepositoryFactory.GetRepository<RouteType>().Single(k => k.Id == m.TypeOfTheRoute.Id).Actions;
                    tmpUsersForStage.ForEach(i =>
                    {
                        i.UsersActions = new List<RouteAction>();
                        i.UsersActions = tmpActionList;
                    });

                    doc.DocumentSignStages.Add(new RouteStage()
                    {
                        Id = Guid.NewGuid(),
                        Name = m.TypeOfTheRoute.Name,
                        Order = doc.DocumentSignStages.Count() > 0 ? doc.DocumentSignStages.Max(j => j.Order + 1) : 0,
                        isCurrent = false,
                        RouteTypeId = m.TypeOfTheRoute.Id,
                        RouteUsers = tmpUsersForStage,
                        NeedSignResultConfirmation = m.NeedSignResultConfirmation,
                        RouteTemplateId = m.Id
                    });
                }
            });



            #region первоначальная отправка по маршруту


            if (doc.DocumentSignStages.Count() > 0)
            {
                var tmpFirstSingStage = doc.DocumentSignStages.OrderBy(m => m.Order).FirstOrDefault();
                if (tmpFirstSingStage != null)
                {                    
                    doc.docState = DocumentState.InWork;
                    tmpFirstSingStage.isCurrent = true;                    
                    runDocumentNumbering(doc, true);
                    result.AddRange(MoveToNextStageUsers(tmpFirstSingStage, doc, webRefreshNotifications, true));
                }
            }

            #endregion


            return result;
        }

        //Выборка пользователей из полей с пользователями для маршрутов
        private static List<RouteStageUser> GetRouteStageUsersFromDynamicField(DocumentFieldValues dynamicField)
        {
            var result = new List<RouteStageUser>();

            var sfieldType = dynamicField.FieldTypeId;

            //и куча ифов :) ну индусятина, а что поделаешь времени мало :(

            //Если используется тип поля "Пользователь"
            if (sfieldType == settings.UserFieldType) //"9b7be5b0-3e69-466a-a876-eae0402ebbe7"
            {
                result.Add(new RouteStageUser()
                {
                    Id = Guid.NewGuid(),
                    Order = 1,
                    IsCurent = false,
                    UsersActions = null,
                    SignUser = RepositoryFactory.GetRepository<User>().Single(k => k.UserId == dynamicField.GuidValue.Value),
                    SignResult = null
                });
            }

            //Если исользуется тип поля "Список пользователей"
            if (sfieldType == settings.ListUsersFieldType) //"2c308153-04a9-4bf6-b021-cc28b82a7ab5"
            {

                dynamicField.UserForRouteListValue.ForEach(m =>
                {
                    result.Add(new RouteStageUser()
                    {
                        Id = Guid.NewGuid(),
                        Order = m.Order,
                        IsCurent = false,
                        UsersActions = null,
                        SignUser = RepositoryFactory.GetRepository<User>().Single(k => k.UserId == m.UserId),
                        SignResult = null
                    });
                });
            }

            return result;
        }

        ////Движение документа к следующему пользователю
        public static List<Notifications> MoveNext(Document doc, User curentUser, UserSignResult signResult, List<WebNotifications> webRefreshNotifications)
        {
            var result = new List<Notifications>();

              if(signResult.Action.Id == settings.OkSignAction ||
                  signResult.Action.Id == settings.OkNegotiationAction ||
                  signResult.Action.Id == settings.OkPerformAction ||
                  signResult.Action.Id == settings.OkControlPerformStage)
                runDocumentNumbering(doc, false);

            //Определяем текущий этап
            var tmpStage = doc.DocumentSignStages.SingleOrDefault((m => m.isCurrent && m.ControlPerformForRouteStageUserId == null));
            //Если текущий этап в исполнении и подписании            
            if (tmpStage != null)
            {
                //Ищем в этапе себя и если мы имеем право на подпись то ставим результат исполнения/подписания
                var tmpRouteUser = tmpStage.RouteUsers.FirstOrDefault(m => !m.isSigned && m.SignUser.UserId == curentUser.UserId && m.IsCurent);
                if (tmpRouteUser != null)
                {
                    tmpRouteUser.SignResult = signResult;
                    tmpRouteUser.IsCurent = false;


                    result.Add(CreateNotifications(curentUser, doc.Author, tmpStage, signResult.Action, doc, webRefreshNotifications));

                    /*Для возвратов документа к Автору*/
                    #region Возврат документов к Автору / Или же отказ по всему документу, на стадии подпись можно и такое, отказ и с этим ничего не поделаешь
                    //Если вернули на этапе согласования то сложно 
                    if (tmpStage.RouteTypeId == settings.NegotiationStage)
                    {
                        if (signResult.Action.Id == settings.NotOkNegotiationAction)
                        {
                            //Если этап согласования не параллельный 
                            if (tmpStage.StageType != DL.Models.Document.NegotiatorsRoutes.NegotiationStageTypes.Parallel)
                            {
                                doc.docState = DocumentState.FinishedWithError;
                                return result;
                            }
                            else
                            {
                                //Если параллельный ничего не делаем, т.к. делать будем на этапе перехода к следующей стадии
                            }
                        }
                    }
                    else
                    {
                        if (signResult.Action.Id == settings.NotOkPerformAction || signResult.Action.Id == settings.NotOkSigntAction)
                        {
                            doc.docState = DocumentState.FinishedWithError;
                            return result;
                        }
                    }
                    #endregion

                    bool bl = tmpStage.NeedSignResultConfirmation??true;



                    if (tmpStage.RouteTypeId == settings.PerformStage && signResult.Action.Id == settings.OkPerformAction && bl && doc.isUseConteroller)
                    {
                        var tmpRouteUsers = new List<RouteStageUser>();
                       
                        tmpRouteUsers.Add(new RouteStageUser()
                        {
                            Id = Guid.NewGuid(),
                            IsCurent = true,
                            Order = 0,
                            SignResult = null,
                            SecondChanceForId = null,
                            SignUser =  (doc is Instruction)?((Instruction)doc).Controller:doc.Author,
                            UsersActions = RepositoryFactory.GetRepository<RouteType>().Single(m => m.Id == settings.ControlPerformStage).Actions
                        });

                        doc.DocumentSignStages.Add(new RouteStage()
                        {
                            Id = new Guid(),
                            isCurrent = true,
                            Name = "Контроль исполнения", 
                            Order = 0,
                            ControlPerformForRouteStageUserId = tmpRouteUser.Id,
                            RouteTypeId = settings.ControlPerformStage,
                            RouteUsers = tmpRouteUsers
                        });

                        AddToViewers(doc, (doc is Instruction) ? ((Instruction)doc).Controller : doc.Author, webRefreshNotifications);


                        result.Add(new Notifications()
                        {
                            Id = Guid.NewGuid(),
                            ForWho = (doc is Instruction) ? ((Instruction)doc).Controller : doc.Author,
                            DocumentId = doc.Id,
                            LinkText = (doc is Instruction) ? String.Format("/Document/GetInstruction?InstructionId={0}", doc.Id) : String.Format("/Document/GetDocument?DocumentId={0}", doc.Id),
                            Text = String.Format("Уважаемый(-ая) {0},", ((doc is Instruction) ? ((Instruction)doc).Controller : doc.Author).GetFIO()) + ((doc is Instruction) ? String.Format("{0} запрашивает контроль исполнения для поручения {1} {2}", curentUser.GetFIO(), doc.DocumentNumber, doc.Header) : String.Format("для документа \"{1} {2} {3}\"", curentUser.GetFIO(), doc.DocumentType.Name, doc.DocumentNumber, doc.Header)),
                            ViewDateTime = null
                        });
                    }
                    else
                    {

                        //Смотрим есть ли еще ктонибудь для подписи текущего документа, если нет, то вперед на следующий этап
                        result.AddRange(MoveToNextStage(tmpStage, doc, webRefreshNotifications));
                        //Основной документ помечаем выполненым
                        if (doc is Instruction)
                        {
                            var tmpDoc = RepositoryFactory.GetRepository<Document>().Single(x => x.Id == doc.ParentDocumentId);
                            if (tmpDoc != null)
                                if (tmpDoc.CurentStageTypeId == settings.PerformInstructionWaiting)
                                {
                                    MoveNext(tmpDoc, tmpRouteUser.SignUser, new UserSignResult() { ActionId = settings.OkPerformAction, Comment = "", Action = RepositoryFactory.GetRepository<RouteAction>().Single(q => q.Id == settings.OkPerformAction), Date = DateTime.Now, User = tmpRouteUser.SignUser, attachment = new List<Guid>() }, webRefreshNotifications);
                                    RepositoryFactory.GetRepository<Document>().update(tmpDoc);
                                }
                        }
                    }
                }

                
                
            }

         

          

            return result;
        }


        public static List<Notifications> SetControPerformResult(Guid UserForRouteId, Document doc, User currentUser, UserSignResult signResult, List<WebNotifications> webRefreshNotifications)
        {
            List<Notifications> result = new List<Notifications>();

            var tmpStage = doc.DocumentSignStages.SingleOrDefault((m => m.isCurrent && m.RouteTypeId == settings.PerformStage));
            if (tmpStage != null)
            {
                var tmpUser = tmpStage.RouteUsers.SingleOrDefault(m => m.Id == UserForRouteId);
                if (tmpUser != null)
                {

                    var tmpCntrolStage = doc.DocumentSignStages.SingleOrDefault(m => m.ControlPerformForRouteStageUserId == tmpUser.Id);
                    if (tmpCntrolStage != null)
                    {

                        var tmpUserControl = tmpCntrolStage.RouteUsers.SingleOrDefault(m => m.IsCurent && m.SignUser.UserId == currentUser.UserId);

                        tmpCntrolStage.isCurrent = false;
                        tmpUserControl.IsCurent = false;
                        tmpUserControl.SignResult = signResult;
                        tmpCntrolStage.FinishDate = DateTime.Now;

                        if (tmpUserControl != null)
                        {
                            if (signResult.Action.Id == settings.OkControlPerformStage)
                            {
                                tmpUserControl.SignResult = signResult;
                                tmpUserControl.IsCurent = false;
                                //tmpStage.isCurrent = false;
                                MoveToNextStage(tmpStage, doc, webRefreshNotifications);

                                if (doc is Instruction)
                                {
                                    var tmpDoc = RepositoryFactory.GetRepository<Document>().Single(x => x.Id == doc.ParentDocumentId);
                                    if(tmpDoc != null)
                                    if (tmpDoc.CurentStageTypeId == settings.PerformInstructionWaiting)
                                    {
                                        MoveNext(tmpDoc, tmpUser.SignUser, new UserSignResult() { ActionId = settings.OkPerformAction, Comment = "", Action = RepositoryFactory.GetRepository<RouteAction>().Single(q => q.Id == settings.OkPerformAction), Date = DateTime.Now, User = tmpUser.SignUser, attachment = new List<Guid>() }, webRefreshNotifications);
                                        RepositoryFactory.GetRepository<Document>().update(tmpDoc);
                                    }
                                    

                                }


                                result.Add(new Notifications()
                                {
                                    Id=Guid.NewGuid(),
                                    ForWho = (doc is Instruction) ? ((Instruction)doc).Controller : doc.Author,
                                    DocumentId = doc.Id,
                                    LinkText = (doc is Instruction) ? String.Format("/Document/GetInstruction?InstructionId={0}", doc.Id) : String.Format("/Document/GetDocument?DocumentId={0}", doc.Id),
                                    Text = String.Format("Уважаемый(-ая) {0},", ((doc is Instruction) ? ((Instruction)doc).Controller : doc.Author).GetFIO()) + ((doc is Instruction) ? String.Format("{0} подтвердил исполнение поручения {1} {2}", currentUser.GetFIO(), doc.DocumentNumber, doc.Header) : String.Format(" подтвердил исполнение документа \"{1} {2} {3}\"", currentUser.GetFIO(), doc.DocumentType.Name, doc.DocumentNumber, doc.Header)),
                                    ViewDateTime = null
                                });

                            }
                            if (signResult.Action.Id == settings.NotOkControlPerformStage)
                            {
                                //Создаем пользовтаеля для исполнения еще раз
                                tmpStage.RouteUsers.Add(new RouteStageUser()
                                {
                                    Id = Guid.NewGuid(),
                                    IsCurent = true,
                                    Order = tmpStage.RouteUsers.Count() > 0 ? tmpStage.RouteUsers.Max(x => x.Order) + 1 : 0,
                                    SecondChanceForId = tmpUser.Id,
                                    SignResult = null,
                                    SignUser = tmpUser.SignUser,
                                    UsersActions = tmpUser.UsersActions
                                });

                                //var tmpRealStage = doc.DocumentSignStages.Where(m=>m.RouteUsers.Any(x=>x.Id == tmpUser.SecondChanceForId)).FirstOrDefault();
                                //if (tmpRealStage != null)
                                //{
                                    //var tmpRealUserName = tmpRealStage.RouteUsers.Where(m => m.Id == tmpUser.SecondChanceForId).FirstOrDefault();

                                    //if (tmpRealUserName != null)
                                    //{

                                        result.Add(new Notifications()
                                        {
                                            Id = Guid.NewGuid(),
                                            ForWho = tmpUser.SignUser,
                                            DocumentId = doc.Id,
                                            LinkText = (doc is Instruction) ? String.Format("/Document/GetInstruction?InstructionId={0}", doc.Id) : String.Format("/Document/GetDocument?DocumentId={0}", doc.Id),
                                            Text = String.Format("Уважаемый(-ая) {0},", tmpUser.SignUser.GetFIO()) + ((doc is Instruction) ? String.Format("{0} отклонил исполнение поручния {1} {2}", currentUser.GetFIO(), doc.DocumentNumber, doc.Header) : String.Format(" отклонил исполнение документа \"{1} {2} {3}\"", currentUser.GetFIO(), doc.DocumentType.Name, doc.DocumentNumber, doc.Header)),
                                            ViewDateTime = null
                                        });
                                //    }
                                //}

                            }

                        }
                    }
                }

            }

            return result;


        }

        private static List<Notifications> MoveToNextStage(RouteStage tmpStage, Document doc, List<WebNotifications> webRefreshNotifications)
        {
         

            List<Notifications> result = new List<Notifications>();

            //Если в стадия параллельная и есть еще подписывающие то годим
            if (tmpStage.StageType != DL.Models.Document.NegotiatorsRoutes.NegotiationStageTypes.Sequenced && tmpStage.RouteUsers.Any(m => !m.isSigned & m.IsCurent))
            {
                return result;
            }

            if (tmpStage.RouteUsers.Count(m => !m.isSigned & !m.IsCurent && m.SecondChanceForId == null) < 1)
            {



                #region Возврат документа автору на стадии согласования если ктото не согласовал
                if (tmpStage.RouteUsers.Where(m => m.SignResult != null && m.SignResult.Action.Id == settings.NotOkNegotiationAction).Count() > 0)
                {
                    doc.docState = DocumentState.FinishedWithError;
                    return result;
                }
                #endregion

                

                tmpStage.isCurrent = false;
                tmpStage.FinishDate = DateTime.Now;

                if (doc.DocumentSignStages.Count(m => m.Order > tmpStage.Order) > 0)
                {
                    var tmpNextStage = doc.DocumentSignStages.Where(m => m.Order > tmpStage.Order).FirstOrDefault();
                    tmpNextStage.isCurrent = true;

                    result.AddRange(MoveToNextStageUsers(tmpNextStage, doc, webRefreshNotifications));
                }
                else
                {
                    
                    //Не осталось этапов для подписей:(
                    doc.docState = DocumentState.FinishedOk;



                }
            }
            else   //Если есть то двигаем документ к нему                  
            {

               
                //var tmpNextUser = tmpStage.RouteUsers.FirstOrDefault(m => m.Order > tmpRouteUser.Order);
                //tmpNextUser.IsCurent = true;

                ////Отправка нотификах
                //result.Add(new Notifications() { ForWho = tmpNextUser.SignUser, Text = "" });

                result.AddRange(MoveToNextStageUsers(tmpStage, doc, webRefreshNotifications));
            }

            return result;
        }



        private static List<Notifications> MoveToNextStageUsers(RouteStage tmpNextStage, Document doc, List<WebNotifications> webRefreshNotifications, bool firstRun=false)
        {

            var result = new List<Notifications>();

            //Если следующий этап типа не согласование
            if (tmpNextStage.RouteTypeId != settings.NegotiationStage) //"b932b534-29f4-4099-bf92-020c271ef18c"
            {
                //Если этап типа ознакомление
                if (tmpNextStage.RouteTypeId == settings.AddViewersStage)
                {

                    //Логика добавления в читатели



                    tmpNextStage.RouteUsers.ForEach(m =>
                        {
                            m.SignResult = new UserSignResult();
                            m.SignResult.Action = new RouteAction();
                            m.IsCurent = false;
                            AddToViewers(doc, m.SignUser, webRefreshNotifications);
                            result.Add(new Notifications()
                                {
                                    Id=Guid.NewGuid(),
                                    ForWho = m.SignUser,
                                    Text = String.Format("Уважаемый(-ая) {0},{1} отправил(-а) Вам на ознакомление документ \"{2} {3} {4}\"", m.SignUser.GetFIO(), doc.Author.GetFIO(), doc.DocumentType.Name, doc.DocumentNumber, doc.Header),
                                    LinkText = String.Format("Для просмотра документа перейдите по <a href=\"/Document/GetDocument?DocumentId={0}\">ссылке</a>", doc.Id.ToString()), // Guid.NewGuid()
                                    DocumentId = doc.Id
                                });

                        });


                    tmpNextStage.isCurrent = false;
                    tmpNextStage.FinishDate = DateTime.Now;

                    doc.docState = DocumentState.FinishedOk;
                    //Отправка 

                    return result;
                }

               

                
                //Если подпись или исполнение
                tmpNextStage.RouteUsers.ForEach(m =>
                {
                    m.IsCurent = true;

                    AddToViewers(doc, m.SignUser, webRefreshNotifications);

                    //Отправка нотификах
                    result.Add(CreateNotifications(doc.Author, m.SignUser, tmpNextStage, null, doc, webRefreshNotifications));
                });



                if (tmpNextStage.RouteTypeId == settings.PerformInstructionWaiting)
                {
                    //Отправка поручений

                    if (doc.TempInstructionStorage != null)
                    {
                        tmpNextStage.RouteUsers = new List<RouteStageUser>();

                        doc.TempInstructionStorage.ForEach(d =>
                        {                            
                            tmpNextStage.RouteUsers.Add(new RouteStageUser()
                            {
                                Id = new Guid(),
                                Instructions = new List<Guid>(),
                                IsCurent = true,
                                Order = 0,
                                SecondChanceForId = null,
                                SignResult = null,                                
                                SignUser = d.UserFor,
                                UsersActions = new List<RouteAction>()
                            });

                            webRefreshNotifications.AddRange(SendInstruction(doc.Id, d, doc.Id, result, false, doc));

                           
                        });
                    }
                }


            }
            else
            {
                //Если согласование

                //Если соглосование параллельное
                if (tmpNextStage.StageType == DL.Models.Document.NegotiatorsRoutes.NegotiationStageTypes.Parallel)
                {
                    tmpNextStage.RouteUsers.ForEach(m =>
                    {
                        m.IsCurent = true;

                        AddToViewers(doc, m.SignUser, webRefreshNotifications);

                        //Отправка нотификах
                        result.Add(CreateNotifications(doc.Author, m.SignUser, tmpNextStage, null, doc, webRefreshNotifications));
                    });
                }
                //Если не параллельное
                else
                {
                    var tmpNextNegotUser = tmpNextStage.RouteUsers.OrderBy(m => m.Order).FirstOrDefault(m=>!m.isSigned);
                    if (tmpNextNegotUser != null)
                    {
                        //Отправляем документ следующему пользователю на согласование
                        tmpNextNegotUser.IsCurent = true;

                        AddToViewers(doc, tmpNextNegotUser.SignUser, webRefreshNotifications);

                        //Отправка нотификах
                        result.Add(CreateNotifications(doc.Author, tmpNextNegotUser.SignUser, tmpNextStage, null, doc, webRefreshNotifications));
                    }
                }
            }

          
            return result;
        }

        public static void runDocumentNumbering(Document doc, bool firstRun){
              //Нумерация документов
            if (!(doc is Instruction))
            {
                var runDocumentNumbering = false;

                if (doc.DocumentType.StageiDForNumbering == Guid.Empty && firstRun)

                    runDocumentNumbering = true;
                else
                {
                    if (!firstRun && doc.DocumentSignStages.Single(m => m.Id == doc.CurentStageId).RouteTemplateId == doc.DocumentType.StageiDForNumbering)
                        runDocumentNumbering = true;
                }

                if (runDocumentNumbering && (doc.Version < 2 || doc.DocumentNumber.Trim().IndexOf("Версия") == 0))
                {
                    //  if (doc.DocumentSignStages.Single(m => m.Id == doc.CurentStageId).Order >= doc.DocumentType.StageOrderForNumbering)
                    if (doc.DocumentType.FiledTypeForNumbering != Guid.Empty)
                    {
                        var tmpFieldValue = doc.FieldValues.Single(m => m.FieldTemplateId == doc.DocumentType.FiledTypeForNumbering).DynamicRecordId;
                        doc.DocumentNumber = !doc.DocumentType.NumberingDependsOnUser ? RepositoryFactory.GetDocumentTypeCountRepository().GetDocumentNextNumber(tmpFieldValue).ToString() : RepositoryFactory.GetDocumentTypeCountRepository().GetDocumentNextNumberByUser(tmpFieldValue, doc.Author.UserId).ToString();
                    }
                    else
                    {
                        doc.DocumentNumber = !doc.DocumentType.NumberingDependsOnUser ? RepositoryFactory.GetDocumentTypeCountRepository().GetDocumentNextNumber(doc.DocumentType.Id).ToString() : RepositoryFactory.GetDocumentTypeCountRepository().GetDocumentNextNumberByUser(doc.DocumentType.Id, doc.Author.UserId).ToString();
                    }
                }
            }
        }

        public static Notifications CreateNotifications(User fromUser, User user, RouteStage stage, RouteAction action, Document doc, List<WebNotifications> webRefreshNotifications)
        {
            string Text = String.Format("Уважаемый(-ая) {0},", user.GetFIO());
            if (stage.RouteTypeId == settings.NegotiationStage)
            {
                if (action != null)
                {
                    if (action.Id == settings.OkNegotiationAction)
                        Text = Text + String.Format("{0} согласовал(-а) документ \"{1} {2} {3}\"", fromUser.GetFIO(), doc.DocumentType.Name, doc.DocumentNumber, doc.Header);
                    if (action.Id == settings.NotOkNegotiationAction)
                        Text = Text + String.Format("{0} отклонил(-а) согласование документа \"{1} {2} {3}\"", fromUser.GetFIO(), doc.DocumentType.Name, doc.DocumentNumber, doc.Header);
                }
                else
                {
                    Text = Text + String.Format("{0} отправил(-а) Вам на согласование документ \"{1} {2} {3}\"", fromUser.GetFIO(), doc.DocumentType.Name, doc.DocumentNumber, doc.Header);
                }
            }

            if (stage.RouteTypeId == settings.SignStage) //"145e6ca4-a973-4b52-98ab-1f2c42b8a897"
            {
                if (action != null)
                {
                    if (action.Id == settings.OkSignAction)
                        Text = Text + String.Format("{0} подписал(-а) документ \"{1} {2} {3}\"", fromUser.GetFIO(), doc.DocumentType.Name, doc.DocumentNumber, doc.Header);
                    if (action.Id == settings.NotOkSigntAction)
                        Text = Text + String.Format("{0} отклонил(-а) подписание документа \"{1} {2} {3}\"", fromUser.GetFIO(), doc.DocumentType.Name, doc.DocumentNumber, doc.Header);
                }
                else
                {
                    Text = Text + String.Format("{0} отправил(-а) Вам на подписание документ \"{1} {2} {3}\"", fromUser.GetFIO(), doc.DocumentType.Name, doc.DocumentNumber, doc.Header);
                }
            }

            if (stage.RouteTypeId.ToString() == "fbba25c0-d74f-4007-8edb-ec6e38e2486d")
            {

                if (doc is Instruction)
                {
                    if (action != null && action.Id == settings.OkPerformAction)
                    {
                        Text = Text + String.Format("{0} исполнил(-а) поручение \"{1} {2}\"", fromUser.GetFIO(), doc.DocumentNumber, doc.Header);
                    }
                    else
                    Text = Text + String.Format("{0} отправил(-а) Вам на исполнение поручение \"{1} {2}\"", fromUser.GetFIO(), doc.DocumentNumber, doc.Header);
                }
                else
                {
                    if (action !=null && action.Id == settings.OkPerformAction)
                    {
                        Text = Text + String.Format("{0} исполнил(-а) документ \"{1} {2} {3}\"", fromUser.GetFIO(), doc.DocumentType.Name, doc.DocumentNumber, doc.Header);
                        
                    }
                    else
                    Text = Text + String.Format("{0} отправил(-а) Вам на исполнение документ \"{1} {2} {3}\"", fromUser.GetFIO(), doc.DocumentType.Name, doc.DocumentNumber, doc.Header);
                        
                }

            }




            webRefreshNotifications.Add(new WebNotifications()
            {
                userName = user.Name.ToLower(),
                message = "refreshMain"
            });

            webRefreshNotifications.Add(new WebNotifications()
            {
                userName = user.Name.ToLower(),
                message = "refreshGrid:Notifications"
            });

            string LinkText = string.Empty;

            if(doc is Instruction)
            LinkText = String.Format("/Document/GetInstruction?InstructionId={0}", doc.Id); 
            else
               LinkText = String.Format("/Document/GetDocument?DocumentId={0}", doc.Id); 



            return new Notifications() { ForWho = user, Text = Text, LinkText = LinkText, DocumentId = doc.Id, Id = Guid.NewGuid() };
        }


        public static void AddToViewersRecursivelyForInstruction(Instruction doc, User user, List<WebNotifications> webRefreshNotifications, bool isAuthor = false, bool isFromAddViewer = false)
        {
            var realDoc = doc;
            
                while (true)
                {
                    if (RepositoryFactory.GetRepository<Instruction>().GetListCount(m => m.Id == realDoc.ParentDocumentId) > 0)
                    {
                        realDoc = RepositoryFactory.GetRepository<Instruction>().Single(m => m.Id == realDoc.ParentDocumentId);
                        AddToViewers(realDoc, doc.Controller, webRefreshNotifications, isAuthor, isFromAddViewer);
                        AddToViewers(realDoc, user, webRefreshNotifications, isAuthor, isFromAddViewer);
                        DL.Repositories.RepositoryFactory.GetRepository<Instruction>().update(realDoc);
                    }
                    else
                        if (RepositoryFactory.GetRepository<Document>().GetListCount(m => m.Id == realDoc.ParentDocumentId) > 0)
                        {                 
                            var documentik = RepositoryFactory.GetRepository<Document>().Single(m => m.Id == realDoc.ParentDocumentId);
                            AddToViewers(documentik, user, webRefreshNotifications, isAuthor, isFromAddViewer);
                            AddToViewers(documentik, doc.Controller, webRefreshNotifications, isAuthor, isFromAddViewer);
                            DL.Repositories.RepositoryFactory.GetRepository<Document>().update(documentik);
                            break;
                        }
                        else
                        {
                            break;
                        }
                }
            
        }

        public static void AddToViewers(Document doc, User user, List<WebNotifications> webRefreshNotifications, bool isAuthor = false, bool isFromAddViewer = false)
        {           
            
            if (!doc.DocumentViewers.ContainsKey(user.UserId.ToString()))
            {

                DateTime? viewdate = null;
                if (isAuthor)
                    viewdate = DateTime.Now;
                doc.DocumentViewers.Add(user.UserId.ToString(), new DocumentViewers() { Date = DateTime.Now.Date, Time = DateTime.Now, ViewDateTime = viewdate });
                if (doc.NewDocumentViewers == null)
                 doc.NewDocumentViewers = new List<DocumentViewer>();

                doc.NewDocumentViewers.Add(new DocumentViewer() { Date = DateTime.Now.Date, Time = DateTime.Now, ViewDateTime = viewdate, UserId = user.UserId });
            }
            else
            {
                doc.DocumentViewers[user.UserId.ToString()].ViewDateTime = null;

                if (doc.NewDocumentViewers == null)
                    doc.NewDocumentViewers = new List<DocumentViewer>();

                doc.NewDocumentViewers.Where(m=>m.UserId == user.UserId).ToList().ForEach(k =>
                {
                    k.ViewDateTime = null;
                });
            }



            webRefreshNotifications.Add(new WebNotifications()
            {
                userName = user.Name.ToLower(),
                message = "refreshMain"
            });

            webRefreshNotifications.Add(new WebNotifications()
            {
                userName = user.Name.ToLower(),
                message = String.Format("refreshGrid:{0}", doc.DocumentType.Id.ToString())
            });

            webRefreshNotifications.Add(new WebNotifications()
            {
                userName = user.Name.ToLower(),
                message = String.Format("refreshMainGrid", doc.DocumentType.Id.ToString())
            });





        }


        public static  List<WebNotifications> SendInstruction(Guid docId , Instruction doc,Guid RootDocumentId, List<Notifications> notifications, bool isForInstruction=false, Document tmpRealDoc = null)
        {
            Document realDoc = null;

            if (tmpRealDoc == null)
            {
                if (!isForInstruction)
                    realDoc = RepositoryFactory.GetDocumentRepository().Single(m => m.Id == docId);
                else
                    realDoc = RepositoryFactory.GetRepository<Instruction>().Single(m => m.Id == docId);
            }
            else
            {
                realDoc = tmpRealDoc;
            }

            doc.RootDocumentId = RootDocumentId;

            if (!isForInstruction)
                doc.DocumentNumber = String.Format("к документу {0} № {1}", realDoc.DocumentType.Name, realDoc.DocumentNumber);
            else
                doc.DocumentNumber = realDoc.DocumentNumber;

            realDoc.DocumentViewers.Keys.ToList().ForEach(x =>
            {
                if (doc.DocumentViewers == null) { doc.DocumentViewers = new Dictionary<string, DocumentViewers>(); }
                doc.DocumentViewers.Add(x, realDoc.DocumentViewers[x]);

                if (doc.NewDocumentViewers == null)
                    doc.NewDocumentViewers = new List<DocumentViewer>();

                doc.NewDocumentViewers.Add(new DocumentViewer()
                {
                    Date = realDoc.DocumentViewers[x].Date, 
                    Time = realDoc.DocumentViewers[x].Time,
                    UserId = new Guid(x),
                    ViewDateTime = realDoc.DocumentViewers[x].ViewDateTime
                });
            });

            var tmpRealDocStage = realDoc.DocumentSignStages.Where(m => m.Id == realDoc.CurentStageId).FirstOrDefault();
            var tmpReallDocuser = tmpRealDocStage.RouteUsers.Where(m => m.Id == realDoc.CurrentStageUserId).FirstOrDefault();


            if (tmpReallDocuser.Instructions == null) { tmpReallDocuser.Instructions = new List<Guid>(); }
            tmpReallDocuser.Instructions.Add(doc.Id);

            if(tmpRealDoc == null)
            if (!isForInstruction)
                RepositoryFactory.GetDocumentRepository().update(realDoc);
            else
                RepositoryFactory.GetRepository<Instruction>().update(realDoc as Instruction);

            List<WebNotifications> webNotifications = new List<WebNotifications>();

            DocumentRouting.AddToViewersRecursivelyForInstruction(doc, doc.UserFor, webNotifications);

           // webNotifications.ForEach(m => { SignalRWebNotifierHelper.SendToRefreshMainMenu(m.message, m.userName); });


            //webNotifications = new List<WebNotifications>();
            notifications.AddRange(DocumentRouting.CreateRouteStagesForDocument(doc, RepositoryFactory.GetRepository<User>().Single(m => m.UserId == RepositoryFactory.GetCurrentUser()), webNotifications));

            //webNotifications.ForEach(m => { SignalRWebNotifierHelper.SendToRefreshMainMenu(m.message, m.userName); });

            notifications.ForEach(m =>
            {
                m.Id = Guid.NewGuid();
                BL.SMTP.SMTPHelper.sendMessage(m);
                //RepositoryFactory.GetRepository<Notifications>().Insert(m);                
            });

            RepositoryFactory.GetRepository<Instruction>().Insert(doc);

            return webNotifications;
        }

    }
}
