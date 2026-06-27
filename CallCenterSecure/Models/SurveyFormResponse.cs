using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterSecure.Models
{
    [Table("SurveyFormResponse")]
    public class SurveyFormResponse
    {
        public SurveyFormResponse()
        {
            Answers = new List<SurveyFormAnswer>();
        }

        public int Id { get; set; }

        [Required]
        public int SurveyFormId { get; set; }

        public int? SurveyCustomerDataId { get; set; }

        [MaxLength(100)]
        public string RespondentName { get; set; }

        [MaxLength(50)]
        public string RespondentMobile { get; set; }

        [Required]
        [MaxLength(100)]
        public string SubmittedBy { get; set; }

        public DateTime SubmittedDate { get; set; }

        [ForeignKey("SurveyFormId")]
        public virtual SurveyForm SurveyForm { get; set; }

        [ForeignKey("SurveyCustomerDataId")]
        public virtual SurveyCustomerData SurveyCustomerData { get; set; }

        public virtual ICollection<SurveyFormAnswer> Answers { get; set; }
    }
}
