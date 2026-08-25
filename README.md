# 📊 RetailPulse

### Retail Sales Analytics Dashboard

**RetailPulse** is a data analytics dashboard built with **ASP.NET Core MVC** that transforms raw retail transaction data into meaningful business insights.

The application imports sales data from CSV files, validates and cleans the data, stores it in a SQLite database, and calculates **KPIs, trends, product performance, regional performance, and automated business insights** through an interactive dashboard.

> **RetailPulse focuses on analytics rather than retail management — turning transactional data into information that can support better business decisions.**

---

## 🚀 Features

| Feature                       | Description                                              |
| ----------------------------- | -------------------------------------------------------- |
| 📥 **CSV Import**             | Import retail sales transactions from CSV files          |
| 🧹 **Data Cleaning**          | Validate, trim and reject invalid records                |
| 🔍 **Duplicate Detection**    | Prevent duplicate transactions from being imported       |
| 🗄️ **Data Persistence**      | Store validated transactions using EF Core + SQLite      |
| 📈 **KPI Dashboard**          | Revenue, units sold, average order value and top product |
| 📊 **Revenue Trends**         | Analyse revenue performance over time                    |
| 🏷️ **Category Analysis**     | Compare revenue across product categories                |
| 🏆 **Top Products**           | Identify the five highest-revenue products               |
| 🌍 **Regional Analysis**      | Compare sales performance across regions                 |
| 🎛️ **Interactive Filtering** | Filter analytics by date, category and region            |
| 💡 **Business Insights**      | Automatically generate rule-based observations           |
| 📱 **Responsive UI**          | Dashboard works across desktop and mobile                |
| ⚠️ **Error Handling**         | Friendly validation, empty and error states              |

---

## 🖥️ Dashboard

RetailPulse provides a single analytics-focused dashboard where users can quickly understand overall sales performance.

### Key metrics include:

* **Total Revenue**
* **Units Sold**
* **Average Order Value**
* **Top Product**
* **Revenue Over Time**
* **Revenue by Category**
* **Top 5 Products**
* **Revenue by Region**
* **Automated Business Insights**

### Interactive filtering

Users can filter the dashboard using:

* Date range
* Product category
* Region

The dashboard updates without requiring a full page reload.

---

## 📸 Screenshots

### Dashboard

<img width="1905" height="962" alt="image" src="https://github.com/user-attachments/assets/2b079ff5-f3a2-4bb3-8be9-46c63894a123" />

<img width="1887" height="962" alt="image" src="https://github.com/user-attachments/assets/68e9409b-cdba-46f7-b40d-6faa54ad2207" />


### Data Import

<img width="1882" height="961" alt="image" src="https://github.com/user-attachments/assets/8fe809ce-42d6-4d04-af0e-02444980791f" />


---

## 🏗️ Architecture

RetailPulse follows a simple, focused architecture designed around the application's actual requirements.

```text
                    ┌──────────────────┐
                    │    CSV Dataset   │
                    └────────┬─────────┘
                             │
                             ▼
                 ┌───────────────────────┐
                 │   CSV Import Service  │
                 │                       │
                 │ Parsing & Validation  │
                 │ Data Cleaning         │
                 │ Duplicate Detection   │
                 └───────────┬───────────┘
                             │
                             ▼
                 ┌───────────────────────┐
                 │     SQLite Database   │
                 │     Entity Framework  │
                 └───────────┬───────────┘
                             │
                             ▼
                 ┌───────────────────────┐
                 │    Analytics Service  │
                 │                       │
                 │ KPIs                   │
                 │ Trends                 │
                 │ Categories             │
                 │ Products               │
                 │ Regions                │
                 │ Business Insights      │
                 └───────────┬───────────┘
                             │
                             ▼
                 ┌───────────────────────┐
                 │    Dashboard View     │
                 │                       │
                 │ KPIs • Charts •       │
                 │ Filters • Insights    │
                 └───────────────────────┘
```

The application deliberately avoids unnecessary architectural complexity such as CQRS or a repository layer.

Instead, the project uses:

* A single `DbContext`
* A dedicated CSV import service
* A dedicated analytics service
* MVC controllers for request handling
* ViewModels for presentation-specific data

This keeps the application **simple, maintainable and appropriate for its scope**.

---

## 📁 Project Structure

```text
RetailPulse/
│
├── Controllers/
│   ├── DashboardController.cs
│   └── ImportController.cs
│
├── Data/
│   └── ApplicationDbContext.cs
│
├── Models/
│   ├── Sale.cs
│   ├── SalesFilter.cs
│   └── ViewModels/
│
├── Services/
│   ├── ICsvImportService.cs
│   ├── CsvImportService.cs
│   ├── IAnalyticsService.cs
│   └── AnalyticsService.cs
│
├── Views/
│   ├── Dashboard/
│   ├── Import/
│   └── Shared/
│
├── wwwroot/
│   ├── css/
│   │   └── site.css
│   └── js/
│       └── dashboard.js
│
├── sample-data/
│   └── sample-sales-data.csv
│
└── RetailPulse.Tests/
```

---

## 📊 Analytics

RetailPulse demonstrates multiple types of historical sales analytics.

### Descriptive Analytics

**What happened?**

The dashboard calculates:

* Total revenue
* Units sold
* Average order value
* Number of transactions

### Performance Analysis

**What performed best?**

The application identifies:

* Highest-revenue product
* Best-performing category
* Best-performing region
* Top five products by revenue

### Trend Analysis

**How is performance changing?**

RetailPulse analyses:

* Revenue over time
* Monthly revenue
* Month-over-month growth

### Automated Business Insights

The application converts calculated results into simple, rule-based observations.

For example:

> **Electronics generated the highest revenue across all categories.**

These insights are generated dynamically from the imported dataset rather than being hard-coded.

---

## 🔄 Data Processing Pipeline

RetailPulse demonstrates a basic **ETL-style workflow**:

```text
Extract
  │
  ▼
CSV Sales Data
  │
  ▼
Transform
  │
  ├── Trim values
  ├── Validate required fields
  ├── Validate numeric values
  ├── Validate dates
  └── Detect duplicates
  │
  ▼
Load
  │
  ▼
SQLite Database
  │
  ▼
Analyse
  │
  ├── KPIs
  ├── Trends
  ├── Categories
  ├── Products
  ├── Regions
  └── Business Insights
```

---

## 🧹 Data Validation & Cleaning

Before transactions are stored, `CsvImportService` validates and prepares each row.

The import process:

* Trims whitespace from fields
* Rejects missing required fields
* Validates `TransactionId`
* Validates `Quantity`
* Ensures quantity is greater than zero
* Validates `UnitPrice`
* Validates `Revenue`
* Prevents negative monetary values
* Validates transaction dates
* Detects duplicate transaction IDs

Invalid records are skipped instead of terminating the entire import.

After processing, the user receives a summary showing:

```text
Records imported
Duplicates skipped
Invalid rows skipped
```

This provides a more realistic data-processing workflow than simply importing every row directly into the database.

---

## 🗃️ Sample Dataset

The repository includes a sample dataset:

```text
sample-data/sample-sales-data.csv
```

The dataset contains:

* **220 sales transactions**
* **January – June 2026**
* **14 products**
* **4 categories**
* **5 South African regions**

### Categories

* Electronics
* Accessories
* Furniture
* Office Equipment

### Regions

* Gauteng
* KwaZulu-Natal
* Western Cape
* Eastern Cape
* Free State

The dataset is intentionally structured with variations in monthly sales, product performance and regional activity so that the dashboard produces meaningful analytics rather than identical results.

---

## 🛠️ Technology Stack

### Backend

* **C#**
* **ASP.NET Core MVC**
* **.NET 8**
* **Entity Framework Core**
* **SQLite**

### Data Processing

* **CsvHelper**
* CSV validation
* Data cleaning
* Duplicate detection
* ETL-style processing

### Frontend

* **HTML**
* **CSS**
* **Bootstrap 5**
* **JavaScript**
* **Chart.js**
* **Razor Views**

### Testing

* **xUnit**
* Unit testing of analytics/business logic

---

## 🧪 Testing

RetailPulse includes unit tests focused on the core analytics functionality.

Tests cover calculations such as:

* Total revenue
* Average order value
* Top product
* Revenue by category
* Month-over-month growth

The tests use small, controlled datasets where expected results can be manually verified.

Run the test suite with:

```bash
dotnet test
```

---

## ⚙️ Getting Started

### Prerequisites

Before running RetailPulse, install:

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* Visual Studio 2022 **or** the .NET CLI

---

### 1. Clone the repository

```bash
git clone https://github.com/<your-username>/RetailPulse.git
cd RetailPulse
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Create the database

Using the Package Manager Console:

```powershell
Add-Migration InitialCreate
Update-Database
```

Or using the .NET CLI:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This creates the local:

```text
retailpulse.db
```

SQLite database.

The application also runs `Database.Migrate()` during startup, allowing existing migrations to be applied automatically.

### 4. Run the application

Using Visual Studio:

```text
Press F5
```

Or using the CLI:

```bash
dotnet run --project RetailPulse
```

### 5. Import the sample data

Open the application and navigate to:

```text
Import Data
```

Upload:

```text
sample-data/sample-sales-data.csv
```

### 6. Explore the dashboard

Navigate to:

```text
Dashboard
```

You can now explore the KPIs, charts, filters and automatically generated business insights.

---

## 📈 What This Project Demonstrates

RetailPulse demonstrates practical skills in:

* **Data analytics**
* **Data processing and cleaning**
* **CSV ingestion**
* **Relational database development**
* **Entity Framework Core**
* **ASP.NET Core MVC**
* **Business KPI calculation**
* **Data visualisation**
* **Interactive dashboards**
* **Business insight generation**
* **Unit testing**
* **Responsive web development**
* **Separation of concerns**

The project focuses on the complete journey from **raw transactional data → cleaned data → stored data → analytics → visualisation → business insight**.

---

## 🧠 What I Learned

Through RetailPulse, I gained practical experience in:

* Processing and validating structured CSV data
* Designing a data-import workflow
* Working with SQLite and Entity Framework Core
* Using migrations to manage database changes
* Separating business logic from MVC controllers
* Designing reusable analytics calculations
* Building interactive charts with Chart.js
* Creating dashboards around real business KPIs
* Writing focused unit tests for business logic
* Turning numerical results into understandable business insights

---

## 🔮 Future Improvements

The following features are **not currently implemented** but would be logical next steps:

* 🔐 Authentication and role-based access
* 📄 PDF and Excel report exports
* 📊 Advanced dashboard customisation
* 📈 Predictive sales forecasting
* 🔄 Real-time POS data integration
* 🚨 Advanced sales anomaly detection
* ☁️ Cloud deployment
* 📧 Scheduled analytics reports

---

## 🎯 Project Goal

RetailPulse was created as a portfolio project to demonstrate how **software development and data analytics can be combined into a practical business application**.

Rather than simply displaying raw sales records, the application demonstrates how a developer can take a dataset, process and validate it, store it efficiently, analyse it, and present the results in a way that is useful for decision-making.

---

## 👤 Author

**Diya Lakha**

Bachelor of Computer and Information Sciences
Application Development

**RetailPulse** is a personal portfolio project developed to demonstrate skills in **C#, ASP.NET Core, SQL/data persistence, data analytics, data visualisation, testing, and application development**.

---

## 📄 License

This is a personal portfolio project shared for educational and demonstration purposes.
