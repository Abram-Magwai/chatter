using System.Text;
using chatter.core.entities;
using chatter.core.interfaces;
using chatter.core.services;
using chatter.core.settings;
using chatter.infrastructure.repositories;
using chatter.view.models;
using chatter.view.services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;

services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
var appSettings = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
services.Configure<DbSettings>(builder.Configuration.GetSection(nameof(DbSettings)));
var dbSettings = builder.Configuration.GetSection(nameof(DbSettings)).Get<DbSettings>();

    services.AddScoped<IUserService, UserService>();
services.AddScoped<IProfileService, ProfileService>();
services.AddScoped<IMessageService, MessageService>();
services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

services.AddIdentity<ApplicationUser, ApplicationRole>()
.AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
(
    dbSettings!.ConnectionString, dbSettings.DatabaseName
);


// services.AddAuthentication(options => {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
// }).AddJwtBearer(o => {
//     o.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidIssuer = appSettings.Issuer,
//         ValidAudience = appSettings.Audience,
//         IssuerSigningKey = new SymmetricSecurityKey
//         (Encoding.UTF8.GetBytes(appSettings.SecretKey)),
//         ValidateIssuer = true,
//         ValidateAudience = false,
//         ValidateLifetime = false,
//         ValidateIssuerSigningKey = true
//     };
// });
services.AddSignalR();
services.AddScoped<ISignalRHub, SignalRHub>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.Cookie.Name = "chat";
    options.LoginPath = "/account/login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    options.SlidingExpiration = true;
    options.LogoutPath = "/account/login";
    options.SlidingExpiration = true;
});
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.MapHub<SignalRHub>("/chatHub");

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();
