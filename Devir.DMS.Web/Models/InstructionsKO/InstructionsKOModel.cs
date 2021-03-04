using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.InstructionsKO
{
    public class InstructionKO
    {
        public int Order { get; set; }
        public List<IstructionKOUserModel> UsersFor { get; set; }

        public Guid? UserFor { get; set; }
        public string UserForS { get; set; }
        public String DateBefore { get; set; }
        public string Body { get; set; }
        public string Resolutions { get; set; }
        public bool isUseController { get; set; }
        public Guid? UserController { get; set; }
        public string UserControllerS { get; set; }
        public bool ShowResolutions { get; set; }
    }

    public class IstructionKOUserModel
    {
        public Guid? UserFor { get; set; }
        public string UserForS { get; set; }
    }

    public class InstructionKOEditorModel
    {
        public List<InstructionKO> Instructions { get;set;}
        public bool ShowResolutions { get; set;}

        public InstructionKOEditorModel()
        {
            Instructions = new List<InstructionKO>();
            //Instructions.Add(new InstructionKO() { Order = Instructions.Count() > 0 ? Instructions.Max(m => m.Order) + 1 : 0 });
        }

        private void recalculateOrders()
        {
            var tmpInstructions = this.Instructions.OrderBy(m => m.Order).ToList();

            var i = 0;
            tmpInstructions.ForEach(m =>
            {
                m.Order = i++;
            });
            this.Instructions = tmpInstructions;
        }

        public void AddInstructionToList()
        {
            var user = RepositoryFactory.GetAnonymousRepository<User>().Single(m=>m.UserId == RepositoryFactory.GetCurrentUser());
            Instructions.Add(new InstructionKO() { isUseController=true, Order = Instructions.Count() > 0 ? Instructions.Max(m => m.Order) + 1 : 0, UsersFor = new List<IstructionKOUserModel>(), DateBefore=DateTime.Now.ToString("dd.MM.yyyy"), UserController = user.UserId, UserControllerS = user.GetFIO(), ShowResolutions = this.ShowResolutions});
        }

        public void Delete(int index)
        {
            if (index >= 0 && index < Instructions.Count)
                Instructions.RemoveAt(index);
            recalculateOrders();
        }

    }

}