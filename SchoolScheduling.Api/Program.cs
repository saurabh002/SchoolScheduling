using Microsoft.EntityFrameworkCore;
using SchoolScheduling;
using SchoolScheduling.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// 1. Resolve persistent directory path BEFORE configuring services
// In Azure App Service (Linux or Windows), D:\home or /home is persistent.
var homeDir = Environment.GetEnvironmentVariable("HOME") ?? AppContext.BaseDirectory;
var dataFolder = Path.Combine(homeDir, "data");
Directory.CreateDirectory(dataFolder);

var dbPath = Path.Combine(dataFolder, "school.db");

// 2. Register DbContext ONCE with the correct path
builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddControllers();

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 1. Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowStaticWebApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build(); // <--- Build must happen AFTER registering services!

// 3. Database initialization
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors("AllowStaticWebApp");
app.UseAuthorization();
app.MapControllers();
app.MapTeacherEndpoints();
app.MapTimeTableEndpoints();
app.MapAbsenceEndpoints();
app.MapReportEndpoints();
app.MapReportExportEndpoint();

app.Run();