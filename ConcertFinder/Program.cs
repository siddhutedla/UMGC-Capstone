using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<SeatGeekSettings>(builder.Configuration.GetSection("SeatGeek"));
builder.Services.AddHttpClient("SeatGeekClient", client =>

{
    client.BaseAddress = new Uri("https://api.seatgeek.com/2/");
});

builder.Services.AddSession(options =>
{
    // Session timeout to 20 minutes
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpClient("SeatGeekClient", client =>
{
    client.BaseAddress = new Uri("https://api.seatgeek.com/2/");
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Configuration of SQLite database 
    _ = options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

app.UseSession();

if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/error");
}
app.UseStaticFiles();
app.UseRouting();

#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints(endpoints =>
{
    // Serve home.html - redirects user to login page instead of home - kimball
    _ = endpoints.MapGet("/", async (HttpContext context) =>
    {
        var userId = context.Session.GetString("UserId");
        if (userId == null)
        {
            context.Response.Redirect("/login");
        }
        else
        {
            var username = context.Session.GetString("Username"); // Retrieve the username from session
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "home.html");
            var htmlContent = await File.ReadAllTextAsync(filePath);
            htmlContent = htmlContent.Replace("<span id=\"username\"></span>", $"<span id=\"username\">{username}</span>"); // Inject username into HTML
            await context.Response.WriteAsync(htmlContent, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
    });



    // API call to SeatGeek when searching
    _ = endpoints.MapGet("/search", async (HttpContext context) =>
    {
        var artistName = context.Request.Query["artist"]; // Retrieve the artist name from query
        var seatGeekSettings = context.RequestServices.GetRequiredService<IOptions<SeatGeekSettings>>().Value;

        // Console logging
        Console.WriteLine($"Using Client ID: {seatGeekSettings.ClientId}");
        Console.WriteLine($"Using Client Secret: {seatGeekSettings.ClientSecret}");

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
            Console.WriteLine($"Failed to retrieve events, HTTP Status: {response.StatusCode}");
        }
    });


    _ = endpoints.MapGet("/api/isLoggedIn", (HttpContext context) =>
{
    var isLoggedIn = context.Session.GetString("UserId") != null;
    return Results.Json(new { isLoggedIn });
});


    // Serve login.html when accessing the /login URL
    _ = endpoints.MapGet("/login", (HttpContext context) =>
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "login.html");
        return Results.File(filePath, "text/html");
    });

    // Serve register.html when accessing the /register URL
    _ = endpoints.MapGet("/register", (HttpContext context) =>
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "register.html");
        return Results.File(filePath, "text/html");
    });

    // Register endpoint
    _ = endpoints.MapPost("/register", async (HttpContext context) =>
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
        _ = dbContext.Users.Add(newUser);
        _ = await dbContext.SaveChangesAsync();

        context.Response.Redirect("/login");
    });

    // Login endpoint
    _ = endpoints.MapPost("/login", async (HttpContext context) =>
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

        // Set session data
        context.Session.SetString("UserId", user.Id.ToString());
        context.Session.SetString("Username", user.Username); // Add this line
        Console.WriteLine($"User {user.Username} logged in with ID {user.Id}");
        Console.WriteLine($"Username stored in session: {context.Session.GetString("Username")}");
        context.Response.Redirect("/");
    });

    // Logout endpoint
    _ = endpoints.MapPost("/logout", async (HttpContext context) =>
    {
        context.Session.Clear();
        context.Response.Redirect("/login");
    });

    // change password endpoint
    endpoints.MapPost("/change-password", async context =>
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
        Console.WriteLine($"Current: {currentPassword}, New: {newPassword}, Confirm: {confirmPassword}");
        var dbContext = context.RequestServices.GetRequiredService<AppDbContext>();
        var user = await dbContext.Users.FindAsync(int.Parse(userId));

        if (user == null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("User not found.");
            return;
        }

        if (user.Password != currentPassword)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Current password is incorrect.");
            return;
        }

        if (newPassword != confirmPassword)
        {
            Console.WriteLine($"Comparing new: {newPassword} to confirm: {confirmPassword}");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("New passwords do not match.");
            return;
        }
        user.Password = newPassword;
        await dbContext.SaveChangesAsync();
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsync("Password updated successfully!");
    });

    // Account settings endpoint
    _ = app.MapGet("/account-settings", async context =>
    {
        if (context.Session.GetString("UserId") == null)
        {
            context.Response.Redirect("/login");
            return;
        }

        // If the user is logged in, serve the account-settings.html page
        var filePath = Path.Combine(app.Environment.ContentRootPath, "account-settings.html");
        if (File.Exists(filePath))
        {
            await context.Response.SendFileAsync(filePath);
        }
        else
        {
            // File not found, send 404
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Account settings page not found.");
        }
    });


});
#pragma warning restore ASP0014 // Suggest using top level route registrations

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

