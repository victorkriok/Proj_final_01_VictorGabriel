using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Model;

namespace backend.Controller
{
    [ApiController] // Define que a classe é um controller de uma API REST
    [Route("api/[controller]")] // Define a rota base api/produtos ([controller]) sem o nome da classe sem o "Controller"
    public class ProdutosController : ControllerBase // Herda os atributos do Controller Base que dá acesso a Ok() , badrequest, etc
    {
        private readonly AppDbContext _db; // Essa é uma string que representa meu AppDbContext para utilizar as funções da classe

         // Construtor — o ASP.NET injeta automaticamente o AppDbContext via injeção de dependência
        public ProdutosController(AppDbContext db) => _db = db;

        // Get/api/produtos
        // Aqui ele busca pelo nome se o parâmetro for dado
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? nome)
        {
            try
            {
                var lista = string.IsNullOrEmpty(nome)
                ? _db.ListarProdutos() // Se não for informado o nome vai listar todos
                : _db.BuscarPorNome(nome); // Se o nome for informado vai ser filtrado pelo nome
                return Ok(lista);    // Retorna 200 com a lista em JSON

            }
            catch (Exception ex)
            {
                return ErroInterno(ex); // Retorna 500 se algo der errado com mensagem de possível erro do próprio MySQL como ("Erro de Conexão")
            }
        }

        // GET api/produtos/relatorio
        // Retorna os dados agregados da view vw_relatorio_estoque
        [HttpGet("relatorio")]
        public IActionResult GetRelatorio()
        {
            try { return Ok(_db.ObterRelatorio()); } // usa o try catch para pegar os relatórios com o SELECT
            catch (Exception ex) { return ErroInterno(ex); } // Catch para possível erro
        }

        // Get para buscar por ID
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var produto = _db.ConsultarPorId(id);
                return produto is null
                    ? NotFound(new { erro = $"Produto {id} não encontrado." }) // Caso produto não for encontrado ele cairá nessa 
                    : Ok(produto); // Retorna o produto pelo nome se for dado no parâmetro
            }
            catch (Exception ex) { return ErroInterno(ex); }
        }


        //Post de Cadastrar o produto
        [HttpPost]
        public IActionResult Post([FromBody] Produtos produto)
        {
            // Validações de entrada para tratar erros)
            if (string.IsNullOrWhiteSpace(produto.Nome))
                return BadRequest(new { erro = "Nome é obrigatório." });
            if (string.IsNullOrWhiteSpace(produto.Codigo))
                return BadRequest(new { erro = "Código é obrigatório." });
            if (produto.Quantidade < 0)
                return BadRequest(new { erro = "Quantidade não pode ser negativa." });
            if (produto.Preco <= 0)
                return BadRequest(new { erro = "Preço deve ser maior que zero." });

            // Try para tratar possíveis erros e retornar o produto
            try
            {
                produto.Id = _db.CriarProduto(produto);
                return CreatedAtAction(nameof(GetById), new { id = produto.Id }, produto);
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
                when (ex.Number == 1062) // Entrada Duplicada (código unico)
            {
                return Conflict(new { erro = $"Já existe um produto com o código '{produto.Codigo}'." });
            }
            catch (Exception ex) { return ErroInterno(ex); }
        }

        //Put de poder atualizar o algum produto
        [HttpPut("{id:int}")]
        public IActionResult Put(int id, [FromBody] Produtos produto)
        {
            if (id != produto.Id)
                return BadRequest(new { erro = "Id da URL diferente do corpo da requisição." });
            if (string.IsNullOrWhiteSpace(produto.Nome))
                return BadRequest(new { erro = "Nome é obrigatório." });
            if (produto.Quantidade < 0)
                return BadRequest(new { erro = "Quantidade não pode ser negativa." });
            if (produto.Preco <= 0)
                return BadRequest(new { erro = "Preço deve ser maior que zero." });

            // Try para tratar as excessões e conseguir atualizar o produto sem problemas
            try
            {
                return _db.AtualizarProduto(produto)
                    ? Ok(produto)
                    : NotFound(new { erro = $"Produto {id} não encontrado." });
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
                when (ex.Number == 1062)
            {
                return Conflict(new { erro = $"Já existe um produto com o código '{produto.Codigo}'." });
            }
            catch (Exception ex) { return ErroInterno(ex); }
        }

        // Delete onde irá ser feito a remoção de algum produto e pelo ID
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            // Try para verificar possíveis falhas
            try
            {
                return _db.RemoverProduto(id)
                    ? NoContent()   // 204 — cria um sucesso sem o corpo
                    : NotFound(new { erro = $"Produto {id} não encontrado." });
            }
            catch (Exception ex) { return ErroInterno(ex); }
        }

        private ObjectResult ErroInterno(Exception ex) =>
            StatusCode(500, new { erro = ex.Message });
    }
}
