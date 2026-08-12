using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace FreelanceFlow.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? Deadline { get; set; }

        public bool IsActive { get; set; } = true;

        public ProjectStatus Status { get; set; } = ProjectStatus.Active;

        public int ClientId { get; set; }

        [ForeignKey(nameof(ClientId))]
        public virtual Client Client { get; set; } = null!;

        public virtual ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();

        [NotMapped]
        public TimeSpan TotalTimeWorked => TimeLogs
            .Where(t => t.EndTime.HasValue)
            .Aggregate(TimeSpan.Zero, (sum, log) => sum + log.Duration);

        [NotMapped]
        public decimal TotalEarnings => (decimal)TotalTimeWorked.TotalHours * HourlyRate;
    }

    public enum ProjectStatus
    {
        Active,
        OnHold,
        Completed,
        Cancelled
    }
}