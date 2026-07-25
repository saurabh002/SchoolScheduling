// Program.cs
using Microsoft.EntityFrameworkCore;
using SchoolScheduling;
using SchoolScheduling.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Connect the app to a local, relative SQLite file drawer
builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlite("Data Source=school.db"));

builder.Services.AddControllers();

// Enable local Cross-Origin Resource Sharing for modern Angular frontend requests
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Automatically scaffold and verify the .db file on application spin up
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
    db.Database.EnsureCreated();

    // Repair previously-seeded class-section name mismatch in existing DBs.
    db.Database.ExecuteSqlRaw("UPDATE ClassSections SET Name = '4F' WHERE Id = 23 AND Name = '5F';");
    db.Database.ExecuteSqlRaw("UPDATE ClassSections SET Name = '5F' WHERE Id = 29;");
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapTeacherEndpoints();
app.MapTimeTableEndpoints();
app.MapAbsenceEndpoints();
app.MapReportEndpoints();
app.MapReportExportEndpoint();

app.Run();