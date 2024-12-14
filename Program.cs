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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar o Kestrel para escutar na porta 80
builder.WebHost.ConfigureKestrel(options =>
{
    // options.ListenAnyIP(80); // Escuta na porta 80 comentar para funcionar local
});

var app = builder.Build();

app.UseCors("AllowAll");

// Configure o pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); //Remover antes de publicar comentar para funcionar local
app.UseAuthorization();
app.MapControllers();
app.Run();
