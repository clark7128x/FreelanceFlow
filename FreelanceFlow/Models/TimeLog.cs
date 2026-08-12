using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreelanceFlow.Models
{
    public class TimeLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsBillable { get; set; } = true;

        public int ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public virtual Project Project { get; set; } = null!;

        [NotMapped]
        public TimeSpan Duration
        {
            get
            {
                if (EndTime.HasValue)
                    return EndTime.Value - StartTime;
                return DateTime.Now - StartTime;
            }
        }

        [NotMapped]
        public bool IsRunning => !EndTime.HasValue;

        [NotMapped]
        public decimal Earnings => IsBillable && Project != null
            ? (decimal)Duration.TotalHours * Project.HourlyRate
            : 0;
    }
}