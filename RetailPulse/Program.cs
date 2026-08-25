using Microsoft.EntityFrameworkCore;
using RetailPulse.Data;
using RetailPulse.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC with views - this is a server-rendered dashboard, no SPA framework needed.
builder.Services.AddControllersWithViews();

// SQLite via EF Core. The connection string points at a single file database
// stored in the project folder - see appsettings.json.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services are registered as scoped (one instance per request), which matches
// the lifetime of the DbContext they depend on.
builder.Services.AddScoped<ICsvImportService, CsvImportService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

var app = builder.Build();

// Apply any pending EF Core migrations automatically on startup, so the
// database is always up to date without a manual step when running locally.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
