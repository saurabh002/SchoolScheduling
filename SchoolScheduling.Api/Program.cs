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
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapTeacherEndpoints();
app.MapTimeTableEndpoints();
app.MapAbsenceEndpoints();
app.Run();