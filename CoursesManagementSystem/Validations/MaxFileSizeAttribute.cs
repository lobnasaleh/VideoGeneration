using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.Validations
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _MaxSize;
        public MaxFileSizeAttribute(int _MaxSize)
        {
            this._MaxSize = _MaxSize;
        }
        protected override ValidationResult IsValid(object value,
            ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
             
                if (file.Length > _MaxSize)
                {
                    return new ValidationResult(errorMessage: $"Maximum allowed file size is  {_MaxSize} Bytes");
                }
            }
            return ValidationResult.Success;
        }


    }
}
