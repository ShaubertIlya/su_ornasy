using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.DocumentTemplates
{
    public class RouteTemplate
    {
        public Guid Id { get; set; }

        [BsonIgnore]        
        [Required]
        public Guid TypeOfTheRouteId { get; set; }

        [BsonIgnore]
        public string DocumentFieldName { get; set; }

        [Required]
        public Guid DocumentFieldTemplate { get;set; }
        public int FieldOrder { get; set; }

        public string RouteCondition { get; set; }  // TODO: Сделать инструмент для составления условий выполнения этапа маршрута

        public Guid? UserByDefault {get;set;} // TODO: Сделать интерфейс для выбора пользователя по умолчанию

        public RouteType TypeOfTheRoute { get; set; }

        public bool? NeedSignResultConfirmation { get; set; } // TODO: Сделать интерфейс для определния галочки подтверждения исполнения 

        public bool isAuthor { get; set; }

        #warning Замутить возможность добавления списка полей к маршруту
        public List<Guid> DocumentFields { get; set; } //Пока не используем
               
    }
}
