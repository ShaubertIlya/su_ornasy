using Devir.DMS.DL.Models.Document.StorageTypes;
using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Models.References.DynamicReferences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Devir.DMS.DL.Models.Document
{
    public class DocumentFieldValues
    {
        public Guid Id { get; set; }

        public String ValueToDisplay { get; set; }
        public Guid FieldTemplateId { get; set; }
        public Guid FieldTypeId { get; set; }
        public int OrderInDocument { get; set; }
        public Guid DynamicReferenceId { get; set; }
        public Guid DynamicReferenceFieldTemplateId { get; set; }
        public String Header { get; set; }       

        public int? IntValue { get; set; }
        public string StringValue { get; set; }
        public bool? BooleanValue { get; set; }
        public Guid? GuidValue { get; set; }

        public string DecimalValue { get; set; }
        public List<int> IntListValue { get; set; }
        public List<decimal> DecimalListValue { get; set; }
        public List<string> StringListValue { get; set; }
        public List<bool> BoolListValue { get; set; }
        public List<Guid> GuidListValue { get; set; }
        //   public UserForRoute UserForRouteValue { get; set; }
        public List<UsersForRoute> UserForRouteListValue { get; set; }
        public DateTime? DateTimeValue { get; set; }
        public Guid DynamicRecordId { get; set; }       
    }

    public class UsersForRoute
    {
        public Guid UserId { get; set; }
        public int Order { get; set; }
    }

    public enum FieldValueType
    {
        Info = 1,
    RouteSign = 2,
    RouteActors = 3    
    }
}
