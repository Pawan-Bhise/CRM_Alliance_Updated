using System.ComponentModel.DataAnnotations;

namespace CallCenterSecure.Models
{
    public class SurveyTemplateViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Template Name")]
        public string Name { get; set; }
    }
}
