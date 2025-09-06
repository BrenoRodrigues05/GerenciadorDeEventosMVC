using EventosMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuração dos HttpClients para APIs externas
builder.Services.AddHttpClient("EventosAPI", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ServiceUri:EventosAPI"]);
});

builder.Services.AddHttpClient("AuthApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUri:AuthAPI"]);
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Serviços customizados
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAutenticação, AutenticacaoService>();
builder.Services.AddScoped<IEventosService, EventosService>();
builder.Services.AddScoped<IInscricaoService, InscricaoService>();

// Configuração de autenticação via Cookie
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = "EventosMVCCookie";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // IMPORTANTE
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
