using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Repositories;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.DocumentTemplates
{
    public class FieldTemplate
    {
        public Guid Id { get;set; }
        [DisplayName("Наименование")]
        [Required(ErrorMessage = "Введи наименование")]
        public string Header { get; set; }    
        public int FieldOrder { get; set; }
        public bool isRequired { get; set; }
        [BsonIgnore]
        [Required(ErrorMessage="Выберите тип поля")]
        public Guid FieldTypeId { get; set; }
        public FieldType FieldType { get; set; }

        

        //LazyForDucmentTypeId
        //#region Lazy for DocumentType
        //private Guid _documentTypeId;

        //[BsonIgnore]
        //public Guid DocumentTypeId
        //{
        //    get { return _documentTypeId; }
        //    set
        //    {
        //        DocumentType = new Lazy<DocumentType>(() =>
        //        {
        //            return RepositoryFactory.GetRepository<DocumentType>()
        //                                                     .Single(m => m.Id == DocumentTypeId && m.isDeleted == false);
        //        }, true);
        //        _documentTypeId = value; }
        //}
        //[BsonIgnore]
        //public Lazy<DocumentType> DocumentType;
        //#endregion

        //#region Lazy for DocumentType
        
        //private DocumentType _documentType;
        //[BsonIgnore]
        //public DocumentType DocumentType
        //{ 
        //    get
        //    {
        //        if (_documentType == null)
        //            _documentType = 

        //        return _documentType;
        //    }
        //}
        //#endregion

        //#region Lazy for FieldType
        //[BsonIgnore]
        //private FieldType _fieldType;
        //[BsonIgnore]
        //public FieldType FieldType
        //{
        //    get
        //    {
        //        if (_fieldType == null)
        //            _fieldType = RepositoryFactory.GetRepository<FieldType>()
        //                                             .Single(m => m.Id == this.FieldTypeId && m.isDeleted == false);

        //        return _fieldType;
        //    }
        //}
        //#endregion
    }
}
