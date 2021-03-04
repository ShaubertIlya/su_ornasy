using Devir.DMS.DL.Models.DocumentTemplates;
using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document.Route
{
   

    public class RouteStageUser
    {
        public Guid Id { get; set; }

        //Порядок для подписи в одном этапе
        public int Order { get; set; }

        //Предварительно расчитанный пользователь, при подходе к текущему этапу пересчет
        public User SignUser { get; set; }

        //Досутпные действия
        public List<RouteAction> UsersActions { get; set; }

        //Результат подписи, если не подписан то null
        public UserSignResult SignResult {get;set;}

        //Флаг подписан ли документ или нет
        public bool isSigned{get{ return SignResult!=null;}}

        //Флаг показывающий что текущий пользователь выполняет в документе какието действия
        public bool IsCurent { get; set; }

        public Guid? SecondChanceForId { get; set; }

       

        public List<Guid> Instructions { get; set; }
    }
    
    public class RouteStage
    {        
        public Guid Id { get; set; }
        public string Name { get; set; }
        ////Тип стадии
        //public Guid RouteTypeId { get; set; }
        //Порядок стадий
        public int Order {get;set;}
        //Тип стадии
        public Guid RouteTypeId { get; set; }
        //Пользователи стадии
        public List<RouteStageUser> RouteUsers { get; set; }
        //Текущая ли стадия
        public bool isCurrent { get; set; }
        //Дата заврщения стадии
        public DateTime? FinishDate { get; set; }
        //Тип стадии для согласования
        public NegotiatorsRoutes.NegotiationStageTypes StageType { get; set; }
        //Нужен ли на стадии контроль исполнения
        public bool? NeedSignResultConfirmation { get; set; }
        //jjj
        public Guid? ControlPerformForRouteStageUserId { get; set; }
        
        public Guid RouteTemplateId { get; set; }
     }

   

    //public class NegotiationRouteStage
    //{
    //    public Guid Id { get; set; }
    //    //Порядок
    //    public int Order { get; set; }
       
    //    //Список польщователей в текущем этапе
    //    public List<RouteStageUser> SignUserList { get; set; }
    //    //Текущая стадия согласования
    //    public bool IsCurrent { get; set; }
    //    //Дата завершения этапа
    //    public DateTime? FinishDate { get; set; }
        
    //}

    //public class Negotiation
    //{
    //    //Активна ли сейчас стадия согласования
    //    public bool isActive { get;set;}
    //    //Этапы согласования
    //    public List<NegotiationRouteStage> Stages { get; set; }
    //    //Дата завершения стадии согласования
    //    public DateTime? FinishDate { get; set; }
    //}

    

    

    
}
