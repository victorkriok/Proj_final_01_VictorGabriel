using backend.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

// Registra Database (nova instância por requisição HTTP)
// A MySqlConnection dentro de Database é aberta e fechada em cada método using

builder.Services.AddScoped<AppDbContext>();

// OpenAPI / Swagger — documenta os endpoints automaticamente
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// CORS — Permite que o frontend consuma a API mesmo em portas diferentes
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi(); 

app.UseCors("AllowAll");

// Esse wwwroot/index.html serve para quando for executado o código o localhost leve direto a página do front 
app.UseDefaultFiles();
app.UseStaticFiles();   // serve o arquivo de wwwroot/

app.UseAuthorization();
app.MapControllers();

// Fallback para o frontend em qualquer rota não coberta pela API
app.MapFallbackToFile("index.html");

app.Run();