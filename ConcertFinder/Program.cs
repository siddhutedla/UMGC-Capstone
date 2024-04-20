using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession(options =>
{
    // Set session timeout to 20 minutes
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddHttpClient("SeatGeekClient", client =>
{
    client.BaseAddress = new Uri("https://api.seatgeek.com/2/");
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Configure your SQLite database connection here
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    // Serve home.html when accessing the root URL
    endpoints.MapGet("/", async (HttpContext context) =>
    {
        var userId = context.Session.GetString("UserId");
        var username = context.Session.GetString("Username"); // Retrieve the username from session
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "home.html");
        var htmlContent = await File.ReadAllTextAsync(filePath);
        htmlContent = htmlContent.Replace("<span id=\"username\"></span>", $"<span id=\"username\">{username}</span>"); // Inject username into HTML
        await context.Response.WriteAsync(htmlContent, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    });

    // Add endpoint for artist search
    endpoints.MapGet("/search", async (HttpContext context) =>
    {
        var artist = context.Request.Query["artist"].ToString();
        var client = context.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient("SeatGeekClient");
        var seatGeekSettings = context.RequestServices.GetRequiredService<IOptions<SeatGeekSettings>>().Value;
        var response = await client.GetAsync($"events?q={Uri.EscapeDataString(artist)}&client_id={seatGeekSettings.ClientId}");

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

    endpoints.MapGet("/api/isLoggedIn", (HttpContext context) =>
{
    var isLoggedIn = context.Session.GetString("UserId") != null;
    return Results.Json(new { isLoggedIn });
    });


    // Serve login.html when accessing the /login URL
    endpoints.MapGet("/login", (HttpContext context) =>
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "login.html");
        return Results.File(filePath, "text/html");
    });

    // Serve register.html when accessing the /register URL
    endpoints.MapGet("/register", (HttpContext context) =>
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "register.html");
        return Results.File(filePath, "text/html");
    });

    // Register endpoint
    endpoints.MapPost("/register", async (HttpContext context) =>
    {
        var form = await context.Request.ReadFormAsync();
        var username = form["username"].FirstOrDefault();
        var password = form["password"].FirstOrDefault();

        var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();

        // Check if the username already exists in the database
        if (await dbContext.Users.AnyAsync(u => u.Username == username))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Username already exists");
            return;
        }

        // Save to database
        var newUser = new User { Username = username, Password = password };
        dbContext.Users.Add(newUser);
        await dbContext.SaveChangesAsync();

        context.Response.Redirect("/login");
    });

// Login endpoint
endpoints.MapPost("/login", async (HttpContext context) =>
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

    // Set session data including username
    context.Session.SetString("UserId", user.Id.ToString());
    context.Session.SetString("Username", user.Username); // Add this line
    Console.WriteLine($"User {user.Username} logged in with ID {user.Id}");
    Console.WriteLine($"Username stored in session: {context.Session.GetString("Username")}");
    context.Response.Redirect("/");
});


    // Logout endpoint
    endpoints.MapPost("/logout", async (HttpContext context) =>
    {
        // Clear session data
        context.Session.Clear();

        context.Response.Redirect("/login");
    });
});

app.Use(async (context, next) =>
{
    if (context.Request.Path != "/login" && !context.Session.Keys.Contains("UserId"))
    {
        // Redirect to login if user is not authenticated
        context.Response.Redirect("/login");
        return;
    }

    await next();
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
    public string Username { get; set; }
    public string Password { get; set; }
}

public class SeatGeekSettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; } // If needed
}