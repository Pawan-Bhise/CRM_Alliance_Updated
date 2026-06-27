using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterSecure.Models
{
    [Table("SurveyForm")]
    public class SurveyForm
    {
        public SurveyForm()
        {
            Questions = new List<SurveyQuestion>();
            IsActive = true;
        }

        public int Id { get; set; }

        [Required]
        public int SurveyTemplateId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(100)]
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool IsActive { get; set; }

        [ForeignKey("SurveyTemplateId")]
        public virtual SurveyTemplateType SurveyTemplate { get; set; }

        public virtual ICollection<SurveyQuestion> Questions { get; set; }
    }
}
