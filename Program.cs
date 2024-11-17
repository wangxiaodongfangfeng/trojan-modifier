using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using trojan_modifier;

var builder = WebApplication.CreateBuilder(args);
var corsPolicy = "AllowWestWorldRequest";
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddEnvironmentVariables();
var origin = builder.Configuration["AllowedWestWordOrigin"] ?? "http://west90.com/";
var trojanPath = builder.Configuration["TrojanPath"] ?? "/app/trojan";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy,
        policy => { policy.WithOrigins(origin.Split(',')).AllowAnyMethod().AllowAnyHeader().AllowCredentials(); });
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
Console.WriteLine($"CrosSetting is got which is {origin}");
var app = builder.Build();
app.UseCors(corsPolicy);
// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
app.Use(async (context, next) =>
{
    Console.WriteLine("Incoming Request:");
    Console.WriteLine($"Method: {context.Request.Method}");
    Console.WriteLine($"Path: {context.Request.Path}");
    Console.WriteLine($"Origin: {context.Request.Headers["Origin"]}");
    Console.WriteLine($"Access-Control-Request-Method: {context.Request.Headers["Access-Control-Request-Method"]}");
    Console.WriteLine(
        $"Access-Control-Request-Headers: {context.Request.Headers["Access-Control-Request-Headers"]}");
    await next.Invoke();
});
//}
app.UseHttpsRedirection();

var trojanManager = new TrojanManager("./trojan/trojan", "./trojan/config.json");
var modifier = new ConfigModifier(trojanManager, Path.Combine(trojanPath, "config.json"));
app.MapGet("/config", () =>
    {
        var json = modifier.ReadConfig();
        return json;
    })
    .WithName("GetTrojanConfig").WithOpenApi();

app.MapPost("/config", (ConfigItem item) =>
    {
        if (item is not { Ip: not null, Password: not null, Port: not null })
            return Results.BadRequest("should give enough information");
        modifier.Modify(item.Ip, item.Port, item.Password);
        return Results.Ok(true);
    })
    .WithName("ModifyTrojanConfig")
    .WithOpenApi().RequireCors(corsPolicy);

app.MapPost("/config-pattern", (string body) =>
    {
        var regex = new Regex(@"\/\/([\w|\d]*)@(\d{1,3}\.\d{1,3}.\d{1,3}\.\d{1,3}):(\d{5})\?");
        var match = regex.Match(body);
        if (!match.Success) return false;
        var password = match.Groups[1].Value;
        var ip = match.Groups[2].Value;
        var port = match.Groups[3].Value;
        modifier.Modify(ip, port, password);
        return true;
    })
    .WithName("ModifyConfigWithString")
    .WithOpenApi();

trojanManager.StartTrojanAsync();

app.Run();

internal abstract record ConfigItem(string? Ip, string? Port, string? Password)
{
    public override string ToString()
    {
        return @$"{Password}@{Ip}:{Port}";
    }
}