using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Helpers.Validation
{
    public class SameGuidAttribute : ValidationAttribute
    {
        public string PropertyName { get; set; }

        public SameGuidAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperties().SingleOrDefault(p => p.Name == this.PropertyName);
            var currentGuid = value as Guid?;
            var propertyValue = property.GetValue(validationContext.ObjectInstance) as Guid?;
            if (currentGuid == propertyValue)
                return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
            return null;
        }

    }
}