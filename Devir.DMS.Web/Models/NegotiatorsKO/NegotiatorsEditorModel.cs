using Devir.DMS.DL.Models.Document.NegotiatorsRoutes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.NegotiatorsKO
{
    //Модель пользователя в этапе согласования
    public class UsersForNegotiationEditorModel
    {
        public int order { get; set; }
        [Required]
        public Guid userId { get; set; }
         [Required(ErrorMessage="Необходимо заполнить поле пользователь")]
        public string userName { get;set;}         
        public String signBefore { get; set; }
         
        public bool isMust { get; set; }
    }

    //Сам этап согласования
    public class NegotiationStageEditorModel
    {
        public int order { get; set; }
        public List<UsersForNegotiationEditorModel> UsersForNegotiationStage { get; set; }
        public string StageType { get; set; }
        public List<string> StageTypes { get; set; }
       
                
        public NegotiationStageEditorModel()
        {
            UsersForNegotiationStage = new List<UsersForNegotiationEditorModel>();
            StageTypes = new List<string>() { "Параллельное", "Последовательное" };
        }

        public void IncreaseUsersOrder(int userIndex)
        {
          //  var tmpOrderedUsers = UsersForNegotiationStage.OrderBy(m => order).ToList();
            if (userIndex >= 0 && userIndex < UsersForNegotiationStage.Count)
            {
                if (UsersForNegotiationStage.ElementAtOrDefault(userIndex + 1) != null)
                {
                    UsersForNegotiationStage.ElementAtOrDefault(userIndex + 1).order--;
                    UsersForNegotiationStage[userIndex].order++;
                }
                recalculateOrders();
            }
        }


        public void DecreaseUsersOrder(int userIndex)
        {
            //  var tmpOrderedUsers = UsersForNegotiationStage.OrderBy(m => order).ToList();
            if (userIndex >= 0 && userIndex < UsersForNegotiationStage.Count)
            {
                if (UsersForNegotiationStage.ElementAtOrDefault(userIndex - 1) != null)
                {
                    UsersForNegotiationStage.ElementAtOrDefault(userIndex - 1).order++;
                    UsersForNegotiationStage[userIndex].order--;
                }
                recalculateOrders();
            }
        }

        private void recalculateOrders()
        {
            var tmpUserOrders = this.UsersForNegotiationStage.OrderBy(m => m.order).ToList();

            var i = 0;
            tmpUserOrders.ForEach(m => {
                m.order = i++;
            });
            this.UsersForNegotiationStage = tmpUserOrders;
        }       

        public void AddUserToNegotiationStage()
        {
            UsersForNegotiationStage.Add(new UsersForNegotiationEditorModel(){ isMust=true,  order = UsersForNegotiationStage.Count()>0 ? UsersForNegotiationStage.Max(m=>m.order)+1 : 0 });
        }

        public void DeleteUserFromNegotiationStage(int userIndex)
        {
            if (userIndex >= 0 && userIndex < UsersForNegotiationStage.Count)
                UsersForNegotiationStage.RemoveAt(userIndex);
            recalculateOrders();
        }
    }

   //Общая модель для редактирования этапов согласования
    public class NegotiatorsEditorModel
    {
        public List<NegotiationStageEditorModel> NegotiatorsStage { get; set; }

        private void recalculateOrders()
        {
            var tmpUserOrders = this.NegotiatorsStage.OrderBy(m => m.order).ToList();

            var i = 0;
            tmpUserOrders.ForEach(m =>
            {
                m.order = i++;
            });
            this.NegotiatorsStage = tmpUserOrders;
        }       

        public void AddStage()
        {
            NegotiatorsStage.Add(new NegotiationStageEditorModel() { order = NegotiatorsStage.Count()>0 ? NegotiatorsStage.Max(m=>m.order)+1 : 0 });
        }

        public void DeleteStage(int stageIndex)
        {
            if (stageIndex >= 0 && stageIndex < NegotiatorsStage.Count)
                NegotiatorsStage.RemoveAt(stageIndex);
            recalculateOrders();
        }

        public void RemoveUser(int stageIndex, int userId)
        {
            if (stageIndex >= 0 && stageIndex < NegotiatorsStage.Count)
                NegotiatorsStage[stageIndex].DeleteUserFromNegotiationStage(userId);
        }


        public void AddUserToStage(int stageIndex)
        {
            if (stageIndex >= 0 && stageIndex < NegotiatorsStage.Count)
                NegotiatorsStage[stageIndex].AddUserToNegotiationStage();
        }

        public void IncreaseUsersInStageOrder(int stageIndex, int userId)
        {
             if (stageIndex >= 0 && stageIndex < NegotiatorsStage.Count)
                 NegotiatorsStage[stageIndex].IncreaseUsersOrder(userId);
        }

        public void DecreaseUsersInStageOrder(int stageIndex, int userId)
        {
            if (stageIndex >= 0 && stageIndex < NegotiatorsStage.Count)
                NegotiatorsStage[stageIndex].DecreaseUsersOrder(userId);
        }

       

      

    }



   
}