using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterSecure.Models
{
    [Table("SurveyCustomerFormTracking")]
    public class SurveyCustomerFormTracking
    {
        public int Id { get; set; }

        [Required]
        public int SurveyCustomerDataId { get; set; }

        [Required]
        public int SurveyTemplateTypeId { get; set; }

        [Required]
        public int SurveyFormId { get; set; }

        public int? CallStatusId { get; set; }

        public int? FormStatusId { get; set; }

        [MaxLength(1000)]
        public string CallRemarks { get; set; }

        [MaxLength(100)]
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [ForeignKey("SurveyCustomerDataId")]
        public virtual SurveyCustomerData SurveyCustomerData { get; set; }

        [ForeignKey("SurveyTemplateTypeId")]
        public virtual SurveyTemplateType SurveyTemplateType { get; set; }

        [ForeignKey("SurveyFormId")]
        public virtual SurveyForm SurveyForm { get; set; }

        [ForeignKey("CallStatusId")]
        public virtual SurveyCallStatusMaster CallStatus { get; set; }

        [ForeignKey("FormStatusId")]
        public virtual SurveyFormStatusMaster FormStatus { get; set; }
    }
}