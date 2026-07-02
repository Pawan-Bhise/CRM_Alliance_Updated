using System;
using System.ComponentModel.DataAnnotations;

namespace CallCenterSecure.Models
{
    public class UploadJob
    {
        [Key]
        public int UploadJobId { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FilePath { get; set; }

        [Required]
        public string Status { get; set; }

        public string Message { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? StartedOn { get; set; }
        public DateTime? CompletedOn { get; set; }
        public int? ProcessedRows { get; set; }
    }
}
