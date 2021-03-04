using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.References.OrganizationStructure
{
    public class Department: ModelBase
    {       
        [DisplayName("Наименование")]
        [Required]
        public String Name { get; set; }
        public Guid? ParentDepertmentId { get; set; }
        
        /// <summary>
        /// Пользователи входящие в структуру подразделения + должность
        /// </summary>
        public Dictionary<User, Post> Users { get; set; }
        public Guid? ChiefUserId { get; set; }
        public string OU { get; set; }
        [DisplayName("Код подразделения")]
        public String Code { get; set; }

        public Department()
        {
            Users = new Dictionary<User, Post>();
        }
    }
}
