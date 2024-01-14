using Azure.Storage.Blobs;
using chatter.core.entities;
using chatter.core.interfaces;
using chatter.core.services;
using chatter.core.settings;
using chatter.infrastructure.repositories;
using chatter.view.mappingProfiles;
using chatter.view.models;
using chatter.view.services;
using Microsoft.AspNetCore.Authentication.Cookies;

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

//Azure
services.AddSingleton(new BlobServiceClient(builder.Configuration.GetSection("AzureSettings").GetValue<string>("AzureBlobConnectionString")));
services.AddSingleton<IBlobService, BlobService>();
services.Configure<AzureSettings>(builder.Configuration.GetSection("AzureSettings"));

services.AddIdentity<ApplicationUser, ApplicationRole>()
.AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
(
    dbSettings!.ConnectionString, dbSettings.DatabaseName
);

services.AddSignalR();
services.AddScoped<ISignalRHub, SignalRHub>();
services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.Cookie.Name = "chat";
    options.LoginPath = "/account/login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
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
