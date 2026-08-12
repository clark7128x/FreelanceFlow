using System.Linq;
using FreelanceFlow.Models;
using FreelanceFlow.ViewModels.Base;

namespace FreelanceFlow.ViewModels
{
    public class ClientViewModel : ViewModelBase
    {
        private Client _client = null!;

        public Client Client
        {
            get => _client;
            set
            {
                if (SetProperty(ref _client, value))
                {
                    OnPropertiesChanged(
                        nameof(Id),
                        nameof(Name),
                        nameof(Email),
                        nameof(DefaultHourlyRate),
                        nameof(ProjectCount),
                        nameof(TotalEarnings)
                    );
                }
            }
        }

        public int Id => Client?.Id ?? 0;

        public string Name
        {
            get => Client?.Name ?? string.Empty;
            set
            {
                if (Client != null && Client.Name != value)
                {
                    Client.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Email
        {
            get => Client?.Email ?? string.Empty;
            set
            {
                if (Client != null && Client.Email != value)
                {
                    Client.Email = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal DefaultHourlyRate
        {
            get => Client?.DefaultHourlyRate ?? 0;
            set
            {
                if (Client != null && Client.DefaultHourlyRate != value)
                {
                    Client.DefaultHourlyRate = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ProjectCount => Client?.Projects?.Count ?? 0;

        public decimal TotalEarnings => Client?.Projects?
            .SelectMany(p => p.TimeLogs)
            .Where(t => t.EndTime.HasValue)
            .Sum(t => t.Earnings) ?? 0;
    }
}