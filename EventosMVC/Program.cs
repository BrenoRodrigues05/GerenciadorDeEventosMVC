using EventosMVC.Services;
using Microsoft.Extensions.Configuration;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient("EventosAPI", c=>
{
 c.BaseAddress = new Uri(builder.Configuration["ServiceUri:EventosAPI"]);
});

builder.Services.AddHttpClient("AuthApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUri:AuthAPI"]);
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAutenticação,AutenticacaoService>();
builder.Services.AddScoped<IEventosService, EventosService>();
builder.Services.AddScoped<IInscricaoService, InscricaoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
