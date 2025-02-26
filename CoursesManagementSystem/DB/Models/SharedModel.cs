using System.ComponentModel.DataAnnotations;

namespace CoursesManagementSystem.DB.Models
{
    public abstract class SharedModel
    {
        [Key]
        public int ID { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        public string LastModifiedBy { get; set; }

        public bool IsDeleted { get; set; }
    }
}
