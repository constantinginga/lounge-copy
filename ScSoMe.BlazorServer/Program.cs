using Blazor.Analytics;
using MudBlazor.Services;
using ScSoMe.RazorLibrary;
using ScSoMe.Common;
using Microsoft.EntityFrameworkCore;
using Blazored.LocalStorage;
using Ljbc1994.Blazor.IntersectionObserver;
using Microsoft.AspNetCore.Rewrite;
using ScSoMe.RazorLibrary.Pages.Helpers;
using Microsoft.Extensions.FileProviders;

using Microsoft.AspNetCore.ResponseCompression;
using ScSoMe.BlazorServer.DirectCommunication.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Settings to see error details in console
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

//builder.Services.AddResponseCompression(opts =>
//{
//    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
//        new[] { "application/octet-stream" });
//});


//builder.Services.AddSingleton<AppState>();
builder.Services.AddScoped<AppState>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<ScSoMe.Common.ApiClientFactory>();

builder.Services.AddMudServices();

//Middleware service
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

//builder.Services.AddBlazoredToast();

builder.Services.AddGoogleAnalytics("UA-117081036-1");

builder.Services.AddIntersectionObserver();

builder.Services.AddHttpClient();

builder.Services.AddScoped<IEventService, EventService>();

var app = builder.Build();
app.UsePathBase("/lounge/");

app.Map("/lounge", subapp =>
{
    subapp.UsePathBase("/lounge");
    subapp.UseRouting();
    subapp.UseEndpoints(endpoints => endpoints.MapBlazorHub());
});

app.UseStaticFiles(); // <-- don't forget this
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @".well-known")),
    RequestPath = new PathString("/.well-known"),
    ServeUnknownFileTypes = true
});

var rewriteOptions = new RewriteOptions();
rewriteOptions.AddRewrite("_blazor/initializers", "/_blazor/initializers", skipRemainingRules: true);
app.UseRewriter(rewriteOptions);


//app.UseResponseCompression();
app.UseResponseCompression();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<ChatHub>("/chathub");
app.MapHub<NotificationsHub>("/notificationshub");

app.MapFallbackToPage("/_Host");

app.Run();

// Configure the HTTP request pipeline.
/*if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
//app.MapHub<AlertHub>("/alerthub");
app.MapFallbackToPage("/_Host");



app.Run();
*/