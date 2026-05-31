using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Classes;
using vue_spotify_app.Server;
using vue_spotify_app.Server.Data;
using vue_spotify_app.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextPool<DataContext>(options =>
{
    var databaseContext = builder.Configuration["ConnectionStrings:DatabaseContext"];
    options.UseSqlServer(databaseContext ?? throw new InvalidOperationException("Connection string 'DatabaseContext' not found."));
    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine, LogLevel.Information);

}, poolSize: 32);

builder.Services.AddHttpClient<SpotifyAPIWrapper>(options =>
{
    options.BaseAddress = new Uri("https://api.spotify.com/v1/");
});

builder.Services.AddScoped<TrackService>();
builder.Services.AddScoped<PlaybackRecordService>();
builder.Services.AddScoped<PlaylistService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SpotifyAPIWrapper>();
builder.Services.AddScoped<PlaybackQueueService>();
builder.Services.AddScoped<TrackAliasService>();

builder.Services.AddHostedService<SyncPlaybackRecordService>();
builder.Services.AddHostedService<SyncLikedSongsLibraryService>();
builder.Services.AddHostedService<SyncTracksWithPlaybackHistoryService>();
builder.Services.AddHostedService<SyncPlaylistService>();
builder.Services.AddHostedService<SyncTrackAliasService>();
builder.Services.AddHostedService<SyncTracksService>();

builder.Services.Configure<SpotifyOptions>(builder.Configuration.GetSection("Spotify"));
builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAuthentication().AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Logging.ClearProviders();
builder.Logging.AddDebug();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
