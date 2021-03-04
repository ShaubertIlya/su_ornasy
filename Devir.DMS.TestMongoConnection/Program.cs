using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.DocumentTemplates;
using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Models.References.DynamicReferences;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Models.Settings;
using Devir.DMS.DL.MongoHelpers;
using Devir.DMS.DL.Repositories;
using Devir.DMS.TestMongoConnection.Model;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.IO;
using EPocalipse.IFilter;
using Devir.DMS.Web.Models;
using Devir.DMS.DL.Models.FileStorage;
using Devir.DMS.DL.Models.Document.Route;
using Devir.DMS.Web.FullTextSearch;


namespace Devir.DMS.TestMongoConnection
{
    class Program
    {




        // Access thread-sensitive resources.


        public static Guid getGuid()
        {

            return Guid.NewGuid();

        }


        static void SaveInstructionRecursively(Guid InstructionId, Guid docId)
        {
            var tmpIsnt = RepositoryFactory.GetInstructionRepository().Single(x => x.Id == InstructionId);

            foreach (var ds1 in tmpIsnt.DocumentSignStages)
                foreach (var ru1 in ds1.RouteUsers)
                    if (ru1.Instructions != null)
                        foreach (var inst1 in ru1.Instructions)
                        {

                            var tmpIsnt1 = RepositoryFactory.GetInstructionRepository().Single(x => x.Id == inst1);
                            if (tmpIsnt1 != null)
                            {
                                tmpIsnt1.RootDocumentId = docId;
                                RepositoryFactory.GetInstructionRepository().update(tmpIsnt1);
                                SaveInstructionRecursively(inst1, docId);
                            }


                        }
        }

        static public bool BelonsToDocument(Guid instructionId, Document doc)
        {
            bool isTrue = false;

            foreach (var stage in doc.DocumentSignStages)
            {
                foreach (var user in stage.RouteUsers)
                {
                    if (user.Instructions != null)
                    {
                        if (user.Instructions.Contains(instructionId))
                            isTrue = true;
                        else
                        {
                            user.Instructions.ForEach(m =>
                            {
                                var tmpInstr = RepositoryFactory.GetInstructionRepository().Single(x => x.Id == m);
                                if(tmpInstr!=null)
                                if (BelonsToDocument(instructionId, tmpInstr))
                                {
                                    isTrue = true;
                                }
                            });
                        }
                    }
                }
            }


            return isTrue;

        }


        static void Main(string[] args)
        {




            //Настройка делегатта для получения GUID пользователя
            RepositoryFactory.GetCurrentUser = () =>
            {
                //Сюда код для получения GUID текущего пользователя
                //Для примера я просто генерирую GUID
                return new Guid("7c432691-5359-4fcf-b7f6-43f3f7f8bbb4");
            };

            var userId = new Guid("c75dbf6b-4673-46ce-8fea-a4a4264300a5");
            var sortDirection = 0;
           var query = Query.And(
                    Query.NE("Author.UserId", userId),
                    Query.EQ("NewDocumentViewers.UserId", userId),
                    Query.ElemMatch("NewDocumentViewers", Query.EQ("UserId", userId)),
                    Query.In("docState", new BsonArray(new Object[] { DocumentState.InWork, DocumentState.FinishedOk }))
                    );

           query = Query.Or(
                   query,
                    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")),
                   Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e994583d-2ef8-4368-9ed5-5f4439374197")),
                   Query<Document>.EQ(m => m.DocumentType.Id, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
                   Query<Document>.EQ(m => m.DocumentType.Id, new Guid("852ff790-6ce8-4ca9-b868-f5bedd6c90e4")),
                   Query<Document>.EQ(m => m.DocumentType.Id, new Guid("8ad9bf84-cf30-4f3b-848b-e566311f03fc"))
                   );

           var sortOrder = sortDirection == 0 ? SortBy.Descending("NewDocumentViewers.Date") : SortBy.Ascending("NewDocumentViewers.Date");


            var a = RepositoryFactory.GetDocumentRepository().GetCollection().FindAs<Document>(query).SetSortOrder(sortOrder).SetSkip(40).SetLimit(20).Explain();


            return;
            
            //var db = new Eloquera.Client.DB("server=localhost:43962;user=root;password=root;options=none");
             //db.OpenDatabase("Document");
            
            //db.RefreshMode = ObjectRefreshMode.AlwaysReturnUpdatedValues;
            //var selectedList = RepositoryFactory.GetDocumentRepository().List(m => !m.isDeleted && m.isUrgent).ToList();
            //Console.WriteLine("Selected, Inserting");
            //selectedList.ForEach(m =>
            //{
            //    Console.WriteLine(m.DocumentNumber);
            //    Console.WriteLine(db.Store(m));
            //});
            //Consoldb.Query<Document>().Count(m => !m.isDeleted)e.WriteLine("Inserted");
            //
            //Console.WriteLine("Selecting");
            //db.Query<Document>().Where(m => m.NewDocumentViewers.Count(n => n.UserId != userId)>0).Take(40).ToList().ForEach(m=>
            //{
            // Console.WriteLine(m.DocumentNumber);   
            //});
            

            // db.Close();
            Console.WriteLine("Ready");

            Console.ReadKey();
            return;
           


            Console.WriteLine("Starting with documents");
            RepositoryFactory.GetDocumentRepository().List(m=>!m.isDeleted).ToList().ForEach(m =>
            {
                m.NewDocumentViewers = new List<DocumentViewer>();
                m.DocumentViewers.ToList().ForEach(n => m.NewDocumentViewers.Add(new DocumentViewer()
                {
                    Date = n.Value.Date,
                    Time = n.Value.Time,
                    UserId =  new Guid(n.Key),
                    ViewDateTime = n.Value.ViewDateTime
                }));
                Console.WriteLine("Документ: {0}", m.DocumentNumber);
                RepositoryFactory.GetDocumentRepository().update(m);
            });

            Console.WriteLine("Starting with instructions");
            RepositoryFactory.GetInstructionRepository().List(m => !m.isDeleted).ToList().ForEach(m =>
            {
                m.NewDocumentViewers = new List<DocumentViewer>();
                m.DocumentViewers.ToList().ForEach(n => m.NewDocumentViewers.Add(new DocumentViewer()
                {
                    Date = n.Value.Date,
                    Time = n.Value.Time,
                    UserId = new Guid(n.Key),
                    ViewDateTime = n.Value.ViewDateTime
                }));
                Console.WriteLine("Поручение: {0}", m.DocumentNumber);
                RepositoryFactory.GetInstructionRepository().update(m);
            });

            Console.WriteLine("Ready");
            Console.ReadKey();
            return;

            var doc_type = RepositoryFactory.GetRepository<DocumentType>().Single(m => m.Name == "Исходящий документ");


            


            //var depList =  RepositoryFactory.GetRepository<Department>().List(x => !x.isDeleted);

            //depList.ToList().ForEach(m =>
            //{
            //    if (!String.IsNullOrEmpty(m.Code))
            //    {
            //        m.Users.ToList().ForEach(n =>
            //        {
            //            n.Key.Nomenclature = m.Code;

            //            var tmpUser = RepositoryFactory.GetRepository<User>().Single(h=>h.UserId == n.Key.UserId);

            //            tmpUser.Nomenclature = m.Code;

            //            RepositoryFactory.GetRepository<User>().update(tmpUser);

            //        });

                    
            //    }

            //    RepositoryFactory.GetRepository<Department>().update(m);
            //});

          
                //using (ServiceFTSClient clnt = new ServiceFTSClient())
                //{
                //    RepositoryFactory.GetDocumentRepository().List(f => !f.isDeleted).ToList().ForEach(m =>
                //    {
                //        try
                //        {
                //            clnt.SaveDocument(m.Id);
                //            Console.WriteLine("Проиндексировали " + m.DocumentNumber);
                //        }
                //        catch (Exception ex)
                //        {
                //            Console.WriteLine(ex.ToString());
                //        }

                //    });
                //}
           
            Console.WriteLine("Закончили");
            Console.ReadKey();

            //Console.WriteLine("Получение поручений");
            //var tmpListofInstructions = RepositoryFactory.GetInstructionRepository().List(m => m.CreateDate > DateTime.Now.AddDays(-1)).ToList();
            //Console.WriteLine("Получили поручения");

            //tmpListofInstructions.ForEach(m =>
            //{
            //    var doc = RepositoryFactory.GetDocumentRepository().Single(x => x.Id == m.RootDocumentId);
            //    if (!doc.DocumentViewers.Keys.Any(f=>f == m.Controller.UserId.ToString())) { 
            //    doc.DocumentViewers.Add(m.Controller.UserId.ToString(), new DocumentViewers() { Date = DateTime.Now.Date, Time = DateTime.Now, ViewDateTime = null });
            //    }
            //    RepositoryFactory.GetDocumentRepository().update(doc);
            //});


            //Console.WriteLine("Закончили ворк");

            //Console.ReadKey();
            //return;

            //tmpListofInstructions.ForEach(m =>
            //{
            //    if (m.ParentDocumentId == m.RootDocumentId)
            //    {
            //        var tmpDoc = RepositoryFactory.GetDocumentRepository().Single(x => x.Id == m.RootDocumentId);

            //        if (!tmpDoc.DocumentSignStages.Any(x => x.RouteUsers.Any(x1 => x1.Instructions!=null && x1.Instructions.Contains(m.Id))))
            //        {
            //            Console.WriteLine("В {0} №{1} нет {2}", tmpDoc.DocumentType.Name, tmpDoc.DocumentNumber, m.DocumentNumber);

            //            var stage = tmpDoc.DocumentSignStages.Where(x => x.RouteTypeId == new Guid("ace11fae-204e-40af-bdd0-a686395390e6")).FirstOrDefault();
            //            if(stage!= null)
            //            { 
            //            Console.WriteLine("В документе существует стадия исполнения поручений, количество пользователей:{0}", stage.RouteUsers.Count());

            //            var tmpRouteUser = new RouteStageUser()
            //        {
            //            Id = Guid.NewGuid(),
            //            Instructions = new List<Guid>(),
            //            IsCurent = true,
            //            Order = 0,
            //            SecondChanceForId = null,
            //            SignResult = null,
            //            SignUser = m.UserFor,
            //            UsersActions = new List<RouteAction>()
            //        };
            //            tmpRouteUser.Instructions.Add(m.Id);

            //            stage.RouteUsers.Add(tmpRouteUser);

            //            //RepositoryFactory.GetDocumentRepository().update(tmpDoc);

            //            Console.WriteLine("Обновили документ");
            //            }
            //            else
            //            {
            //                Console.WriteLine("В документе нет стадии исполнения пороучений");
            //            }
            //        }

            //    }

            //});

            //Console.WriteLine("закончили обработку");
            //Console.ReadKey();

            return;


            //using (ServiceFTSClient clnt = new ServiceFTSClient())
            //{
            //    RepositoryFactory.GetDocumentRepository().List(m => !m.isDeleted).ToList().ForEach(m =>
            //    {
            //        try
            //        {
            //            clnt.SaveDocument(m.Id);
            //            Console.WriteLine("Проиндексировали " + m.DocumentNumber);
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex.ToString());
            //        }

            //    });
            //}
           

            //RepositoryFactory.GetInstructionRepository().List(m => !m.isDeleted).ToList().ForEach(m =>
            //{
            //    var tmpDocList = RepositoryFactory.GetDocumentRepository().List(n => n.Id == m.RootDocumentId).ToList();
            //    var isForDelete = true;
            //    if (!tmpDocList.Any())
            //        Console.WriteLine("Совсем сирота: " + m.DocumentNumber);
            //    else
            //    {
            //        foreach (var doc in tmpDocList)
            //        {
            //            if (BelonsToDocument(m.Id, doc))
            //            {
            //                isForDelete = false;
            //            }
            //        }
            //        if (isForDelete)
            //            Console.WriteLine("На удаление!!!:" + m.DocumentNumber);

            //    }


            //    //if (isForDelete)
            //    //    RepositoryFactory.GetNoAuditRepository<Instruction>().Delete(m.Id);
            //});

            //RepositoryFactory.GetDocumentRepository().List(m => !m.isDeleted).ToList().ForEach(m =>
            //{
            //    if (m.docState == DocumentState.FinishedOk)
            //    {
            //        RepositoryFactory.GetInstructionRepository().List(n => n.RootDocumentId == m.Id && n.docState != DocumentState.FinishedOk).ToList().ForEach(n =>
            //        {
            //            if (n.DocumentSignStages.All(x => x.RouteUsers.All(y => y.SignResult != null)))
            //                Console.WriteLine("Поручение:" + n.DocumentNumber);
            //        });
            //    }
            //});
            Console.WriteLine("Finished");
            Console.ReadKey();
            return;




            //RepositoryFactory.GetDocumentRepository().List(m => !m.isDeleted).ToList().ForEach(m =>
            //{

            //});



            //var alldocs = RepositoryFactory.GetDocumentRepository().GetCollection().Find(Query.And(Query<Document>.EQ(m => m.DocumentType.Id, new Guid("e993583d-2ef8-4368-9ed5-5f4439374174")), Query<Document>.GT(m => m.CreateDate, new DateTime(2014, 03, 31, 00, 00, 00)))).OrderBy(m => m.CreateDate).ToList();

            //alldocs.ForEach(m =>
            //{
            //     int number =0;
            //    var oldDocnumber = m.DocumentNumber;

            //    if (int.TryParse(m.DocumentNumber, out number)) {                 

            //    var newNumber = (Convert.ToInt32(m.DocumentNumber) - 2).ToString();
            //    m.DocumentNumber = newNumber;

            //    RepositoryFactory.GetInstructionRepository().GetCollection().Find(Query<Instruction>.EQ(n => n.RootDocumentId, m.Id)).ToList().ForEach(i =>
            //    {
            //        i.DocumentNumber.Replace(oldDocnumber, newNumber);
            //        RepositoryFactory.GetNoAuditRepository<Instruction>().update(i);
            //    });
            //    RepositoryFactory.GetNoAuditRepository<Document>().update(m);
            //        }
            //});

            //Console.ReadKey();


            //using (ServiceFTSClient clnt = new ServiceFTSClient())
            //{
            //    RepositoryFactory.GetDocumentRepository().List(m => !m.isDeleted).ToList().ForEach(m =>
            //    {    
            //        try
            //        {
            //            clnt.SaveDocument(m.Id);
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex.ToString());
            //        }

            //    });
            //}          

            //return;



            //RepositoryFactory.GetDocumentRepository().List(m => m.isDeleted == false).ToList().ForEach(m =>
            //{
            //    for (int i = 0; i < 100; i++)
            //    {
            //        m.Id = Guid.NewGuid();
            //        RepositoryFactory.GetDocumentRepository().Insert(m);
            //    }

            //});

            //return;


            //int docNum = 914;

            //RepositoryFactory.GetRepository<Document>().GetCollection().Find(
            //    Query.And(
            //    Query<Document>.EQ(m => m.DocumentType.Id, new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")),
            //    Query.Not(Query<Document>.EQ(m => m.DocumentNumber, null))
            //    )).SetSortOrder(SortBy<Document>.Ascending(m => m.CreateDate)).ToList().ForEach(m =>
            //    {
            //        m.DocumentNumber = m.Author.Nomenclature+"/"+docNum.ToString();
            //        docNum++;
            //        RepositoryFactory.GetDocumentRepository().update(m);
            //    });

            //Console.WriteLine(docNum);
            //Console.ReadKey();

            //return;

            //RepositoryFactory.GetRepository<User>().Single(m=>m.LastName)


            //var cnt = RepositoryFactory.GetRepository<Department>().GetCollection().Find(
            //   Query.EQ("Users.LastName", "Бейсембаев")
            //    ).Count();



            //return;


            //var instruction = RepositoryFactory.GetInstructionRepository().Single(m => m.Id == new Guid("fc0031ee-0ce8-4267-affe-21358eabc692"));

            //var document = RepositoryFactory.GetDocumentRepository().Single(m => m.Id == new Guid("aebd78e6-2256-4de6-ab9d-6bd57e78e593"));

            //List<Guid> instrs = new List<Guid>();
            //instrs.Add(instruction.Id);

            //document.DocumentSignStages.LastOrDefault().RouteUsers.Add(new RouteStageUser()
            //                {
            //                    Id = new Guid(),
            //                    Instructions = instrs,
            //                    IsCurent = true,
            //                    Order = 0,
            //                    SecondChanceForId = null,
            //                    SignResult = null,
            //                    SignUser = instruction.UserFor,
            //                    UsersActions = new List<RouteAction>()
            //                });

            //RepositoryFactory.GetDocumentRepository().update(document);

            //return; 


            //   using (ServiceFTSClient clnt = new ServiceFTSClient())
            //    {
            //RepositoryFactory.GetDocumentRepository().List(m =>m.DocumentSignStages != null).ToList().ForEach(m =>
            //{
            //    RepositoryFactory.GetDocumentRepository().update(m);


            //        try
            //        {
            //            clnt.SaveDocument(m.Id);
            //        }
            //        catch (Exception ex)
            //        {

            //        }             

            //});
            //    }

            //Console.WriteLine("Обновили все документы!");

            //RepositoryFactory.GetRepository<FileStorage>().List(m => !m.isDeleted).ToList().ForEach(m =>
            //{
            //    m.IsPDFWorkerCompleted = true;
            //    RepositoryFactory.GetRepository<FileStorage>().update(m);
            //}
            //    );

            ////List<Guid> instrList = new List<Guid>();

            //var doc  = RepositoryFactory.GetInstructionRepository().Single(m => m.Id == new Guid("891a099d-4bcd-40f0-ae9a-731a9a729fdb"));
            //Console.WriteLine("Приступаем к поручениям");

            //RepositoryFactory.GetDocumentRepository().List(m => !m.isDeleted).ToList().ForEach(m =>
            //{

            //    foreach (var ds in m.DocumentSignStages)
            //        foreach (var ru in ds.RouteUsers)
            //            if (ru.Instructions != null)
            //                foreach (var inst in ru.Instructions)
            //                {

            //                    //instrList.Add(inst);

            //                    SaveInstructionRecursively(inst, m.Id);

            //                    //tmpIsnt.RootDocumentId = m.Id;
            //                    //RepositoryFactory.GetInstructionRepository().update(tmpIsnt);
            //                }

            //});

            //Console.WriteLine("Закончили поручения");

            //Console.WriteLine("Получаем все гуиды документов");
            //var DocumentIds = RepositoryFactory.GetDocumentRepository().GetCollection().Find(Query<Document>.EQ(m => m.isDeleted, false)).SetFields(new FieldsBuilder<Document>().Include(m => m.Id)).Select(m=>m.Id).ToList();


            //Console.WriteLine("Удаляем все неправильные поручения");
            //int i = 0;
            //RepositoryFactory.GetInstructionRepository().List(m =>!m.isDeleted && !DocumentIds.Contains(m.RootDocumentId)).ToList().ForEach(m => {
            //    i++;
            //    RepositoryFactory.GetInstructionRepository().Delete(m.Id);                
            //});
            //Console.WriteLine("Удалили {0} поручений", i);




            //var ParentDocumentId = RepositoryFactory.GetInstructionRepository().Single(m => m.Id == new Guid("fb6a0271-9ed1-4f85-a829-d4fa1c3d10f8")).ParentDocumentId;

            //if(RepositoryFactory.GetDocumentRepository().GetListCount(m=>m.Id==ParentDocumentId)>0)
            //{
            //    Console.WriteLine("Найден документ в основной БД, все в порядке.");
            //}
            //else {

            //    Console.WriteLine("Не найден документ в основной БД, ищем в аудите.");

            //MongoHelper.Database.GetCollection("Document_audit").FindAs<Document>(Query<Document>.EQ(m => m.Id, ParentDocumentId)).ToList().ForEach(m =>
            //{
            //    Console.WriteLine("Нашли в бд аудита документ с номером: {0}",m.DocumentNumber);

            //    Console.WriteLine("Был ID: {0}, Стал ID: {1}", ParentDocumentId, RepositoryFactory.GetDocumentRepository().Single(n => n.DocumentNumber == m.DocumentNumber).Id);

            //});

            //}





            Console.WriteLine("гОтово");

            //MongoHelper.connect("mongodb://192.168.1.226:27017/", "DMS");

            //MongoHelper.Database.GetCollection("Document_audit").FindAs<Document>(Query<Document>.EQ(m=>m.Id,  new Guid("e14d4eab-0621-4e6e-b751-7e8063730f6b"))).ToList().ForEach(m =>{
            //  Console.WriteLine(m.DocumentNumber);
            //});

            //RepositoryFactory.GetInstructionRepository().List(m => !instrList.Contains(m.Id)).ToList().ForEach(m =>
            //{
            //    Console.WriteLine(m.Id);
            //});

            //Console.WriteLine(RepositoryFactory.GetInstructionRepository().Single(m => m.DocumentNumber == "20").ParentDocumentId);
            //Console.WriteLine(RepositoryFactory.GetDocumentRepository().Single(m => m.Id == new Guid("e14d4eab-0621-4e6e-b751-7e8063730f6b")).DocumentNumber);



            //TextReader reader = new FilterReader("E:\\1.docx");
            //using (reader)
            //    Console.WriteLine(reader.ReadToEnd());

            //MongoHelper.connect("mongodb://192.168.1.226:27017/", "DMS");

            //List<Int16> NewIDS = new List<short>();

            //MongoHelper.Database.GetCollection("Document").FindAllAs<Document>().ToList().ForEach(m =>
            //{

            //    var tmpDocAud=MongoHelper.Database.GetCollection("Document_audit").FindAs<Document>(Query<Document>.EQ(x => x.DocumentNumber, m.DocumentNumber)).OrderBy(x => x.CreateDate).FirstOrDefault();

            //    if(tmpDocAud.Id != m.Id)
            //    {
            //        Console.WriteLine("Ид не совпадают");
            //    }


            //    var tmpViewModelsList = MongoHelper.Database.GetCollection("DocumentViewModelDB_audit").FindAs<DocumentViewModelDB>(Query<DocumentViewModelDB>.EQ(n => n.docId, tmpDocAud.Id)).OrderByDescending(n => n.CreateDate).ToList();

            //    DocumentViewModelDB tmpViewModel = null;
            //    if (tmpViewModelsList.Where(x => x.ViewModel.instructions != null).Count() > 0)
            //        tmpViewModel = tmpViewModelsList.Where(n=>n.ViewModel.instructions != null).OrderByDescending(n => n.CreateDate).FirstOrDefault();
            //    else
            //        tmpViewModel = tmpViewModelsList.OrderByDescending(n => n.CreateDate).FirstOrDefault();

            //    tmpViewModel.docId = m.Id;
            //    tmpViewModel.ViewModel.ParentDoc = m.Id;

            //    MongoHelper.Database.GetCollection("DocumentViewModelDB").Insert(tmpViewModel);

            //    Console.WriteLine("Документ №{0} Количество instructions: {1}", m.DocumentNumber, tmpViewModel.ViewModel.instructions.Count());



            //});


            //NewIDS.OrderBy(m=>m).ToList().ForEach(m => Console.Write(" {0}", m));



            //AddSettings();

            //RepositoryFactory.GetRepository<RouteType>().Insert(new RouteType { Id = new Guid("ACE11FAE-204E-40AF-BDD0-A686395390E6"), Name = "Исполнение поручений", Comment = "Исполнение поручений" });
            // AddSettings();
            //FirstInit();
            //AddRoutes();

            //var rep = RepositoryFactory.GetRepository<Department>();
            //rep.Insert(new Department()
            //{
            //    Name = "Отдел IT",
            //    ParentDepertmentId = null,
            //    OU = "IT"
            //});

            //RepositoryFactory.GetRepository<RouteType>().Insert(new RouteType(){ Name = "Ознакомление", Comment="Для ознакомления пользователей", Actions = new List<RouteAction>()});


            //RepositoryFactory.GetRepository<Doc>().Insert(new Doc()
            //{
            //    Name = "Проверка",
            //    Value = new FieldType()
            //{
            //    Id = new Guid("a427dbfb-9cb7-4f52-9d5e-c7d0677e8103"),
            //    Name = "Строка",
            //    Comment = "Для хранения строковых данных",
            //    SystemName = "System.String"
            //}
            //});


            //var document = RepositoryFactory.GetRepository<Document>().GetCollection(false).Find(Query.EQ("DocumentNumber", "12")).ToList().FirstOrDefault();
            //document.Id = Guid.Empty;
            //RepositoryFactory.GetRepository<Document>().GetCollection(true).Insert(document);



            //for (var i = 0; i < 12; i++)
            //{
            //    var document = RepositoryFactory.GetRepository<Document>().GetCollection(false).Find(Query.EQ("DocumentNumber", "11")).ToList().FirstOrDefault();
            //    Task task = new Task(() =>
            //    {
            //        try
            //        {
            //            for (var j = 0; j < 9000*1000; j++)
            //            {
            //                document.Id = Guid.Empty;
            //                RepositoryFactory.GetRepository<Document>().GetCollection(true).Insert(document);
            //            }
            //        }
            //        catch(Exception ex){
            //            Console.WriteLine(ex.ToString());
            //        }                    
            //    });
            //    task.Start();                
            //}

            //Console.WriteLine("Идет работа.... ничего не трогать!!!");
            //Console.ReadKey();





            //RepositoryFactory.GetRepository<User>().GetListCount(m => m.isDeleted);

            //var start = DateTime.Now;
            //var List = RepositoryFactory.GetRepository<Document>().GetCollection(true).AsQueryable().Select(m => m.DocumentNumber).ToList();

            //var cur = RepositoryFactory.GetRepository<Document>().GetCollection(true).FindAll();
            //cur.SetFields(Fields.Include("DocumentNumber"));

            //var list = cur.Select(m => m.DocumentNumber).ToList();

            //Console.WriteLine("Времени затрачено:{0}", DateTime.Now - start);
            //Console.ReadKey();

            //dynamic d = new ExpandoObject();

            //        d.i = 5;
            //        d.j=4;
            //        d.Name="Toxa";



            //RepositoryFactory.GetRepository<Doc>().Insert(new Doc()
            //{
            //    Name = "Хрень",
            //    //Value = new List<int>()
            //    //{
            //    //    1,2,3,4,5
            //    //}
            //    Value = d
            //});

            //var dict = new Dictionary<string, object>();
            //dict.Add("ОдынОдынОдын", new List<int>()
            //    {
            //        1,2,3,4,5
            //    });
            // dict.Add("ДываДываДыва", new FieldType()
            //{
            //    Id = new Guid("a427dbfb-9cb7-4f52-9d5e-c7d0677e8103"),
            //    Name = "Строка",
            //    Comment = "Для хранения строковых данных",
            //    SystemName = "System.String"
            //});



            //RepositoryFactory.GetRepository<Doc>().Insert(new Doc()
            //{
            //    Name = "Составной один",
            //    Value = dict
            //    }
            //);


            //BsonClassMap.RegisterClassMap<FieldType>();

            //var s = RepositoryFactory.GetRepository<Doc>().GetCollection().Find(Query.EQ("Value._v.ДываДываДыва.Name", "Строка")).ToList();



            //AddSettings();





            Console.ReadKey();

        }

        private static void FirstInit()
        {


            //RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
            //{
            //    Id = new Guid("9b7be5b0-3e69-466a-a876-eae0402ebbe7"),
            //    Name = "Пользователь",
            //    Comment = "Для составления маршрутов",
            //    SystemName = "System.User"
            //});

            RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
            {
                Id = new Guid("a427dbfb-9cb7-4f52-9d5e-c7d0677e8103"),
                Name = "Строка",
                Comment = "Для хранения строковых данных",
                SystemName = "System.String"
            });

            RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
            {
                Id = new Guid("f23165db-7c3d-49d5-bbc0-127eef90de36"),
                Name = "Текст",
                Comment = "Для хранения текста с форматированием",
                SystemName = "System.String"
            });

            RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
            {
                Id = new Guid("8a37142c-0e29-4b40-b4a3-0a3a7d4f21d9"),
                Name = "Целочисленное",
                Comment = "Для хранения целочесленных данных",
                SystemName = "System.int"
            });

            RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
            {
                Id = new Guid("2490becb-3476-43ab-8717-0f0b138a6ab2"),
                Name = "Булево",
                Comment = "Для хранения булевых данных",
                SystemName = "System.bool"
            });

            RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
            {
                Id = new Guid("944388a1-b1e3-4a4d-910d-7ad9df107e20"),
                Name = "Числовой",
                Comment = "Для хранения чисел с плавающей точкой",
                SystemName = "System.decimal"
            });

            //RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
            //{
            //    Id = new Guid("b33f3a3c-e9dd-4fb3-9b0a-00a79870954a"),
            //    Name = "Пользователь для маршрута",
            //    Comment = "Для хранения пользователей с последующим исполльзованием для состоваления маршрута",
            //    SystemName = "Devir.DMS.DL.Models.References.OrganizationStructure.User"
            //});

            RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
            {
                Id = new Guid("0ac58ff0-54de-4d33-b49a-52412fff51dd"),
                Name = "Должности",
                Comment = "Для хранения должностей сотрудников",
                SystemName = "Devir.DMS.DL.Models.References.OrganizationStructure.Post"
            });

            RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
            {
                Id = new Guid("9b7be5b0-3e69-466a-a876-eae0402ebbe7"),
                Name = "Пользователь",
                Comment = "Для составления маршрута",
                SystemName = "Devir.DMS.DL.Models.References.OrganizationStructure.User"
            });

            RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
            {
                Id = new Guid("d88f464a-ca95-4c41-ad7d-7df5adfd90d8"),
                Name = "Дата и время",
                Comment = "Для хранения даты и времени",
                SystemName = "System.DateTime"
            });


            RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
            {
                Id = new Guid("2c308153-04a9-4bf6-b021-cc28b82a7ab5"),
                Name = "Список пользователей",
                Comment = "Для составления маршрутов",
                SystemName = ""
            });

            RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
            {
                Id = new Guid("e3224442-d53a-47e9-b1bb-495c034b10d8"),
                Name = "Согласующие (Конструктор)",
                Comment = "Для составления маршрутов",
                SystemName = ""
            });

            #warning Антон, добавился новый тип поля!
            RepositoryFactory.GetRepository<FieldType>().Insert(new FieldType()
            {
                Id = new Guid("398877ee-49f3-46b6-bc2e-f567ecd75410"),
                Name = "Входящий номер",
                Comment = "Для хранения номеров входящих документов",
                SystemName = "System.String"
            });




        }

        private static void AddSettings()
        {
            RepositoryFactory.GetRepository<Settings>().Insert(new Settings()
            {
                NegotiationStage = new Guid("b932b534-29f4-4099-bf92-020c271ef18c"),
                SignStage = new Guid("145e6ca4-a973-4b52-98ab-1f2c42b8a897"),
                PerformStage = new Guid("fbba25c0-d74f-4007-8edb-ec6e38e2486d"),
                AdditionalNegotiationRequired = new Guid("3e7bfb59-26a4-4b70-b2df-d303dfcb2375"),
                AdditionalPerformRequired = new Guid("45823b21-9e15-4eb0-9ae0-e974c479be74"),
                ListUsersFieldType = new Guid("2c308153-04a9-4bf6-b021-cc28b82a7ab5"),
                UserFieldType = new Guid("9b7be5b0-3e69-466a-a876-eae0402ebbe7"),
                OkNegotiationAction = new Guid("b3dbd6e1-98e0-4ac3-b6c0-af875342e7f4"),
                NotOkNegotiationAction = new Guid("ce9d0f74-80d9-46d8-aae9-36e2652f7d91"),
                OkSignAction = new Guid("c820c2cf-0f00-477d-b397-2ff630a14050"),
                NotOkSigntAction = new Guid("ce9d0f74-80d9-46d8-aae9-36e2652f7d91"),
                OkPerformAction = new Guid("b25f05fc-c36b-4663-b2d1-3ab7b42ce04b"),
                NotOkPerformAction = new Guid("ce9d0f74-80d9-46d8-aae9-36e2652f7d91"),
                ControlPerformStage = new Guid("a9674ad4-c0fb-4282-90f9-020e41c536de"),
                OkControlPerformStage = new Guid("a475e9cf-faa6-41fa-956e-1165c0eb123b"),
                NotOkControlPerformStage = new Guid("7ac2f1da-01ff-4fb8-a338-74046f3d25ec"),
                AddViewersStage = new Guid("5d57bc37-43d4-418c-ac36-6e62555b2c45"),
                PerformInstructionWaiting = new Guid("ACE11FAE-204E-40AF-BDD0-A686395390E6")
            });
        }

        private static void AddDocumentTypes()
        {
            RepositoryFactory.GetRepository<DocumentType>().Insert(new DocumentType()
            {
                Name = "Служебная записка",
                Comment = "Обычная служебная записка"
            });

            RepositoryFactory.GetRepository<DocumentType>().Insert(new DocumentType()
            {
                Name = "Заявка",
                Comment = "Обычная заявка"
            });
        }

        private static void AddDepartmentsTree()
        {
            var rep = RepositoryFactory.GetRepository<Department>();
            rep.Insert(new Department()
            {
                Name = "Администрация",
                ParentDepertmentId = null,
                OU = "IT"
            });
            rep.Insert(new Department()
            {
                Name = "Кадровый департамент",
                ParentDepertmentId = null
            });
            rep.Insert(new Department()
            {
                Name = "Департамент информационных технологий",
                ParentDepertmentId = null
            });

            var depIt = rep.Single(d => d.Name == "Департамент информационных технологий");

            rep.Insert(new Department()
            {
                Name = "Отдел разработки",
                ParentDepertmentId = depIt.Id
            });

            rep.Insert(new Department()
            {
                Name = "Финансовый департамент",
                ParentDepertmentId = null
            });
            var finDep = rep.Single(d => d.Name == "Финансовый департамент");
            rep.Insert(new Department()
            {
                Name = "Бухгалтерия",
                ParentDepertmentId = finDep.Id
            });
            rep.Insert(new Department()
            {
                Name = "Отдел бюджетного планирования",
                ParentDepertmentId = finDep.Id
            });
        }

        private static void AddOneDepartment()
        {
            var rep = RepositoryFactory.GetRepository<Department>();

            var depDev = rep.Single(d => d.Name == "Отдел разработки");

            //depDev.Users.Clear();
            var repUsers = RepositoryFactory.GetRepository<User>();
            var users = repUsers.List();
            users.ToList().ForEach(u =>
            {
                var post = RepositoryFactory.GetRepository<Post>().Single(x => x.Name == "Программист");
                depDev.Users.Add(u, post);
                u.DepartmentId = depDev.Id;
                repUsers.update(u);
            });
            rep.update(depDev);
        }

        private static void AddUsers()
        {
            var rep = RepositoryFactory.GetRepository<User>();

            rep.Insert(new User()
            {
                UserId = Guid.NewGuid(),
                FirstName = "Ержан",
                LastName = "Жуматаев",
                FatherName = "",
                BirthDate = new DateTime(1988, 02, 03),
                Citizenship = "Республика Казахстан",
                Email = "yzhumataev@gmail.com",
                IsMale = true,
                Nationality = "казах",
                Phone = "+7 777 777 77 77",
                Name = "devir\\y.zhumataev"
            });

            rep.Insert(new User()
            {
                UserId = Guid.NewGuid(),
                FirstName = "Даурен",
                LastName = "Даут",
                FatherName = "Канатулы",
                BirthDate = new DateTime(1988, 02, 03),
                Citizenship = "Республика Казахстан",
                Email = "d.daut@devir.kz",
                IsMale = true,
                Nationality = "казах",
                Phone = "+7 777 777 77 77",
                Name = "devir\\d.daut"
            });

            rep.Insert(new User()
            {
                UserId = Guid.NewGuid(),
                FirstName = "Дмитрий",
                LastName = "Климович",
                FatherName = "Рамазович",
                BirthDate = new DateTime(1988, 04, 05),
                Citizenship = "Республика Казахстан",
                Email = "d.klimovich@devir.kz",
                IsMale = true,
                Nationality = "руский",
                Phone = "+7 777 777 77 77",
                Name = "devir\\d.klimovich"
            });

            rep.Insert(new User()
            {
                UserId = Guid.NewGuid(),
                FirstName = "Антон",
                LastName = "Зоря",
                FatherName = "",
                BirthDate = new DateTime(1988, 03, 02),
                Citizenship = "Республика Казахстан",
                Email = "a.zorya@devir.kz",
                IsMale = true,
                Nationality = "руский",
                Phone = "+7 777 777 77 77",
                Name = "devir\\a.zorya"
            });

            rep.Insert(new User()
            {
                UserId = Guid.NewGuid(),
                FirstName = "Алибек",
                LastName = "Каримов",
                FatherName = "Каримович",
                BirthDate = new DateTime(1986, 11, 27),
                Citizenship = "Республика Казахстан",
                Email = "alibek.karimov@gmail.com",
                IsMale = true,
                Nationality = "казах",
                Phone = "+7 777 777 77 77",
                Name = "devir\\a.karimov"
            });
        }

        private static void AddPosts()
        {
            var rep = RepositoryFactory.GetRepository<Post>();
            rep.Insert(new Post()
            {
                Name = "Программист"
            });
        }

        private static void AddUsersToDepartments()
        {
            var repDep = RepositoryFactory.GetRepository<Department>();
            var repUsers = RepositoryFactory.GetRepository<User>();
            var post = RepositoryFactory.GetRepository<Post>().Single(x => x.Name == "Программист");

            var depDev = repDep.Single(d => d.Name == "Отдел разработки");
            foreach (var user in repUsers.List())
            {
                depDev.Users.Add(user, post);
                user.DepartmentId = depDev.Id;
                repUsers.update(user);
            }
            repDep.update(depDev);
        }

        private static void AddRoutes()
        {


            var raRep = RepositoryFactory.GetRepository<RouteAction>();
            var ra0 = new RouteAction { Id = new Guid("b25f05fc-c36b-4663-b2d1-3ab7b42ce04b"), Name = "Исполнить", Comment = "Исполнить" };
            raRep.Insert(ra0);
            var ra1 = new RouteAction { Id = new Guid("ce9d0f74-80d9-46d8-aae9-36e2652f7d91"), Name = "Отклонить", Comment = "Отклонить" };
            raRep.Insert(ra1);
            var ra2 = new RouteAction { Name = "Поручить", Comment = "Поручить" };
            raRep.Insert(ra2);
            var ra3 = new RouteAction { Name = "Вернуть", Comment = "Вернуть" };
            raRep.Insert(ra3);
            var ra4 = new RouteAction { Id = new Guid("c820c2cf-0f00-477d-b397-2ff630a14050"), Name = "Подписать", Comment = "Подписать" };
            raRep.Insert(ra4);
            var ra5 = new RouteAction { Id = new Guid("b3dbd6e1-98e0-4ac3-b6c0-af875342e7f4"), Name = "Согласовать", Comment = "Согласовать" };
            raRep.Insert(ra5);
            var ra6 = new RouteAction { Id = new Guid("3e7bfb59-26a4-4b70-b2df-d303dfcb2375"), Name = "Запросить дополнительное согласование", Comment = "дополнительное согласование" };
            raRep.Insert(ra6);
            var ra7 = new RouteAction { Id = new Guid("45823b21-9e15-4eb0-9ae0-e974c479be74"), Name = "Поручить", Comment = "поручить исполнение" };
            raRep.Insert(ra7);
            var ra8 = new RouteAction { Id = new Guid("a475e9cf-faa6-41fa-956e-1165c0eb123b"), Name = "Подтвердить исполнение", Comment = "Для подтверждения исполнения" };
            raRep.Insert(ra8);
            var ra9 = new RouteAction { Id = new Guid("7ac2f1da-01ff-4fb8-a338-74046f3d25ec"), Name = "Вернуть на доработку", Comment = "Вернуть на доработку" };
            raRep.Insert(ra9);



            var rtRep = RepositoryFactory.GetRepository<RouteType>();
            var rt0 = new RouteType { Id = new Guid("b932b534-29f4-4099-bf92-020c271ef18c"), Name = "Согласование", Comment = "Согласование" };
            rt0.Actions.Add(ra1);
            rt0.Actions.Add(ra5);
            rt0.Actions.Add(ra6);
            rtRep.Insert(rt0);
            var rt1 = new RouteType { Id = new Guid("145e6ca4-a973-4b52-98ab-1f2c42b8a897"), Name = "Подписать", Comment = "Подписать" };
            rt1.Actions.Add(ra1);
            rt1.Actions.Add(ra4);
            rtRep.Insert(rt1);
            var rt2 = new RouteType { Id = new Guid("fbba25c0-d74f-4007-8edb-ec6e38e2486d"), Name = "Исполнение", Comment = "Исполнение" };
            rt2.Actions.Add(ra0);
            rt2.Actions.Add(ra1);
            rt2.Actions.Add(ra7);
            rtRep.Insert(rt2);
            var rt3 = new RouteType { Id = new Guid("a9674ad4-c0fb-4282-90f9-020e41c536de"), Name = "Контроль исполнения", Comment = "Контроль исполнения" };
            rt3.Actions.Add(ra8);
            rt3.Actions.Add(ra9);
            rtRep.Insert(rt3);

            rtRep.Insert(new RouteType { Id = new Guid("5d57bc37-43d4-418c-ac36-6e62555b2c45"), Name = "Ознакомление", Comment = "Для ознакомления пользователей" });

            rtRep.Insert(new RouteType { Id = new Guid("ACE11FAE-204E-40AF-BDD0-A686395390E6"), Name = "Исполнение поручений", Comment = "Исполнение поручений" });
        }
    }
}
