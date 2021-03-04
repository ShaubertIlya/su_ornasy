
using Devir.DMS.DL.Models.Document.Route;
using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document
{
    public class Document: ModelBase
    {

        public Guid? ForRootInstructionId { get; set; }

        public Guid? ForRootDocumentId { get; set; }

        public Guid? ForUserForRouteId { get; set; }

        public bool isUseConteroller { get; set; }
        public User Controller { get; set; }
        public virtual DocumentType DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public String Header { get; set; }
        public String Body { get; set; }
        public List<Guid> Attachments { get; set; }
        public List<NegotiatorsRoutes.NegotiationStage> Negotiators { get; set; }
        public List<DocumentFieldValues> FieldValues {get;set;}
        public Guid ParentDocumentId { get; set; }
        public List<Instruction> TempInstructionStorage { get; set; }
        public List<Route.RouteStage> RouteStages { get; set; }            
       // public List<Instruction> ListOfInstructions { get; set; }
        public User Author { get; set; }

        public bool isUrgent { get; set; }

        public Guid DynamicFiltrationFieldGuid {get;set;}
        public String DynamicFiltrationFieldValue { get; set; }

        //public List<EmbeddedInstructions.EmbeddedInstruction> instructions { get; set; }

        [BsonDefaultValue(1)]
        public int Version { get; set; }
       
        public Dictionary<String, DocumentViewers> DocumentViewers { get; set; }

        public List<DocumentViewer> NewDocumentViewers { get; set; }

        [BsonElement]
        public string CurrentStageCalcualted { get { return DocumentSignStages == null ?"Завершена":DocumentSignStages.Count(m=>m.isCurrent) > 0 ? DocumentSignStages.FirstOrDefault(m => m.isCurrent && m.ControlPerformForRouteStageUserId==null).Name : "Завершена"; } }

        public Guid CurentStageId
        {
            get
            {
                return DocumentSignStages.Count(m => m.isCurrent && m.ControlPerformForRouteStageUserId==null) > 0 ? DocumentSignStages.FirstOrDefault(m => m.isCurrent && m.ControlPerformForRouteStageUserId == null).Id : Guid.Empty; 
            }
        }

        public Guid StageUserIdForCurentPerformationStage
        {
            get
            {
                return DocumentSignStages.Count(m => m.isCurrent && m.ControlPerformForRouteStageUserId != null) > 0 ? DocumentSignStages.FirstOrDefault(m => m.isCurrent && m.ControlPerformForRouteStageUserId != null).ControlPerformForRouteStageUserId.Value : Guid.Empty;
            }
        }

        public bool IsCurrentPerformationStageId
        {
            get
            {
                return DocumentSignStages.Count(m => m.isCurrent && m.ControlPerformForRouteStageUserId != null) > 0 ? true: false;
            }
        }

        public Guid CurrentUserIdForPerformation
        {
            get
            {
                var tmpRoute = DocumentSignStages.FirstOrDefault(m => m.isCurrent && m.ControlPerformForRouteStageUserId != null);
                if (tmpRoute != null)
                {
                    var tmpUser = tmpRoute.RouteUsers.FirstOrDefault(m => m.SignUser.UserId == RepositoryFactory.GetCurrentUser() && m.IsCurent);
                    if (tmpUser != null)
                        return tmpUser.SignUser.UserId;

                }
                return new Guid();
            }
        }

        public Guid CurrentStageUserIdForPerformation
        {
            get
            {
                var tmpRoute = DocumentSignStages.FirstOrDefault(m => m.isCurrent && m.ControlPerformForRouteStageUserId != null);
                if (tmpRoute != null)
                {
                    var tmpUser = tmpRoute.RouteUsers.FirstOrDefault(m => m.SignUser.UserId == RepositoryFactory.GetCurrentUser() && m.IsCurent);
                    if (tmpUser != null)
                        return tmpUser.Id;

                }
                return new Guid();
            }
        }

        public Guid CurentStageTypeId
        {
            get
            {
                return DocumentSignStages.Count(m => m.isCurrent && m.ControlPerformForRouteStageUserId == null) > 0 ? DocumentSignStages.FirstOrDefault(m => m.isCurrent && m.ControlPerformForRouteStageUserId == null).RouteTypeId : Guid.Empty;
            }
        }

        public Guid CurrentStageUserId
        {
            get
            {
                var tmpRoute =  DocumentSignStages.FirstOrDefault(m => m.Id == CurentStageId);
                if(tmpRoute != null)
                {
                    var tmpUser = tmpRoute.RouteUsers.FirstOrDefault(m => m.SignUser.UserId == RepositoryFactory.GetCurrentUser() && m.IsCurent);
                    if(tmpUser !=null)
                        return tmpUser.Id;

                }
                return new Guid();
            }
        }

        public Guid CurrentStagePerformRealUserId
        {
            get
            {
                var tmpRoute = DocumentSignStages.FirstOrDefault(m => m.isCurrent == true && m.RouteTypeId == new Guid("a9674ad4-c0fb-4282-90f9-020e41c536de"));
                if (tmpRoute != null)
                {
                    var tmpUser = tmpRoute.RouteUsers.FirstOrDefault(m =>m.IsCurent);
                    if (tmpUser != null)
                        return tmpUser.SignUser.UserId;

                }
                return new Guid();
            }
        }


        public DocumentState docState { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime FinishDate { get; set; }

        /* Движение документа */

        public List<RouteStage> DocumentSignStages { get; set; }
      //  public Negotiation DocumentNegotiationStages { get; set; }

        /* Поручения */
     //   public List<Instruction> DocumentInstructions { get; set; }
        
        
        
        public void SignDocument()
        {
            MongoHelpers.MongoHelper.Database.Eval("SignDocumentOnCurrentStage", Id, RepositoryFactory.GetCurrentUser());
        }

        public void ChangeFieldValue(Guid FieldGuid)
        {
            //MongoHelpers.MongoHelper.Database.GetCollection<Document>("Document").Update(Query.EQ("_id",Id), Update.set("guidStorage.$.FieldHeader",  ));
        }

        public void AddComment(){

        }


        

        public void SendToOthers()
        {
        }       

        //#region Lazy for DocumentType
        //[BsonIgnore]
        //private DocumentType _documentType;
        //[BsonIgnore]
        //public DocumentType DocumentType
        //{
        //    get
        //    {
        //        if (_documentType == null)
        //            _documentType = RepositoryFactory.GetRepository<DocumentType>()
        //                                             .Single(m => m.Id == this.DocumentTypeId && m.isDeleted == false);

        //        return _documentType;
        //    }
        //}
        //#endregion
    }


    

    
    
}
