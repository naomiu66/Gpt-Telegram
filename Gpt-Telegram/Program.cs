using DotNetEnv;
using Gpt_Telegram.Consumers;
using Gpt_Telegram.Consumers.Handlers.Abstractions;
using Gpt_Telegram.Consumers.Handlers.Implementations;
using Gpt_Telegram.Data;
using Gpt_Telegram.Data.Connections;
using Gpt_Telegram.Data.Repositories.Abstractions;
using Gpt_Telegram.Data.Repositories.Implementations;
using Gpt_Telegram.Producer;
using Gpt_Telegram.Services.Abstractions;
using Gpt_Telegram.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using Telegram.Bot;
using StackExchange.Redis;
using Gpt_Telegram.Data.Redis.Repositories;
using Gpt_Telegram.Pipelines;
using Gpt_Telegram.Pipelines.SessionCreation;
using Gpt_Telegram.Utilities.Telegram;
using Gpt_Telegram.Utilities.OpenAI;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration
    .AddEnvironmentVariables();

builder.Services.AddDbContext<ApplicationContext>(
    options =>
    {
        options.UseNpgsql(builder.Configuration["DEFAULT_CONNECTION"]);
    });

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return ConnectionMultiplexer.Connect(config["Redis"]);
});

builder.Services.AddSingleton<ChatClient>(serviceProvider =>
{
    var config = serviceProvider.GetService<IConfiguration>();
    var apiKey = config["OpenAI:ApiKey"];
    var model = config["OpenAI:Model"];

    return new(model, apiKey);
});

// Pipelines

//SessionCreation Pipeline
builder.Services.AddScoped<StepHandler, SetTitleStepHandler>();
builder.Services.AddScoped<StepHandler, SetSystemPromptStepHandler>();

builder.Services.AddScoped<PipelineRouter>();

// Repositories and Services

builder.Services.AddScoped<IUserStateRepository, UserStateRepository>();

builder.Services.AddScoped<IChatSessionsRepository, ChatSessionsRepository>();
builder.Services.AddScoped<IChatSessionsService, ChatSessionsService>();

builder.Services.AddScoped<IChatMessagesRepository, ChatMessagesRepository>();
builder.Services.AddScoped<IChatMessagesService, ChatMessagesService>();

builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUsersService, UsersService>();

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

// register consumers and handlers
builder.Services.AddSingleton<IUpdateProducer, UpdateProducer>();

builder.Services.AddHostedService<PromptConsumer>();
builder.Services.AddHostedService<CommandConsumer>();
builder.Services.AddHostedService<CallbackConsumer>();

builder.Services.AddScoped<ICommandHandler, CommandHandler>();
builder.Services.AddScoped<IPromptHandler, PromptHandler>();
builder.Services.AddScoped<ICallbackHandler, CallbackHandler>();

// Register Utilities

builder.Services.AddScoped<KeyboardMarkupBuilder>();
builder.Services.AddScoped<ITokenOptimizer, TokenOptimizer>();

builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var token = config["Telegram:Token"];

    return new TelegramBotClient(token);
});

builder.WebHost.UseUrls("http://0.0.0.0:80");

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gpt Telegram API V1");
    });
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var bot = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var webhookUrl = config["Telegram:WebhookUrl"];

    await bot.SetWebhook(webhookUrl);

    var info = await bot.GetWebhookInfo();

    Console.WriteLine($"Webhook status: {info.Url} - last error: {info.LastErrorMessage}");
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    await db.Database.MigrateAsync();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


await app.RunAsync();
