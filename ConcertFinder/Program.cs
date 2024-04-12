using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Linq;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Add database context
var dbContext = new AppDbContext();
dbContext.Database.EnsureCreated(); // Create the database if it doesn't exist

// Serve home.html when accessing the root URL
app.MapGet("/", (HttpContext context) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "home.html");
    return Results.File(filePath, "text/html");
});

// Serve login.html when accessing the /login URL
app.MapGet("/login", (HttpContext context) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "login.html");
    return Results.File(filePath, "text/html");
});

// Serve register.html when accessing the /register URL
app.MapGet("/register", (HttpContext context) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "register.html");
    return Results.File(filePath, "text/html");
});

// Register endpoint
app.MapPost("/register", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["username"].FirstOrDefault();
    var password = form["password"].FirstOrDefault();

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
app.MapPost("/login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["username"].FirstOrDefault();
    var password = form["password"].FirstOrDefault();

    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
    if (user == null)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid username or password");
        return;
    }

    context.Response.Redirect("/");
});


app.Run();
