using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ConcertFinder.Data;
using ConcertFinder.Configuration;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // This will add support for controllers.

// Configure SeatGeek settings and HTTP client.
builder.Services.Configure<SeatGeekSettings>(builder.Configuration.GetSection("SeatGeek"));
builder.Services.AddHttpClient("SeatGeekClient", client =>
{
    client.BaseAddress = new Uri("https://api.seatgeek.com/2/");
});

// Configure session state with a distributed memory cache.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});
builder.Services.AddDistributedMemoryCache();

// Add DbContext using SQLite.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

// Get Username


app.UseStaticFiles(); // For serving static files in wwwroot.
app.UseSession(); // For managing user sessions.
app.UseRouting(); // For URL routing.

// Map controllers using attribute routing.
app.MapControllers();

app.Run();


