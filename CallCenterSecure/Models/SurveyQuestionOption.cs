using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterSecure.Models
{
    [Table("SurveyQuestionOption")]
    public class SurveyQuestionOption
    {
        public int Id { get; set; }

        [Required]
        public int SurveyQuestionId { get; set; }

        [Required]
        [MaxLength(500)]
        public string OptionText { get; set; }

        public int DisplayOrder { get; set; }

        [ForeignKey("SurveyQuestionId")]
        public virtual SurveyQuestion SurveyQuestion { get; set; }
    }
}
