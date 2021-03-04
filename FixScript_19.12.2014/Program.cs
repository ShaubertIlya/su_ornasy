using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixScript_19._12._2014
{
    class Program
    {
        //фикс на документы типа № 3314, 3343, которые завершены "FinishedOk", но весят для них "левые" поручения, в которых статус дока "InWork".
        //в веб интерфейсе эти поручения не видны, но в базе они есть.
        static void Main(string[] args)
        {

            RepositoryFactory.GetCurrentUser = () =>
            {
                return Guid.Empty;
            };

            var i = 1;

            RepositoryFactory.GetDocumentRepository().List(x => !x.isDeleted
                && x.DocumentType.Id == new Guid("e993583d-2ef8-4368-9ed5-5f4439374174") //входящий
                && x.docState == DocumentState.FinishedOk
                && x.CurrentStageCalcualted == "Завершена").ToList().ForEach(doc =>
                {
                    RepositoryFactory.GetInstructionRepository().List(x => !x.isDeleted
                        && x.RootDocumentId == doc.Id
                        && x.docState == DocumentState.InWork).ToList()
                        .ForEach(instr =>
                        {
                            Console.WriteLine(i + " док. № " + doc.DocumentNumber + "  docState в Instruction: " + instr.docState);

                            instr.docState = DocumentState.FinishedOk;
                            instr.DocumentSignStages.Select(x => x.isCurrent == true).ToList().ForEach(current =>
                            {
                                current = false;
                            });

                            i++;
                            RepositoryFactory.GetInstructionRepository().update(instr);                            
                        });
                });

            Console.ReadKey();
        }
    }
}
