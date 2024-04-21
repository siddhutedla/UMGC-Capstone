using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Configure services here
builder.Services.Configure<SeatGeekSettings>(builder.Configuration.GetSection("SeatGeek"));
builder.Services.AddHttpClient("SeatGeekClient", client =>
{
    client.BaseAddress = new Uri("https://api.seatgeek.com/2/");
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure middlewares here
if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/error");
}


app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    // Map "/" to home.html
    _ = endpoints.MapGet("/", async context =>
    {
        var userId = context.Session.GetString("UserId");
        if (userId == null)
        {
            context.Response.Redirect("/login");
        }
        else
        {
            var username = context.Session.GetString("Username");
            var filePath = Path.Combine(app.Environment.WebRootPath, "view", "home.html");
            var htmlContent = await File.ReadAllTextAsync(filePath);
            htmlContent = htmlContent.Replace("[default text or empty]", username);
            await context.Response.WriteAsync(htmlContent, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
    });

    // Serve login.html
    _ = endpoints.MapGet("/login", async context =>
    {
        var filePath = Path.Combine(app.Environment.WebRootPath, "view", "login.html");
        await context.Response.SendFileAsync(filePath);
    });

    // Serve register.html
    _ = endpoints.MapGet("/register", async context =>
    {
        var filePath = Path.Combine(app.Environment.WebRootPath, "view", "register.html");
        await context.Response.SendFileAsync(filePath);
    });

    // Serve account-settings.html
    _ = endpoints.MapGet("/account-settings", async context =>
    {
        if (context.Session.GetString("UserId") == null)
        {
            context.Response.Redirect("/login");
            return;
        }
        var filePath = Path.Combine(app.Environment.WebRootPath, "view", "account-settings.html");
        await context.Response.SendFileAsync(filePath);
    });

    // API call to SeatGeek when searching
    _ = endpoints.MapGet("/search", async context =>
    {
        var artistName = context.Request.Query["artist"];

        // Check if artistName is null or empty
        if (string.IsNullOrEmpty(artistName))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Artist name is required.");
            return;
        }

        var seatGeekSettings = context.RequestServices.GetRequiredService<IOptions<SeatGeekSettings>>().Value;
        var client = context.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient("SeatGeekClient");
        var response = await client.GetAsync($"events?q={Uri.EscapeDataString(artistName)}&client_id={seatGeekSettings.ClientId}&client_secret={seatGeekSettings.ClientSecret}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        }
        else
        {
            context.Response.StatusCode = (int)response.StatusCode;
            await context.Response.WriteAsync("Failed to retrieve events");
        }
    });


    // Check login status API
    _ = endpoints.MapGet("/api/isLoggedIn", context =>
    {
        var isLoggedIn = context.Session.GetString("UserId") != null;
        return (Task)Results.Json(new { isLoggedIn });
    });

    // Register endpoint
    _ = endpoints.MapPost("/register", async context =>
    {
        var form = await context.Request.ReadFormAsync();
        var username = form["username"].FirstOrDefault();
        var password = form["password"].FirstOrDefault();
        var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();

        if (await dbContext.Users.AnyAsync(u => u.Username == username))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Username already exists");
            return;
        }

        var newUser = new User { Username = username, Password = password };
        _ = dbContext.Users.Add(newUser);
        _ = await dbContext.SaveChangesAsync();
        context.Response.Redirect("/login");
    });

    // Login endpoint
    _ = endpoints.MapPost("/login", async context =>
    {
        var form = await context.Request.ReadFormAsync();
        var username = form["username"].FirstOrDefault();
        var password = form["password"].FirstOrDefault();
        var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
        if (user == null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid username or password");
            return;
        }

        context.Session.SetString("UserId", user.Id.ToString());
        context.Session.SetString("Username", user.Username);
        context.Response.Redirect("/");
    });

    // Logout endpoint
    _ = endpoints.MapPost("/logout", context =>
    {
        context.Session.Clear();
        context.Response.Redirect("/login");
        return Task.CompletedTask;
    });

    // Change password endpoint
    _ = endpoints.MapPost("/change-password", async context =>
    {
        var userId = context.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("User not logged in.");
            return;
        }

        var form = await context.Request.ReadFormAsync();
        var currentPassword = form["currentPassword"].FirstOrDefault()?.Trim();
        var newPassword = form["newPassword"].FirstOrDefault()?.Trim();
        var confirmPassword = form["confirmPassword"].FirstOrDefault()?.Trim();
        var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.FindAsync(int.Parse(userId));

        if (user == null || user.Password != currentPassword)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Current password is incorrect or user not found.");
            return;
        }

        if (newPassword != confirmPassword)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("New passwords do not match.");
            return;
        }

        user.Password = newPassword;
        _ = await dbContext.SaveChangesAsync();
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync("Password updated successfully!");
    });

    // ... add other endpoints as needed ...
});

app.Run();

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty; // Initialize as empty string
    public string Password { get; set; } = string.Empty; // Initialize as empty string
}

public class SeatGeekSettings
{
    public string ClientId { get; set; } = string.Empty; // Initialize as empty string
    public string ClientSecret { get; set; } = string.Empty; // Initialize as empty string
}

