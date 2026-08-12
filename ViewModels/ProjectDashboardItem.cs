using System;
using FreelanceFlow.Models;
using FreelanceFlow.ViewModels.Base;

namespace FreelanceFlow.ViewModels
{
    public class ProjectDashboardItem : ViewModelBase
    {
        private Project _project = null!;
        private TimeSpan _hoursToday;
        private TimeSpan _hoursThisWeek;
        private TimeSpan _totalHours;
        private decimal _earningsToday;
        private decimal _earningsThisWeek;
        private decimal _totalEarnings;
        private bool _isTimerRunning;

        public Project Project
        {
            get => _project;
            set => SetProperty(ref _project, value);
        }

        public int Id => Project?.Id ?? 0;
        public string ProjectName => Project?.Name ?? string.Empty;
        public string ClientName => Project?.Client?.Name ?? string.Empty;
        public decimal HourlyRate => Project?.HourlyRate ?? 0;
        public ProjectStatus Status => Project?.Status ?? ProjectStatus.Active;

        public TimeSpan HoursToday
        {
            get => _hoursToday;
            set
            {
                if (SetProperty(ref _hoursToday, value))
                {
                    OnPropertyChanged(nameof(HoursTodayFormatted));
                    EarningsToday = (decimal)value.TotalHours * HourlyRate;
                }
            }
        }

        public TimeSpan HoursThisWeek
        {
            get => _hoursThisWeek;
            set
            {
                if (SetProperty(ref _hoursThisWeek, value))
                {
                    OnPropertyChanged(nameof(HoursThisWeekFormatted));
                    EarningsThisWeek = (decimal)value.TotalHours * HourlyRate;
                }
            }
        }

        public TimeSpan TotalHours
        {
            get => _totalHours;
            set
            {
                if (SetProperty(ref _totalHours, value))
                {
                    OnPropertyChanged(nameof(TotalHoursFormatted));
                    TotalEarnings = (decimal)value.TotalHours * HourlyRate;
                }
            }
        }

        public decimal EarningsToday
        {
            get => _earningsToday;
            set => SetProperty(ref _earningsToday, value);
        }

        public decimal EarningsThisWeek
        {
            get => _earningsThisWeek;
            set => SetProperty(ref _earningsThisWeek, value);
        }

        public decimal TotalEarnings
        {
            get => _totalEarnings;
            set => SetProperty(ref _totalEarnings, value);
        }

        public bool IsTimerRunning
        {
            get => _isTimerRunning;
            set => SetProperty(ref _isTimerRunning, value);
        }

        public string HoursTodayFormatted => FormatTimeSpan(HoursToday);
        public string HoursThisWeekFormatted => FormatTimeSpan(HoursThisWeek);
        public string TotalHoursFormatted => FormatTimeSpan(TotalHours);

        private static string FormatTimeSpan(TimeSpan ts)
        {
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        public void RefreshDisplayProperties()
        {
            OnPropertiesChanged(
                nameof(ProjectName),
                nameof(ClientName),
                nameof(HourlyRate),
                nameof(Status)
            );
        }
    }
}