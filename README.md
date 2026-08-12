# FreelanceFlow

A Windows desktop app for freelancers to track clients, projects, and billable time — and turn logged hours straight into an invoice, without a spreadsheet in sight.

## Features

- **Client & project management** — clients each have a default hourly rate; projects belong to a client and can override it; project status (Active / On Hold / Completed / Cancelled)
- **Live time tracking** — start/stop timer per project with a running elapsed-time display, backed by a real `TimeLog` record from the moment you hit start (not just a client-side stopwatch)
- **Dashboard** — hours and earnings today and this week, per project and in total, recalculated live while the timer runs
- **Invoice export** — generate a formatted, itemized invoice (single project or every billable project at once) as a plain-text/CSV file, with per-project totals and a detailed time-log breakdown
- **Dark theme** by default

## Tech stack

- **WPF** (.NET 8) with the **MVVM** pattern — `RelayCommand`/`AsyncRelayCommand`, `ObservableCollection`, value converters for status colors and visibility
- **Entity Framework Core** with **SQLite** — the database is created automatically on first run, no separate setup step
- No external services — fully offline, single-user, local-first

## Getting started

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) and Windows (WPF is Windows-only).

```bash
git clone <this-repo>
cd FreelanceFlow/FreelanceFlow
dotnet run --project FreelanceFlow
```

The SQLite database file is created automatically the first time the app runs — there's nothing to configure.

## Project structure

```
FreelanceFlow/
  Models/         Client, Project, TimeLog (+ ProjectStatus enum)
  Data/           EF Core DbContext
  ViewModels/     MainViewModel (dashboard, timer, CRUD, invoice export)
  Views/          MainWindow (XAML)
  Converters/     Bool→Visibility, Status→Color, TimeSpan→String
  Themes/         Dark theme resource dictionary
```