using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using Devir.DMS.Web.Helpers.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.OrganiztionStructure
{
    public class UserViewModel
    {

        public Guid UserId { get; set; }
        [DisplayName("Логин")]
        public String Name { get; set; }
        [DisplayName("Имя")]
        public String FirstName { get; set; }
        [DisplayName("Фамилия")]
        [Required]
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
        public DateTime BirthDate { get; set; }
        [DisplayName("Национальность")]
        public String Nationality { get; set; }
        [DisplayName("Гражданство")]
        public String Citizenship { get; set; }
        public Guid DepartmentId { get; set; }
        [DisplayName("Номенклатура")]
        public String Nomenclature { get; set; }
        
        [SameGuidAttribute("UserId", ErrorMessage="Сотрудник не может заменять сам себя")]
        public Guid? AlterUserId { get; set; }
        [DisplayName("Заменяющий сотрудник")]
        public String AlterUserName { get; set; }

        public void Save()
        {
            var userRep = RepositoryFactory.GetRepository<User>();
            var user = userRep.Single(u => !u.isDeleted && u.UserId == this.UserId);
            user.BirthDate = DateTime.Parse(this.BirthDate.ToString("yyyy-MM-dd"));
            user.Citizenship = this.Citizenship;
            user.Email = this.Email;
            user.FatherName = this.FatherName;
            user.FirstName = this.FirstName;
            user.IsMale = this.IsMale;
            user.LastName = this.LastName;
            user.Nationality = this.Nationality;
            user.Phone = this.Phone;
            user.AlterUserId = this.AlterUserId;
            user.Nomenclature = this.Nomenclature;
            userRep.update(user);
            var depRep = RepositoryFactory.GetRepository<Department>();
            var dep = depRep.Single(d => !d.isDeleted && d.Id == this.DepartmentId);
            var depUser = dep.Users.SingleOrDefault(u => !u.Key.isDeleted && u.Key.UserId == this.UserId);
            depUser.Key.BirthDate = DateTime.Parse(this.BirthDate.ToString("yyyy-MM-dd"));
            depUser.Key.Citizenship = this.Citizenship;
            depUser.Key.Email = this.Email;
            depUser.Key.FatherName = this.FatherName;
            depUser.Key.FirstName = this.FirstName;
            depUser.Key.IsMale = this.IsMale;
            depUser.Key.LastName = this.LastName;
            depUser.Key.Nationality = this.Nationality;
            depUser.Key.Phone = this.Phone;
            depUser.Key.AlterUserId = this.AlterUserId;
            depUser.Key.Nomenclature = this.Nomenclature;
            depRep.update(dep);
        }
        
    }
}