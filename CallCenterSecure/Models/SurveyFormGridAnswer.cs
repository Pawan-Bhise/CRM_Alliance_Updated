using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallCenterSecure.Models
{
    [Table("SurveyFormGridAnswer")]
    public class SurveyFormGridAnswer
    {
        public int Id { get; set; }

        [Required]
        public int SurveyFormAnswerId { get; set; }

        [Required]
        [MaxLength(500)]
        public string RowText { get; set; }

        [MaxLength(500)]
        public string SelectedColumnText { get; set; }

        [MaxLength(1000)]
        public string SelectedColumnTextsCsv { get; set; }

        [ForeignKey("SurveyFormAnswerId")]
        public virtual SurveyFormAnswer SurveyFormAnswer { get; set; }
    }
}
