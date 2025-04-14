using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.Validations
{
    public class AllowedExtensionsAttribute:ValidationAttribute
    {
        private readonly string _allowedExtensions;
        public AllowedExtensionsAttribute(string _allowedExtensions)
        {
            this._allowedExtensions = _allowedExtensions;
        }
        protected override ValidationResult IsValid(object value,
            ValidationContext validationContext)
        {
           if (value is IFormFile file)
            {
                var extension=Path.GetExtension(file.FileName);
                var isallowed=_allowedExtensions.Split(',').Contains(extension,StringComparer.OrdinalIgnoreCase);

                if (!isallowed) {

                    return new ValidationResult(errorMessage: $"only {_allowedExtensions } are allowed");
                }
            
            }
            return ValidationResult.Success;
        }

       
    }
}
