using BdsSoft.DirectoryServices.Linq;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.ActiveDirectory
{
    public static class ADHelper
    {
        public static void FillPosts(DirectoryEntry de, RepositoryBase<Post> rep)
        {
            var users = new DirectorySource<ADUser>(de, SearchScope.Subtree);
            var posts = users.ToList().GroupBy(u => (u.Position == null ? "" : u.Position).ToLower()).Select(gr => gr.First().Position == null ? "" : gr.First().Position);
            posts.ToList().ForEach(p =>
            {
                if (rep.Single(p2 => p2.Name.ToLower() == p.ToLower()) == null)
                    rep.Insert(new Post() { Name = p });

            });
        }

        public static DirectoryEntry FindDirectoryEntry(DirectoryEntry de, string name)
        {
            DirectoryEntry result = null;
            if (de.Name.ToLower() == string.Format("OU={0}", name).ToLower())
                return de;

            if (result == null)
            {
                foreach (var childDe in de.Children.Cast<DirectoryEntry>().Where(d => d.SchemaClassName == "organizationalUnit"))
                {
                    if (childDe.Name.ToLower() == string.Format("OU={0}", name).ToLower())
                        return childDe;
                    result = FindDirectoryEntry(childDe, name);
                    if (result != null)
                        return result;
                }
            }
            return result;
        }

        public static DirectoryEntry FindTop(DirectoryEntry de)
        {
            if (de.Parent != null && de.Parent.SchemaClassName == "organizationalUnit")
            {
                de = FindTop(de.Parent);
            }
            return de;
        }

    }
}
