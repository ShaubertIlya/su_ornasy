using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using BdsSoft.DirectoryServices.Linq;
using System.Configuration;
using Devir.DMS.DL.ActiveDirectory;

namespace Devir.DMS.ADImport
{
    class Program
    {
        private static string CONNECTION_STRING = ConfigurationManager.AppSettings["ConnectionString"];
        
        static void Main(string[] args)
        {

            RepositoryFactory.GetCurrentUser = () =>
            {
                //Сюда код для получения GUID текущего пользователя
                //Для примера я просто генерирую GUID
                return new Guid("C6F70CC1-1E8F-447B-81F6-B3B88674877B");
            };
            
            var depRep = RepositoryFactory.GetRepository<Department>();
            var uRep = RepositoryFactory.GetRepository<User>();
            var postRep = RepositoryFactory.GetRepository<Post>();

            var de = new DirectoryEntry(CONNECTION_STRING);

            ADHelper.FillPosts(de, postRep);

            var deps = depRep.List(d => !string.IsNullOrEmpty(d.OU) && !d.isDeleted);

            string domain = "";
            
            de = ADHelper.FindTop(de);

            domain = de.Parent.Properties["dc"].Value as string;

            foreach (var dep in deps)
            {
                dep.Users.Clear();
                //depRep.update(dep);
                var depDE = ADHelper.FindDirectoryEntry(de, dep.OU);
                var users = new DirectorySource<ADUser>(depDE, SearchScope.OneLevel);
                users.ToList().ForEach(u =>
                {
                    User user = new User()
                    {
                        UserId = new Guid((byte[])u.UserId),
                        Name = string.Format("{0}\\{1}", domain, u.AccountName),
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        WhenCreated = u.WhenCreated,
                        WhenChanged = u.WhenChanged,
                        DepartmentId = dep.Id
                    };

                    if (uRep.Single(u2 => u2.UserId == user.UserId) == null)
                        uRep.Insert(user);
                    Post post = null;
                    if (!string.IsNullOrEmpty(u.Position))
                    {
                        post = postRep.Single(p => p.Name.ToLower() == u.Position.ToLower());
                    }
                    dep.Users.Add(user, post);
                });
                depRep.update(dep);
            }
        }

        
    }
}
