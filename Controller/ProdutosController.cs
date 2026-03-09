using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Model;

namespace backend.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProdutosController(AppDbContext db) => _db = db;


        [HttpGet]
        public IActionResult GetAll([FromQuery] string? nome)
        {
            try
            {
                var lista = string.IsNullOrEmpty(nome)
                ? _db.ListarProdutos()
                : _db.BuscarPorNome(nome);
                return Ok(lista);

            }
            catch (Exception ex)
            {
                return ErroInterno(ex);
            }
        }

        [HttpGet("relatorio")]
        public IActionResult GetRelatorio()
        {
            try { return Ok(_db.ObterRelatorio()); } // usa o try catch para pegar os relatórios com o SELECT
            catch (Exception ex) { return ErroInterno(ex); }
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var produto = _db.ConsultarPorId(id);
                return produto is null
                    ? NotFound(new { erro = $"Produto {id} não encontrado." })
                    : Ok(produto);
            }
            catch (Exception ex) { return ErroInterno(ex); }
        }

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


        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
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
