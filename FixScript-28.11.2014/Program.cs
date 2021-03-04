using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Repositories;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixScript_28._11._2014
{
    class Program
    {
        static void Main(string[] args)
        {
            //Актобе 
            //исправление документов где не выбрали номер и выставился по умолчанию 333 и 362

            RepositoryFactory.GetCurrentUser = () =>
            {
                return Guid.Empty;
            };


            RepositoryFactory.GetDocumentRepository().List(x => x.isDeleted == false && 
                                                           x.DocumentType.Id == new Guid("9655a0c3-a516-41cb-a2df-cbd2a096cf2a")).ToList()
                .ForEach(doc =>
                {
                    doc.FieldValues.Where(w => w.FieldTypeId == new Guid("398877ee-49f3-46b6-bc2e-f567ecd75410") &&  
                        (w.ValueToDisplay == "333" || w.ValueToDisplay == "362") ).ToList()

                    .ForEach(fv =>
                    {
                        fv.ValueToDisplay = "Не связан";
                        fv.StringValue = "";
                    });

                    RepositoryFactory.GetDocumentRepository().update(doc);
                    Console.WriteLine("Обновился док " + doc.DocumentNumber);

                });

            Console.WriteLine("Завершено");
            Console.ReadKey();

        }
    }
}
