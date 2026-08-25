# RetailPulse

A retail sales analytics dashboard built with ASP.NET Core MVC that processes transactional data and turns it into interactive KPIs, visualisations, filtering and automated business insights.

## Overview

Retail businesses generate large amounts of transactional data, but raw data is difficult to interpret quickly. RetailPulse takes a CSV export of sales transactions, validates and cleans it, stores it in a SQLite database, and transforms it into meaningful KPIs, charts and business insights through an interactive dashboard.

RetailPulse is an **analytics application**, not a full retail management system — there is no inventory, ordering, or customer management. The focus is entirely on turning raw sales data into useful business understanding.

## Features

- CSV sales data import with server-side validation
- Data cleaning (trimming, numeric/date validation, rejection of invalid rows)
- Duplicate transaction detection on import
- SQLite storage via Entity Framework Core
- KPI dashboard (total revenue, units sold, average order value, top product)
- Revenue trend analysis over time
- Revenue by category analysis
- Top 5 products by revenue
- Revenue by region analysis
- Interactive dashboard filtering (date range, category, region) without full page reloads
- Automated, rule-based business insights, including month-over-month growth
- Responsive UI for desktop and mobile
- Friendly empty and error states

## Technologies

- C#
- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQLite
- CsvHelper
- JavaScript (vanilla, no framework)
- Chart.js
- Bootstrap 5
- HTML / CSS
- xUnit

## Architecture

```
CSV file
 ↓
CSV Import Service (validation & parsing)
 ↓
Data Cleaning (trimming, numeric/date checks, duplicate detection)
 ↓
SQLite Database (Entity Framework Core)
 ↓
Analytics Service (KPI, trend, category, region and insight calculations)
 ↓
Dashboard ViewModel
 ↓
Charts + KPIs + Business Insights (Dashboard view)
```

The project deliberately uses a small, flat structure rather than layered patterns like CQRS or a repository layer, because a single DbContext and two focused services are all this application needs:

```
RetailPulse/
├── Controllers/
│   ├── DashboardController.cs   Serves the dashboard and its JSON filter endpoint
│   └── ImportController.cs      Handles CSV upload
├── Data/
│   └── ApplicationDbContext.cs  EF Core context (single Sales table)
├── Models/
│   ├── Sale.cs                  The persisted entity
│   ├── SalesFilter.cs           Shared filter criteria
│   └── ViewModels/              Data shaped specifically for the views
├── Services/
│   ├── ICsvImportService.cs / CsvImportService.cs   Import, validation, cleaning
│   └── IAnalyticsService.cs / AnalyticsService.cs    KPIs, charts, insights
├── Views/
│   ├── Dashboard/
│   ├── Import/
│   └── Shared/
├── wwwroot/
│   ├── css/site.css
│   └── js/dashboard.js
├── sample-data/
│   └── sample-sales-data.csv
└── RetailPulse.Tests/           Unit tests for AnalyticsService
```

## Data Analytics

RetailPulse demonstrates several categories of analytics, all calculated directly from the imported dataset (nothing is hard-coded):

- **Descriptive analytics** — what happened: total revenue, units sold, average order value.
- **Diagnostic-style analysis** — what performed best: top product, best-performing category, best-performing region.
- **Trend analysis** — how performance is changing: revenue over time, month-over-month growth.
- **Business insights** — plain-language, rule-based observations generated from the above (e.g. "Electronics generated the highest revenue across all categories").

RetailPulse does **not** perform predictive analytics, forecasting, or machine learning of any kind — every figure is a calculation over historical data that already exists in the database.

## Screenshots

### Dashboard

![RetailPulse Dashboard](screenshots/dashboard.png)

### Data Import

![Data Import](screenshots/import.png)

### Analytics

![Analytics](screenshots/analytics.png)

## Setup

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download) and Visual Studio 2022 (or the `dotnet` CLI).

1. **Clone the repository**
   ```
   git clone https://github.com/<your-username>/RetailPulse.git
   cd RetailPulse
   ```

2. **Open the solution**
   Open `RetailPulse.sln` in Visual Studio, or open the folder from the CLI.

3. **Restore NuGet packages**
   Visual Studio restores packages automatically on open. From the CLI:
   ```
   dotnet restore
   ```

4. **Create the database with EF Core migrations**
   From the Package Manager Console (Default project: `RetailPulse`):
   ```
   Add-Migration InitialCreate
   Update-Database
   ```
   Or from the CLI, in the `RetailPulse` project folder:
   ```
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
   This creates a local `retailpulse.db` SQLite file. The app also calls `Database.Migrate()` on startup, so once a migration exists the database is created/updated automatically the next time you run it.

5. **Run the application**
   Press `F5` in Visual Studio, or:
   ```
   dotnet run --project RetailPulse
   ```

6. **Import the sample CSV**
   Open the app, go to **Import Data**, and upload `sample-data/sample-sales-data.csv`.

7. **Open the dashboard**
   Go to **Dashboard** to see the KPIs, charts and business insights generated from the imported data.

## Sample Dataset

`sample-data/sample-sales-data.csv` contains 220 realistic sales transactions spanning January–June 2026, across 14 products, 4 categories (Electronics, Accessories, Furniture, Office Equipment) and 5 South African regions (Gauteng, KwaZulu-Natal, Western Cape, Eastern Cape, Free State). Transaction volume increases gradually month over month, and Electronics sales are weighted more heavily toward Gauteng and KwaZulu-Natal, so the dashboard's trend and regional analysis produce genuinely interesting results rather than flat, identical numbers.

## Data Cleaning

Before any row is stored, `CsvImportService` performs basic data preparation, demonstrating a realistic (if intentionally simple) ETL step:

- Trims whitespace from every field
- Rejects rows with missing required fields
- Validates that `TransactionId` and `Quantity` are integers, and that `Quantity` is greater than zero
- Validates that `UnitPrice` and `Revenue` are numeric and not negative
- Validates that `Date` parses as a real date
- Skips rows whose `TransactionId` already exists in the database (or elsewhere in the same file), reporting how many were skipped as duplicates

Invalid rows are skipped rather than aborting the whole import, and the user is told how many records were imported, how many duplicates were skipped, and how many invalid rows were skipped.

## Testing

Unit tests cover the core analytics logic in `AnalyticsService` using small, hand-built datasets so expected results can be verified by hand: total revenue, average order value, top product, revenue by category, and month-over-month growth.

Run the tests from Visual Studio's Test Explorer, or from the CLI:

```
dotnet test
```

## What I Learned

- Processing and validating structured (CSV) data before persisting it
- Working with a relational database (SQLite) through Entity Framework Core, including migrations
- Designing a dedicated analytics service to keep business logic out of controllers
- Calculating business KPIs and rule-based insights directly from stored data
- Building interactive data visualisations with Chart.js
- Building a data-driven ASP.NET Core MVC application with a clean, simple architecture
- Writing focused unit tests for business logic rather than trivial code
- Separating concerns: import/validation, analytics, and presentation each live in their own layer

## Future Improvements

These are intentionally **not implemented** — they are realistic next steps for a production version of this project:

- Authentication and role-based dashboards (e.g. manager vs. analyst views)
- Exporting analytics to PDF or Excel
- Predictive sales forecasting
- Real-time data integration (e.g. POS system feeds)
- Advanced anomaly detection

## License

This is a personal portfolio project, shared for demonstration purposes.
