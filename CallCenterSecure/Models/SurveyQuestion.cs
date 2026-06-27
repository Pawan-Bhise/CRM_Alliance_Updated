using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterSecure.Models
{
    [Table("SurveyQuestion")]
    public class SurveyQuestion
    {
        public SurveyQuestion()
        {
            Options = new List<SurveyQuestionOption>();
            GridRows = new List<SurveyGridRow>();
            GridColumns = new List<SurveyGridColumn>();
        }

        public int Id { get; set; }

        [Required]
        public int SurveyFormId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string QuestionText { get; set; }

        [Required]
        [MaxLength(50)]
        public string QuestionType { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsRequired { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? MinValue { get; set; }

        public int? MaxValue { get; set; }

        [MaxLength(100)]
        public string MinLabel { get; set; }

        [MaxLength(100)]
        public string MaxLabel { get; set; }

        [ForeignKey("SurveyFormId")]
        public virtual SurveyForm SurveyForm { get; set; }

        public virtual ICollection<SurveyQuestionOption> Options { get; set; }

        public virtual ICollection<SurveyGridRow> GridRows { get; set; }

        public virtual ICollection<SurveyGridColumn> GridColumns { get; set; }
    }
}
