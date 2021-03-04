using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Repositories;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixScript_18._11._2014
{
    class Program
    {
        static void Main(string[] args)
        {
            /*1) Документы которые фактически исполнены и всеми согласованны, имеют статус "На подписи" (пример: 3353)
              2) Документы исполненные имеющие статус "Выполнено", в отчетах идут как не исполненные и просроченные (пример: 6610)*/

            RepositoryFactory.GetCurrentUser = () =>
            {
                return Guid.Empty;
            };

            //Исполнить
            var routeAction = RepositoryFactory.GetAnonymousRepository<RouteAction>().Single(x=>x.Id == 
                new Guid("b25f05fc-c36b-4663-b2d1-3ab7b42ce04b"));

            RepositoryFactory.GetDocumentRepository().List(x => x.isDeleted == false).ToList().ForEach(doc =>
            {
                if (doc.CurrentStageCalcualted != "Завершена")
                {
                    var count = RepositoryFactory.GetInstructionRepository().GetCollection(false).Count(
                         Query.And(
                         Query<Instruction>.EQ(m => m.RootDocumentId, doc.Id),
                         Query<Instruction>.NE(m => m.CurrentStageCalcualted, "Завершена")
                         )
                         );

                    var countOfIstr = RepositoryFactory.GetInstructionRepository().GetCollection(false).Count(
                         Query.And(
                         Query<Instruction>.EQ(m => m.RootDocumentId, doc.Id)                         
                         )
                         );


                    if (count == 0 && countOfIstr > 0)
                    {
                        doc.DocumentSignStages.ForEach(dss =>
                        {
                            dss.isCurrent = false;
                            dss.FinishDate = DateTime.Now;                           

                            dss.RouteUsers.ForEach(ru =>
                            {
                                ru.IsCurent = false;
                                ru.SignResult = new Devir.DMS.DL.Models.Document.Route.UserSignResult();
                                ru.SignResult.Action = routeAction;
                                ru.SignResult.ActionId = routeAction.Id;
                                ru.SignResult.Date = DateTime.Now;
                            });

                        });

                        RepositoryFactory.GetDocumentRepository().update(doc);
                        Console.WriteLine("Обновился документ" + doc.DocumentNumber);
                    }                   

                }
                              
            });

            Console.WriteLine("Завершено");
            Console.ReadKey();

            //RepositoryFactory.GetDocumentRepository().List(x => x.TempInstructionStorage.Select(i => i.CurrentStageCalcualted == "Завершена" ));
        }
    }
}