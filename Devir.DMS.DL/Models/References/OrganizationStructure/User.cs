using Devir.DMS.DL.Repositories;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.References.OrganizationStructure
{
    public class User: ModelBase
    {
        public Guid UserId { get; set; }
        [DisplayName("Логин")]
        public String Name { get; set; }
        [DisplayName("Имя")]
        public String FirstName { get; set; }
        [DisplayName("Фамилия")]
        public String LastName { get; set; }
        [DisplayName("Отчество")]
        public String FatherName { get; set; }
        [DisplayName("Email")]
        public String Email { get; set; }
        [DisplayName("Телефон")]
        public String Phone { get; set; }
        [DisplayName("Пол")]
        public bool? IsMale { get; set; }
        [DisplayName("Дата рождения")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime BirthDate { get; set; }
        [DisplayName("Национальность")]
        public String Nationality { get; set; }
        [DisplayName("Гражданство")]
        public String Citizenship { get; set; }
        [DisplayName("")]
        public Guid DepartmentId { get; set; }
        [DisplayName("")]
        public DateTime WhenCreated { get; set; }
        public DateTime WhenChanged { get; set; }
        public Guid? AlterUserId { get; set; }
        [DisplayName("Номенклатура")]
        public String Nomenclature { get; set; }
       
        public bool InRole(string roleName)
        {
            var role = RepositoryFactory.GetRepository<Role>().Single(r => r.Name.ToLower() == roleName.ToLower());
            if (role == null)
                throw new Exception("Указанной роли не существует");
            return role.UsersInRoles.Contains(this.UserId);
        }

        public List<string> GetRoles()
        {
            return RepositoryFactory.GetRepository<Role>().List(r => r.UsersInRoles.Contains(this.UserId)).Select(r2 => r2.Name).ToList();
        }

        public bool CheckRoles(List<Role> roles)
        {
            return roles.Select(m => m.Name).Intersect(GetRoles()).Any();
        }

        public string GetFIO()
        {
            //var tmpFirst = !String.IsNullOrEmpty(this.FirstName)?this.FirstName.First().ToString().ToUpper()+".":"";
            //var tmpFathers = !String.IsNullOrEmpty(this.FatherName)?this.FatherName.First().ToString().ToUpper()+".":"";
            
            //return String.Format("{0} {1}{2}", this.LastName, tmpFirst, tmpFathers);

            return String.Format("{0} {1} {2}", this.LastName, this.FirstName, this.FatherName);
        }
    }
}
