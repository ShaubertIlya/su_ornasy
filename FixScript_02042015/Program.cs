using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixScript_02042015
{
    class Program
    {
        static void Main(string[] args)
        {

            RepositoryFactory.GetCurrentUser = () =>
            {
                return Guid.Empty;
            };

             RepositoryFactory.GetInstructionRepository().List(x => !x.isDeleted).ToList().ForEach(
                 m =>
                     {
                         var doc =
                             RepositoryFactory.GetDocumentRepository()
                                 .List(k => k.Id == m.RootDocumentId)
                                 .FirstOrDefault();
                         if (doc != null) { 
                         m.RootDocumentTypeId = doc.DocumentType.Id;

                         RepositoryFactory.GetInstructionRepository().update(m);

                         Console.WriteLine("Законили с поручением №"+m.DocumentNumber);
                         }
                     });



        }
    }
}
