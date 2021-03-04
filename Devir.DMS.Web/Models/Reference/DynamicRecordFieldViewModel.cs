using Devir.DMS.DL.Models.Document.NegotiatorsRoutes;
using Devir.DMS.DL.Models.Document.StorageTypes;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using Devir.DMS.Web.Models.NegotiatorsKO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.Reference
{
    public class DynamicRecordFieldViewModel
    {
        public Guid Id { get; set; }
        public string Header { get; set; }
        public string Value { get; set; }
        public Guid ValueId { get; set; }      
        public bool isRequired {get; set; }
       // public UserForRoute ValueUserForRoute { get; set; }
        public List<UsersKO.UserKO> ValueUsersKo { get; set;}
        public Guid DynamicFieldTemplateId { get; set; }       
        public Guid TypeOfTheFieldId { get; set; }
        public Guid DynamicReferenceId { get; set; }
        public string DynamicReferenceResult { get; set; }
        public List<NegotiationStageEditorModel> ModelHelper { get; set; } //Костыль

        public DynamicRecordFieldViewModel()
        {
            //var res = new List<NegotiationStage>();
            //var usrRes = new List<UserForRoute>();

            //var userGuid = RepositoryFactory.GetCurrentUser();

            

            //usrRes.Add(new UserForRoute() { IsMustSign = true, Order = 1, User = Devir.DMS.DL.Repositories.RepositoryFactory.GetRepository<User>().Single(m => m.Id == userGuid), DateBeforeSign = DateTime.Now, UserId = RepositoryFactory.GetCurrentUser() });

            //res.Add(new NegotiationStage() { Order = 321, StageType = NegotiationStageTypes.Sequenced, UsersForNegotiation = usrRes });
            //Negotiators = res;
        }
    }

    
}