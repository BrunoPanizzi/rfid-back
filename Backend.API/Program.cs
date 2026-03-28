using Microsoft.EntityFrameworkCore;
using DotNetEnv;

using Backend.Features.Users;
using Backend.Database;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

const string DevCorsPolicyName = "DevCors";

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(DevCorsPolicyName, policy =>
        {
            policy
                .WithOrigins("*") // in dev mode we allow everything
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}

var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

// Register feature services
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Apply pending migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(DevCorsPolicyName);
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
