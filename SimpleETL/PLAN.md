# PLAN.md

## SimpleETL Project Development Plan

### 1. Project Initialization
- [x] Create .NET WPF project for Blazor Hybrid.
- [x] Update `.csproj` to support Razor and Blazor WebView.
- [x] Install Nuget packages (MudBlazor, Microsoft.AspNetCore.Components.WebView.Wpf, EF Core SQLite).
- [x] Set up `wwwroot` folder, `index.html`, and base Blazor files (`App.razor`, `_Imports.razor`, `MainLayout.razor`).

### 2. Database & Data Models (SQLite + EF Core)
- [x] Define Models: `DataFlow`, `JobLog`, `JobSchedule`.
- [x] Create `AppDbContext`.
- [x] Implement EF Core migrations (or auto-create database).

### 3. Core Architecture & Services
- [ ] Implement `FlowEngine` to execute the ETL pipeline.
- [ ] Implement `JobSchedulerService` using a simple background task for scheduling.
- [ ] Implement REST API server (embedded ASP.NET Core) for triggering flows.

### 4. UI/UX with MudBlazor
- [x] Configure MudBlazor theme (Light/Dark mode).
- [x] Create Navigation Menu:
  - Create / Open Data Flow
  - Job Monitoring & Logs
  - Settings
  - About
  - Exit

### 5. Drag-and-Drop Flow Designer
- [x] Implement interactive canvas for ETL building (mocked using custom UI).
- [x] Create components for Sources (SQL, API, CSV, etc.).
- [x] Create components for Transformations (Filter, Join, Mapping, Code Scripts, ML/LLM Mockups).
- [x] Create components for Destinations.
- [ ] Add Export/Import to JSON feature.

### 6. Mockups & Extensibility
- [x] Create mock dialogs/configurations for LLM Actions and ML Actions.
- [ ] Implement basic plugin loading concept.

### 7. Finalization
- [ ] Write `README.md` (English & Indonesian).
- [ ] Compile and test the application.
- [ ] Send project to user.