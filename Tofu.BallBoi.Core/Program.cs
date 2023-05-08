using Tofu.BallBoi.Core.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.AddConsole();
builder.Logging.AddDebug();

var apiKey = Environment.GetEnvironmentVariable("APIKEY");
builder.Services.AddOpenAi(settings =>
{
    settings.ApiKey = apiKey;
});
builder.Services.AddScoped<slashCommandService>();
builder.Services.AddScoped<ChatService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
