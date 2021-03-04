using BdsSoft.DirectoryServices.Linq;
using Devir.DMS.DL.ActiveDirectory;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.ADSync
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

            var depRep = RepositoryFactory.GetNoAuditRepository<Department>();
            var uRep = RepositoryFactory.GetNoAuditRepository<User>();
            var postRep = RepositoryFactory.GetNoAuditRepository<Post>();

            var deps = depRep.List(d => !d.isDeleted && !string.IsNullOrEmpty(d.OU));

            var de = new DirectoryEntry(CONNECTION_STRING);
            string domain = "";
            de = ADHelper.FindTop(de);
            domain = de.Parent.Properties["dc"].Value as string;
            var AllUsersToDelete = new List<User>();
            foreach (var dep in deps)
            {
                if (string.IsNullOrEmpty(dep.OU))
                    continue;

                DirectoryEntry depDE = ADHelper.FindDirectoryEntry(de, dep.OU);
                if (depDE == null)
                {
                    depRep.Delete(dep.Id);
                    continue;
                }
                
                var users = new DirectorySource<ADUser>(depDE, SearchScope.OneLevel).ToList();
                bool update = false;
                foreach (var user in users)
                {

                    Guid search_id = Guid.Empty;
                    if (string.Format("{0}\\{1}", domain.ToLower(), user.AccountName.ToLower())
                        != "akbulak\\s.dzhadiev") search_id = new Guid((byte[])user.UserId);
                    else search_id = new Guid("B8DC8180-B53E-475B-9AB0-58701300DCC5");

                    var depUser = dep.Users.SingleOrDefault(u => !u.Key.isDeleted && u.Key.UserId == search_id);
                    if (depUser.Key == null)
                    {
                        User newUser = uRep.Single(uu => !uu.isDeleted && uu.UserId == new Guid((byte[])user.UserId));

                        if (newUser == null)
                        {
                            Guid id = Guid.Empty;
                            if (string.Format("{0}\\{1}", domain.ToLower(), user.AccountName.ToLower())
                                != "akbulak\\s.dzhadiev") id = new Guid((byte[])user.UserId);
                            else id = new Guid("B8DC8180-B53E-475B-9AB0-58701300DCC5");
                            newUser = new User()
                                {
                                    UserId = id,
                                    Name = string.Format("{0}\\{1}", domain, user.AccountName),
                                    FirstName = user.FirstName,
                                    LastName = user.LastName,
                                    Email = user.Email,
                                    WhenCreated = user.WhenCreated,
                                    WhenChanged = user.WhenChanged,
                                    DepartmentId = dep.Id
                                };
                            uRep.Insert(newUser);
                        }
                        else
                        {
                            if (newUser.WhenChanged != user.WhenChanged)
                            {
                                newUser.Name = string.Format("{0}\\{1}", domain, user.AccountName);
                                newUser.FirstName = user.FirstName;
                                newUser.LastName = user.LastName;
                                newUser.Email = user.Email;
                                newUser.WhenCreated = user.WhenCreated;
                                newUser.WhenChanged = user.WhenChanged;
                                newUser.DepartmentId = dep.Id;                              
                                uRep.update(newUser);
                            }
                            var oldDep = deps.SingleOrDefault(d => !d.isDeleted && d.Users.Where(du => !du.Key.isDeleted && du.Key.UserId == newUser.UserId).Count() > 0);
                            if (oldDep != null)
                            {
                                var oldUser = oldDep.Users.Single(odu => !odu.Key.isDeleted && odu.Key.UserId == newUser.UserId);
                                oldUser.Key.isDeleted = true;
                                oldUser.Key.DeleteDate = DateTime.Now;
                                depRep.update(oldDep);
                            }
                        }

                        Post post = null;
                        if (!string.IsNullOrEmpty(user.Position))
                        {
                            post = postRep.Single(p => !p.isDeleted && p.Name.ToLower() == user.Position.ToLower());

                            if (post == null)
                            {
                                post = new Post()
                                {
                                    Name = user.Position
                                };
                                postRep.Insert(post);
                            }
                        }
                        dep.Users.Add(newUser, post);
                        update = true;
                    }
                    else
                    {
                        if (depUser.Key.WhenChanged != user.WhenChanged)
                        {
                            depUser.Key.Name = string.Format("{0}\\{1}", domain, user.AccountName);
                            depUser.Key.FirstName = user.FirstName;
                            depUser.Key.LastName = user.LastName;
                            depUser.Key.Email = user.Email;
                            depUser.Key.WhenChanged = user.WhenChanged;
                            depUser.Key.WhenCreated = user.WhenCreated;
                            if (depUser.Value == null && !string.IsNullOrEmpty(user.Position) || depUser.Value != null && depUser.Value.Name.ToLower() != user.Position.ToLower())
                            {
                                Post newPost = postRep.Single(p => p.Name.ToLower() == user.Position.ToLower());
                                if (newPost == null && !string.IsNullOrEmpty(user.Position))
                                {
                                    newPost = new Post();
                                    newPost.Name = user.Position;
                                    postRep.Insert(newPost);
                                }
                                dep.Users[depUser.Key] = newPost;
                            }
                            User userFromUsers = uRep.Single(ufu => ufu.UserId == depUser.Key.UserId);
                            if (userFromUsers == null)
                            {
                                userFromUsers = new User();
                            }
                            userFromUsers.UserId = depUser.Key.UserId;
                            userFromUsers.Name = string.Format("{0}\\{1}", domain, user.AccountName);
                            userFromUsers.FirstName = user.FirstName;
                            userFromUsers.LastName = user.LastName;
                            userFromUsers.Email = user.Email;
                            userFromUsers.WhenChanged = user.WhenChanged;
                            userFromUsers.WhenCreated = user.WhenCreated;
                            uRep.update(userFromUsers);
                            update = true;
                        }
                    }
                }

                var usersToDelete = dep.Users.Where(ud => !ud.Key.isDeleted && users.Where(uu => new Guid((byte[])uu.UserId) == ud.Key.UserId).Count() == 0
                    && (dep.ChiefUserId == null || dep.ChiefUserId != ud.Key.UserId)
                    );
                if (usersToDelete.Count() > 0)
                {
                    usersToDelete.ToList().ForEach(utd =>
                    {
                        if (dep.Users.Keys.Any(m => m.Id == utd.Key.Id))
                        {
                            if (dep.Users[utd.Key] != null)
                            {
                                Console.WriteLine("Удаляем пользователя: {0}", utd.Key.Name);

                                dep.Users.Keys.Where(m => !m.isDeleted && m.UserId == utd.Key.UserId).ToList().ForEach(u =>
                                u.isDeleted = true);
                            dep.Users[utd.Key].isDeleted = true;
                            dep.Users[utd.Key].DeleteDate = DateTime.Now;
                            AllUsersToDelete.Add(utd.Key);
                        }
                        }
                    });

                    update = true;
                }

                if (update)
                {                    
                    depRep.update(dep);
                    Console.WriteLine("Удалили");
                }
            }

            AllUsersToDelete.ForEach(u =>
            {
                if (deps.Count(d => !d.isDeleted && d.Users.Where(u2 => !u2.Key.isDeleted && u2.Key.UserId == u.UserId).Count() > 0) > 0)
                {
                    User userToDelete = uRep.Single(u3 => !u3.isDeleted && u3.UserId == u.UserId);
                    try
                    {
                        uRep.Delete(userToDelete.Id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Проблема при удалении пользователя", u.Name);
                    }

                    
                }
            });
        }

        
    }
}
