using CVUSV.Components;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization();
builder.Services.AddControllers();
builder.Services.AddCsp(nonceByteAmount: 32);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(60);
    options.ExcludedHosts.Add("ulisessandria.com");
    options.ExcludedHosts.Add("www.ulisessandria.com");
});
var app = builder.Build();

string[] supportedCultures = ["en-US", "es-MX"];
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
 
app.Use(async (context, next) =>
{
     context.Response.Headers.Add("X-Content-Type-Options", new StringValues("nosniff"));
    context.Response.Headers.Add("X-Frame-Options", new StringValues("SAMEORIGIN"));
    context.Response.Headers.Add("X-XSS-Protection", new StringValues("1; mode=block"));
    context.Response.Headers.Add("Strict-Transport-Security", new StringValues("max-age=31536000; includeSubDomains"));
    context.Response.Headers.Add("Content-Security-Policy", new StringValues("script-src 'self'"));
    context.Response.Headers.Add("Permissions-Policy", new StringValues("geolocation=(self 'https://www.ulisessandria.com'), microphone=()"));
    await next();
});
app.UseReferrerPolicy(new ReferrerPolicyOptions() { PolicyValue = ReferrerPolicyOptions.ReferrerPolicyValue.NoReferrer });

app.Run();
