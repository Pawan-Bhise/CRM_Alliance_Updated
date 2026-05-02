using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterSecure.Models
{
    [Table("SurveyCustomerData")]
    public class SurveyCustomerData
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ClientName { get; set; }

        [MaxLength(20)]
        public string Gender { get; set; }

        [Required]
        [MaxLength(50)]
        public string CustomerCode { get; set; }

        [MaxLength(30)]
        public string MobileNumber1 { get; set; }

        [MaxLength(30)]
        public string MobileNumber2 { get; set; }

        [MaxLength(100)]
        public string Region { get; set; }

        [MaxLength(100)]
        public string Branch { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        [MaxLength(100)]
        public string LoanProduct { get; set; }

        public int? Age { get; set; }

        [MaxLength(100)]
        public string BusinessCategory { get; set; }

        [MaxLength(200)]
        public string ActivitiesSector { get; set; }

        [MaxLength(50)]
        public string LevelOfEducation { get; set; }

        [MaxLength(100)]
        public string IncomeLevel { get; set; }

        [MaxLength(200)]
        public string HouseholdAssets { get; set; }

        public int? PovertyScore { get; set; }

        public int? SurveyTemplateTypeId { get; set; }

        [ForeignKey("SurveyTemplateTypeId")]
        public virtual SurveyTemplateType SurveyTemplateType { get; set; }
    }
}