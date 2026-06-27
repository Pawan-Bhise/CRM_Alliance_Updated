using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterSecure.Models
{
    [Table("SurveyGridColumn")]
    public class SurveyGridColumn
    {
        public int Id { get; set; }

        [Required]
        public int SurveyQuestionId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ColumnText { get; set; }

        public int DisplayOrder { get; set; }

        [ForeignKey("SurveyQuestionId")]
        public virtual SurveyQuestion SurveyQuestion { get; set; }
    }
}
