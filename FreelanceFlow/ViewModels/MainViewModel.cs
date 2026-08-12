using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using FreelanceFlow.Data;
using FreelanceFlow.Models;
using FreelanceFlow.ViewModels.Base;

namespace FreelanceFlow.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private readonly DispatcherTimer _timer;
        private TimeLog? _activeTimeLog;
        private DateTime _timerStartTime;

        private bool _isLoading;
        private string _statusMessage = "Ready";
        private string _currentView = "Dashboard";

        private bool _isTimerRunning;
        private TimeSpan _elapsedTime;
        private string _currentTaskDescription = string.Empty;

        private ProjectDashboardItem? _selectedProject;
        private Client? _selectedClient;
        private TimeLog? _selectedTimeLog;

        private string _newClientName = string.Empty;
        private string _newClientEmail = string.Empty;
        private decimal _newClientRate = 50.00m;

        private string _newProjectName = string.Empty;
        private string _newProjectDescription = string.Empty;
        private decimal _newProjectRate = 50.00m;

        private TimeSpan _totalHoursToday;
        private TimeSpan _totalHoursThisWeek;
        private decimal _totalEarningsToday;
        private decimal _totalEarningsThisWeek;

        #endregion

        #region Collections

        public ObservableCollection<ProjectDashboardItem> DashboardProjects { get; } = new();
        public ObservableCollection<Client> Clients { get; } = new();
        public ObservableCollection<Project> Projects { get; } = new();
        public ObservableCollection<TimeLog> RecentTimeLogs { get; } = new();

        #endregion

        #region Properties

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public bool IsTimerRunning
        {
            get => _isTimerRunning;
            set
            {
                if (SetProperty(ref _isTimerRunning, value))
                {
                    OnPropertyChanged(nameof(TimerButtonText));
                    OnPropertyChanged(nameof(TimerButtonIcon));
                }
            }
        }

        public TimeSpan ElapsedTime
        {
            get => _elapsedTime;
            set
            {
                if (SetProperty(ref _elapsedTime, value))
                {
                    OnPropertyChanged(nameof(ElapsedTimeFormatted));
                }
            }
        }

        public string ElapsedTimeFormatted =>
            $"{(int)ElapsedTime.TotalHours:D2}:{ElapsedTime.Minutes:D2}:{ElapsedTime.Seconds:D2}";

        public string TimerButtonText => IsTimerRunning ? "Stop Timer" : "Start Timer";
        public string TimerButtonIcon => IsTimerRunning ? "■" : "▶";

        public string CurrentTaskDescription
        {
            get => _currentTaskDescription;
            set => SetProperty(ref _currentTaskDescription, value);
        }

        public ProjectDashboardItem? SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (SetProperty(ref _selectedProject, value))
                {
                    ((RelayCommand)ToggleTimerCommand).RaiseCanExecuteChanged();
                    ((AsyncRelayCommand)DeleteProjectCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (SetProperty(ref _selectedClient, value))
                {
                    ((AsyncRelayCommand)DeleteClientCommand).RaiseCanExecuteChanged();
                    if (value != null)
                    {
                        NewProjectRate = value.DefaultHourlyRate;
                    }
                }
            }
        }

        public TimeLog? SelectedTimeLog
        {
            get => _selectedTimeLog;
            set
            {
                if (SetProperty(ref _selectedTimeLog, value))
                {
                    ((AsyncRelayCommand)DeleteTimeLogCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string NewClientName
        {
            get => _newClientName;
            set => SetProperty(ref _newClientName, value);
        }

        public string NewClientEmail
        {
            get => _newClientEmail;
            set => SetProperty(ref _newClientEmail, value);
        }

        public decimal NewClientRate
        {
            get => _newClientRate;
            set => SetProperty(ref _newClientRate, value);
        }

        public string NewProjectName
        {
            get => _newProjectName;
            set => SetProperty(ref _newProjectName, value);
        }

        public string NewProjectDescription
        {
            get => _newProjectDescription;
            set => SetProperty(ref _newProjectDescription, value);
        }

        public decimal NewProjectRate
        {
            get => _newProjectRate;
            set => SetProperty(ref _newProjectRate, value);
        }

        public TimeSpan TotalHoursToday
        {
            get => _totalHoursToday;
            set
            {
                if (SetProperty(ref _totalHoursToday, value))
                    OnPropertyChanged(nameof(TotalHoursTodayFormatted));
            }
        }

        public TimeSpan TotalHoursThisWeek
        {
            get => _totalHoursThisWeek;
            set
            {
                if (SetProperty(ref _totalHoursThisWeek, value))
                    OnPropertyChanged(nameof(TotalHoursThisWeekFormatted));
            }
        }

        public decimal TotalEarningsToday
        {
            get => _totalEarningsToday;
            set => SetProperty(ref _totalEarningsToday, value);
        }

        public decimal TotalEarningsThisWeek
        {
            get => _totalEarningsThisWeek;
            set => SetProperty(ref _totalEarningsThisWeek, value);
        }

        public string TotalHoursTodayFormatted =>
            $"{(int)TotalHoursToday.TotalHours:D2}:{TotalHoursToday.Minutes:D2}";

        public string TotalHoursThisWeekFormatted =>
            $"{(int)TotalHoursThisWeek.TotalHours:D2}:{TotalHoursThisWeek.Minutes:D2}";

        #endregion

        #region Commands

        public RelayCommand ToggleTimerCommand { get; }
        public AsyncRelayCommand LoadDataCommand { get; }
        public AsyncRelayCommand RefreshDashboardCommand { get; }
        public AsyncRelayCommand AddClientCommand { get; }
        public AsyncRelayCommand AddProjectCommand { get; }
        public AsyncRelayCommand DeleteClientCommand { get; }
        public AsyncRelayCommand DeleteProjectCommand { get; }
        public AsyncRelayCommand DeleteTimeLogCommand { get; }
        public AsyncRelayCommand ExportInvoiceCommand { get; }
        public AsyncRelayCommand ExportAllInvoicesCommand { get; }
        public RelayCommand<string> NavigateCommand { get; }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;

            ToggleTimerCommand = new RelayCommand(
                ExecuteToggleTimer,
                () => SelectedProject != null);

            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            RefreshDashboardCommand = new AsyncRelayCommand(RefreshDashboardAsync);

            AddClientCommand = new AsyncRelayCommand(
                AddClientAsync,
                () => !string.IsNullOrWhiteSpace(NewClientName));

            AddProjectCommand = new AsyncRelayCommand(
                AddProjectAsync,
                () => !string.IsNullOrWhiteSpace(NewProjectName) && SelectedClient != null);

            DeleteClientCommand = new AsyncRelayCommand(
                DeleteClientAsync,
                () => SelectedClient != null);

            DeleteProjectCommand = new AsyncRelayCommand(
                DeleteProjectAsync,
                () => SelectedProject != null && !IsTimerRunning);

            DeleteTimeLogCommand = new AsyncRelayCommand(
                DeleteTimeLogAsync,
                () => SelectedTimeLog != null && !SelectedTimeLog.IsRunning);

            ExportInvoiceCommand = new AsyncRelayCommand(
                ExportInvoiceAsync,
                () => SelectedProject != null);

            ExportAllInvoicesCommand = new AsyncRelayCommand(ExportAllInvoicesAsync);

            NavigateCommand = new RelayCommand<string>(view =>
            {
                if (!string.IsNullOrEmpty(view))
                    CurrentView = view;
            });

            _ = LoadDataAsync();
        }

        #endregion

        #region Timer Methods

        private void Timer_Tick(object? sender, EventArgs e)
        {
            ElapsedTime = DateTime.Now - _timerStartTime;

            if (SelectedProject != null)
            {
                CalculateProjectHours(SelectedProject);
            }

            UpdateTotalStatistics();
        }

        private void ExecuteToggleTimer()
        {
            if (SelectedProject == null) return;

            if (IsTimerRunning)
            {
                StopTimer();
            }
            else
            {
                StartTimer();
            }
        }

        private async void StartTimer()
        {
            if (SelectedProject == null) return;

            try
            {
                await using var context = new FreelanceFlowDbContext();

                _activeTimeLog = new TimeLog
                {
                    ProjectId = SelectedProject.Id,
                    StartTime = DateTime.Now,
                    Description = CurrentTaskDescription,
                    IsBillable = true
                };

                context.TimeLogs.Add(_activeTimeLog);
                await context.SaveChangesAsync();

                _timerStartTime = _activeTimeLog.StartTime;
                IsTimerRunning = true;
                SelectedProject.IsTimerRunning = true;
                _timer.Start();

                StatusMessage = $"Timer started for {SelectedProject.ProjectName}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error starting timer: {ex.Message}";
            }
        }

        private async void StopTimer()
        {
            if (_activeTimeLog == null) return;

            try
            {
                _timer.Stop();

                await using var context = new FreelanceFlowDbContext();
                var timeLog = await context.TimeLogs.FindAsync(_activeTimeLog.Id);

                if (timeLog != null)
                {
                    timeLog.EndTime = DateTime.Now;
                    timeLog.Description = CurrentTaskDescription;
                    await context.SaveChangesAsync();
                }

                IsTimerRunning = false;
                if (SelectedProject != null)
                {
                    SelectedProject.IsTimerRunning = false;
                }

                StatusMessage = $"Timer stopped. Duration: {ElapsedTimeFormatted}";
                ElapsedTime = TimeSpan.Zero;
                CurrentTaskDescription = string.Empty;
                _activeTimeLog = null;

                await RefreshDashboardAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error stopping timer: {ex.Message}";
            }
        }

        #endregion

        #region Data Loading Methods

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            StatusMessage = "Loading data...";

            try
            {
                await using var context = new FreelanceFlowDbContext();

                await context.Database.EnsureCreatedAsync();

                var clients = await context.Clients
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                Clients.Clear();
                foreach (var client in clients)
                {
                    Clients.Add(client);
                }

                await RefreshDashboardAsync();

                StatusMessage = "Data loaded successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshDashboardAsync()
        {
            try
            {
                await using var context = new FreelanceFlowDbContext();

                var projects = await context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.TimeLogs)
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Client.Name)
                    .ThenBy(p => p.Name)
                    .ToListAsync();

                DashboardProjects.Clear();

                foreach (var project in projects)
                {
                    var dashboardItem = new ProjectDashboardItem { Project = project };
                    CalculateProjectHours(dashboardItem);
                    DashboardProjects.Add(dashboardItem);
                }

                var recentLogs = await context.TimeLogs
                    .Include(t => t.Project)
                    .ThenInclude(p => p.Client)
                    .OrderByDescending(t => t.StartTime)
                    .Take(20)
                    .ToListAsync();

                RecentTimeLogs.Clear();
                foreach (var log in recentLogs)
                {
                    RecentTimeLogs.Add(log);
                }

                UpdateTotalStatistics();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing dashboard: {ex.Message}";
            }
        }

        private void CalculateProjectHours(ProjectDashboardItem item)
        {
            if (item.Project?.TimeLogs == null) return;

            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);

            var completedLogs = item.Project.TimeLogs.Where(t => t.EndTime.HasValue).ToList();
            var runningLog = item.Project.TimeLogs.FirstOrDefault(t => !t.EndTime.HasValue);

            var hoursToday = completedLogs
                .Where(t => t.StartTime.Date == today)
                .Aggregate(TimeSpan.Zero, (sum, log) => sum + log.Duration);

            if (runningLog != null && runningLog.StartTime.Date == today)
            {
                hoursToday += runningLog.Duration;
            }

            var hoursThisWeek = completedLogs
                .Where(t => t.StartTime.Date >= weekStart)
                .Aggregate(TimeSpan.Zero, (sum, log) => sum + log.Duration);

            if (runningLog != null && runningLog.StartTime.Date >= weekStart)
            {
                hoursThisWeek += runningLog.Duration;
            }

            var totalHours = completedLogs
                .Aggregate(TimeSpan.Zero, (sum, log) => sum + log.Duration);

            if (runningLog != null)
            {
                totalHours += runningLog.Duration;
            }

            item.HoursToday = hoursToday;
            item.HoursThisWeek = hoursThisWeek;
            item.TotalHours = totalHours;
        }

        private void UpdateTotalStatistics()
        {
            TotalHoursToday = DashboardProjects.Aggregate(
                TimeSpan.Zero, (sum, p) => sum + p.HoursToday);

            TotalHoursThisWeek = DashboardProjects.Aggregate(
                TimeSpan.Zero, (sum, p) => sum + p.HoursThisWeek);

            TotalEarningsToday = DashboardProjects.Sum(p => p.EarningsToday);
            TotalEarningsThisWeek = DashboardProjects.Sum(p => p.EarningsThisWeek);
        }

        #endregion

        #region CRUD Operations

        private async Task AddClientAsync()
        {
            if (string.IsNullOrWhiteSpace(NewClientName)) return;

            try
            {
                await using var context = new FreelanceFlowDbContext();

                var client = new Client
                {
                    Name = NewClientName.Trim(),
                    Email = NewClientEmail.Trim(),
                    DefaultHourlyRate = NewClientRate,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                context.Clients.Add(client);
                await context.SaveChangesAsync();

                Clients.Add(client);

                NewClientName = string.Empty;
                NewClientEmail = string.Empty;
                NewClientRate = 50.00m;

                StatusMessage = $"Client '{client.Name}' added successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding client: {ex.Message}";
            }
        }

        private async Task AddProjectAsync()
        {
            if (string.IsNullOrWhiteSpace(NewProjectName) || SelectedClient == null) return;

            try
            {
                await using var context = new FreelanceFlowDbContext();

                var project = new Project
                {
                    Name = NewProjectName.Trim(),
                    Description = NewProjectDescription.Trim(),
                    HourlyRate = NewProjectRate,
                    ClientId = SelectedClient.Id,
                    CreatedAt = DateTime.Now,
                    Status = ProjectStatus.Active,
                    IsActive = true
                };

                context.Projects.Add(project);
                await context.SaveChangesAsync();

                await context.Entry(project).Reference(p => p.Client).LoadAsync();

                var dashboardItem = new ProjectDashboardItem { Project = project };
                DashboardProjects.Add(dashboardItem);

                NewProjectName = string.Empty;
                NewProjectDescription = string.Empty;

                StatusMessage = $"Project '{project.Name}' added successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding project: {ex.Message}";
            }
        }

        private async Task DeleteClientAsync()
        {
            if (SelectedClient == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedClient.Name}' and all associated projects?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await using var context = new FreelanceFlowDbContext();

                var client = await context.Clients.FindAsync(SelectedClient.Id);
                if (client != null)
                {
                    context.Clients.Remove(client);
                    await context.SaveChangesAsync();

                    Clients.Remove(SelectedClient);

                    var projectsToRemove = DashboardProjects
                        .Where(p => p.Project.ClientId == client.Id)
                        .ToList();

                    foreach (var project in projectsToRemove)
                    {
                        DashboardProjects.Remove(project);
                    }

                    StatusMessage = $"Client '{client.Name}' deleted successfully";
                    SelectedClient = null;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting client: {ex.Message}";
            }
        }

        private async Task DeleteProjectAsync()
        {
            if (SelectedProject == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedProject.ProjectName}' and all time logs?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await using var context = new FreelanceFlowDbContext();

                var project = await context.Projects.FindAsync(SelectedProject.Id);
                if (project != null)
                {
                    context.Projects.Remove(project);
                    await context.SaveChangesAsync();

                    DashboardProjects.Remove(SelectedProject);
                    StatusMessage = $"Project '{project.Name}' deleted successfully";
                    SelectedProject = null;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting project: {ex.Message}";
            }
        }

        private async Task DeleteTimeLogAsync()
        {
            if (SelectedTimeLog == null) return;

            var result = MessageBox.Show(
                "Are you sure you want to delete this time log?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await using var context = new FreelanceFlowDbContext();

                var timeLog = await context.TimeLogs.FindAsync(SelectedTimeLog.Id);
                if (timeLog != null)
                {
                    context.TimeLogs.Remove(timeLog);
                    await context.SaveChangesAsync();

                    RecentTimeLogs.Remove(SelectedTimeLog);
                    await RefreshDashboardAsync();

                    StatusMessage = "Time log deleted successfully";
                    SelectedTimeLog = null;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting time log: {ex.Message}";
            }
        }

        #endregion

        #region Invoice Export

        private async Task ExportInvoiceAsync()
        {
            if (SelectedProject == null) return;

            try
            {
                await using var context = new FreelanceFlowDbContext();

                var project = await context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.TimeLogs)
                    .FirstOrDefaultAsync(p => p.Id == SelectedProject.Id);

                if (project == null) return;

                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Invoice_{project.Client.Name}_{project.Name}_{DateTime.Now:yyyyMMdd}",
                    DefaultExt = ".csv",
                    Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt"
                };

                if (dialog.ShowDialog() == true)
                {
                    await GenerateInvoiceFileAsync(dialog.FileName, new[] { project });
                    StatusMessage = $"Invoice exported to {dialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting invoice: {ex.Message}";
            }
        }

        private async Task ExportAllInvoicesAsync()
        {
            try
            {
                await using var context = new FreelanceFlowDbContext();

                var projects = await context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.TimeLogs)
                    .Where(p => p.IsActive && p.TimeLogs.Any(t => t.EndTime.HasValue))
                    .ToListAsync();

                if (!projects.Any())
                {
                    StatusMessage = "No billable projects found";
                    return;
                }

                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Invoice_All_{DateTime.Now:yyyyMMdd}",
                    DefaultExt = ".csv",
                    Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt"
                };

                if (dialog.ShowDialog() == true)
                {
                    await GenerateInvoiceFileAsync(dialog.FileName, projects);
                    StatusMessage = $"All invoices exported to {dialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting invoices: {ex.Message}";
            }
        }

        private static async Task GenerateInvoiceFileAsync(string filePath, IEnumerable<Project> projects)
        {
            var lines = new List<string>
            {
                "===============================================================================",
                "                        FREELANCEFLOW INVOICE                                  ",
                "===============================================================================",
                $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                "",
                "Client Name          | Project                   | Total Hours | Rate/Hr    | Total Cost",
                "-------------------------------------------------------------------------------"
            };

            decimal grandTotal = 0;
            TimeSpan totalHours = TimeSpan.Zero;

            foreach (var project in projects)
            {
                var projectHours = project.TimeLogs
                    .Where(t => t.EndTime.HasValue)
                    .Aggregate(TimeSpan.Zero, (sum, t) => sum + t.Duration);

                var totalCost = (decimal)projectHours.TotalHours * project.HourlyRate;
                grandTotal += totalCost;
                totalHours += projectHours;

                var hoursFormatted = $"{projectHours.TotalHours:F2}";
                lines.Add($"{project.Client.Name,-20} | {project.Name,-25} | {hoursFormatted,11} | ${project.HourlyRate,9:F2} | ${totalCost,11:F2}");
            }

            lines.Add("-------------------------------------------------------------------------------");
            lines.Add($"{"TOTAL",-20} | {"",-25} | {totalHours.TotalHours,11:F2} | {"",10} | ${grandTotal,11:F2}");
            lines.Add("===============================================================================");
            lines.Add("");
            lines.Add("Detailed Time Logs:");
            lines.Add("-------------------------------------------------------------------------------");

            foreach (var project in projects)
            {
                lines.Add($"\n{project.Client.Name} - {project.Name}:");

                foreach (var log in project.TimeLogs.Where(t => t.EndTime.HasValue).OrderBy(t => t.StartTime))
                {
                    lines.Add($"  {log.StartTime:yyyy-MM-dd HH:mm} - {log.EndTime:HH:mm} ({log.Duration.TotalHours:F2}h): {log.Description}");
                }
            }

            await File.WriteAllLinesAsync(filePath, lines);
        }

        #endregion
    }
}