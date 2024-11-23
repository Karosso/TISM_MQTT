using Firebase.Database;
using Microsoft.Extensions.Options;
using TISM_MQTT.Firebase;
using TISM_MQTT.Services;

var builder = WebApplication.CreateBuilder(args);

// Adicione serviços ao contêiner
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar o HttpClient
builder.Services.AddHttpClient();

// Registrar o MqttClientService como singleton e HostedService
builder.Services.AddSingleton<MqttClientService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<MqttClientService>());

builder.Services.Configure<FirebaseSettings>(builder.Configuration.GetSection("Firebase"));
builder.Services.AddSingleton<FirebaseClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<FirebaseSettings>>().Value;
    return new FirebaseClient(settings.DatabaseUrl, new FirebaseOptions
    {
        AuthTokenAsyncFactory = () => Task.FromResult(settings.AuthSecret)
    });
});

var app = builder.Build();

// Configure o pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
