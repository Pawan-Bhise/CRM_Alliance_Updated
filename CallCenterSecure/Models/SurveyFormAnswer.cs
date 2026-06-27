using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterSecure.Models
{
    [Table("SurveyFormAnswer")]
    public class SurveyFormAnswer
    {
        public SurveyFormAnswer()
        {
            GridAnswers = new List<SurveyFormGridAnswer>();
        }

        public int Id { get; set; }

        [Required]
        public int SurveyFormResponseId { get; set; }

        [Required]
        public int SurveyQuestionId { get; set; }

        [MaxLength(2000)]
        public string AnswerText { get; set; }

        [MaxLength(500)]
        public string SelectedOption { get; set; }

        [MaxLength(1000)]
        public string SelectedOptionsCsv { get; set; }

        [MaxLength(255)]
        public string FileName { get; set; }

        [MaxLength(500)]
        public string FilePath { get; set; }

        [ForeignKey("SurveyFormResponseId")]
        public virtual SurveyFormResponse SurveyFormResponse { get; set; }

        [ForeignKey("SurveyQuestionId")]
        public virtual SurveyQuestion SurveyQuestion { get; set; }

        public virtual ICollection<SurveyFormGridAnswer> GridAnswers { get; set; }
    }
}
