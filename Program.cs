using backend.Data;

// Cria o builder da aplicação — é ele que configura todos os serviços antes de rodar
var builder = WebApplication.CreateBuilder(args);

// Registra os controllers no sistema de injeção de dependência
// Sem isso, as rotas definidas nos controllers (ProdutosController, etc.) não funcionam

builder.Services.AddControllers();

// Registra o AppDbContext com ciclo de vida Scoped
// Scoped = uma nova instância é criada por requisição HTTP e descartada ao final dela
// Isso garante que cada requisição tenha sua própria conexão com o banco
builder.Services.AddScoped<AppDbContext>();

// Registra o explorador de endpoints — necessário para o Swagger descobrir as rotas automaticamente
builder.Services.AddEndpointsApiExplorer();

// Registra o OpenAPI/Swagger — gera a documentação interativa da API
// Acessível em /openapi no ambiente de desenvolvimento
builder.Services.AddOpenApi();

// Configura o CORS (Cross-Origin Resource Sharing)
// Sem isso, o navegador bloqueia chamadas do frontend (ex: index.html na porta 5500)
// para a API (ex: localhost:5260) por serem origens diferentes
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin() // Permite requisições de qualquer domínio/porta
              .AllowAnyMethod()  // Permite qualquer método HTTP (GET, POST, PUT, DELETE...)
              .AllowAnyHeader()); // Permite qualquer cabeçalho (Authorization, Content-Type...)
});

// Constrói a aplicação com todas as configurações registradas acima
var app = builder.Build();

// Ativa o Swagger apenas em ambiente de desenvolvimento
// Em produção ele fica desativado por segurança
if (app.Environment.IsDevelopment())
    app.MapOpenApi(); 

// Aplica a política de CORS configurada acima em todas as requisições
// IMPORTANTE: deve vir antes de UseAuthorization e MapControllers
app.UseCors("AllowAll");

// Procura automaticamente por um arquivo padrão na pasta wwwroot
// Por padrão serve: index.html, index.htm, default.html, default.htm
app.UseDefaultFiles();

// Habilita o servidor de arquivos estáticos — serve os arquivos da pasta wwwroot/
// É assim que o index.html, CSS, JS e imagens ficam acessíveis pelo navegador
app.UseStaticFiles(); 

// Habilita o middleware de autorização
// Necessário para que atributos como [Authorize] funcionem nos controllers
app.UseAuthorization();

// Mapeia todas as rotas dos controllers registrados
// É aqui que o ASP.NET conecta as URLs (ex: GET /api/produtos) aos métodos dos controllers
app.MapControllers();

// Fallback — qualquer rota que não seja da API e não exista em wwwroot
// redireciona para o index.html (útil para SPAs com roteamento no frontend)
app.MapFallbackToFile("index.html");

// Inicia o servidor e fica escutando as requisições
app.Run();
