using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixScript_09._12._2014
{
    class Program
    {
        static void Main(string[] args)
        {
            //для полнотекстового поиска

            RepositoryFactory.GetCurrentUser = () =>
            {
                return Guid.Empty;
            };

            using (ServiceReference1.ServiceFTSClient clnt = new ServiceReference1.ServiceFTSClient())
            {

                var DateTimeFrom = new DateTime(2015, 09, 04);
                                                                              //&& x.CreateDate > new DateTime(2014, 12, 08)
                RepositoryFactory.GetDocumentRepository().List(x => !x.isDeleted && x.CreateDate > DateTimeFrom).ToList().ForEach(m =>
                {
                    clnt.SaveDocument(m.Id);

                    Console.WriteLine("Обработан док. № " + m.DocumentNumber);
                });

                Console.WriteLine("Обработка завершена");
                Console.ReadKey();

            }

        }
    }
}
