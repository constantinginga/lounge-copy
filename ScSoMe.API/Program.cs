using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using ScSoMe.API.Controllers.AccountRegistration.AccountRegistrationController;
using ScSoMe.API.Services;
using ScSoMe.EF;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AccountRegistrationController>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173/", "https://profile-page-orpin.vercel.app/","https://localhost:7279", "https://0.0.0.0", "app://0.0.0.0")
            .AllowAnyOrigin().AllowAnyHeader();
        });
});



//https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-6.0#configure-session-state
//builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromSeconds(910);
//    options.Cookie.HttpOnly = false;
//    options.IOTimeout = TimeSpan.FromSeconds(9);
//    options.Cookie.Name = ".ScSoMe.API.Session";
//    options.Cookie.IsEssential = true;
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(MyAllowSpecificOrigins);

    //app.UseCors(policy => policy
    //.WithOrigins("https://localhost:7019")
    //.AllowAnyMethod()
    //.AllowAnyHeader()
    //.AllowCredentials()
    //// https://code-maze.com/aspnetcore-add-custom-headers/
    //.WithExposedHeaders("X-ScSoMe-Token"));
}

app.UseStaticFiles(); // <-- don't forget this
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @".well-known")),
    RequestPath = new PathString("/.well-known"),
    ServeUnknownFileTypes = true
});

if (app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(policy => policy
    .WithOrigins("https://profile-page-orpin.vercel.app/", "https://www.startupcentral.dk", "https://startupcentral.dk", "https://groups.startupcentral.dk", "https://testgroups.startupcentral.dk", "https://test.startupcentral.dk", "https://localhost:7279", "https://0.0.0.0", "app://0.0.0.0") // TODO: Set to production deployment endpoint !!! // https://youtu.be/5SLGQDDp0aI?t=1716
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    //.AllowAnyOrigin()
    // https://code-maze.com/aspnetcore-add-custom-headers/
    .WithExposedHeaders("X-ScSoMe-Token"));
}

app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();
//app.UseSession();

app.MapControllers();

bool isProd = Environment.MachineName.Equals("startupVM");
if (isProd)
    ScSoMe.API.Services.EmailNotificationsService.StartPeriodicTimerAsync();

var runTask = app.RunAsync();
await runTask;
