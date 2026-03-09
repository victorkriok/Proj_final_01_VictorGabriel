using backend.Model;
using MySql.Data.MySqlClient;

namespace backend.Data
{
    public class AppDbContext
    {
        private readonly string _connectionString;

        public AppDbContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
             ?? throw new InvalidOperationException("Connection String 'DefaultConnection' não encontrada.");
        }

        public MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        // Método usado para converter uma linha de resultado do banco de dados em um objeto
        private Produtos MapReader(MySqlDataReader r) => new()
        {
            Id = r.GetInt32("id"),
            Nome = r.GetString("nome"),
            Codigo = r.GetString("codigo"),
            Quantidade = r.GetInt32("quantidade"),
            Preco = r.GetDouble("preco"),
            EstoqueMinimo = r.GetInt32("estoqueMinimo")
        };

        public int CriarProduto(Produtos produto)
        {
            const string sql = @"
                INSERT INTO Produtos (nome, codigo, quantidade, preco, estoqueMinimo)
                VALUES (@nome, @codigo, @quantidade, @preco, @estoqueMinimo);
                SELECT LAST_INSERT_ID();";

            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", produto.Nome);
            cmd.Parameters.AddWithValue("@codigo", produto.Codigo);
            cmd.Parameters.AddWithValue("@quantidade", produto.Quantidade);
            cmd.Parameters.AddWithValue("@preco", produto.Preco);
            cmd.Parameters.AddWithValue("@estoqueMinimo", produto.EstoqueMinimo);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<Produtos> ListarProdutos()
        {
            const string sql = @"
                SELECT id, nome, codigo, quantidade, preco, estoqueMinimo
                FROM Produtos
                ORDER BY nome";

            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            var lista = new List<Produtos>();
            while (reader.Read())
                lista.Add(MapReader(reader));

            return lista;
        }

        public Produtos? ConsultarPorId(int id)
        {
            const string sql = @"
                SELECT id, nome, codigo, quantidade, preco, estoqueMinimo
                FROM Produtos
                WHERE id = @id";

            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapReader(reader) : null;
        }

        public List<Produtos> BuscarPorNome(string nome)
        {
            const string sql = @"
                SELECT id, nome, codigo, quantidade, preco, estoqueMinimo
                FROM Produtos
                WHERE nome LIKE @nome
                ORDER BY nome";

            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nome", $"%{nome}%");

            using var reader = cmd.ExecuteReader();
            var lista = new List<Produtos>();
            while (reader.Read())
                lista.Add(MapReader(reader));

            return lista;
        }

        public bool AtualizarProduto(Produtos produto)
        {
            const string sql = @"
                UPDATE Produtos
                SET nome          = @nome,
                    codigo        = @codigo,
                    quantidade    = @quantidade,
                    preco         = @preco,
                    estoqueMinimo = @estoqueMinimo
                WHERE id = @id";

            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", produto.Id);
            cmd.Parameters.AddWithValue("@nome", produto.Nome);
            cmd.Parameters.AddWithValue("@codigo", produto.Codigo);
            cmd.Parameters.AddWithValue("@quantidade", produto.Quantidade);
            cmd.Parameters.AddWithValue("@preco", produto.Preco);
            cmd.Parameters.AddWithValue("@estoqueMinimo", produto.EstoqueMinimo);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool RemoverProduto(int id)
        {
            const string sql = "DELETE FROM Produtos WHERE id = @id";

            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        public RelatorioEstoque ObterRelatorio()
        {
            const string sql = "SELECT TotalProdutos, TotalItens, ValorTotalEstoque FROM vw_relatorio_estoque";

            using var conn = OpenConnection();
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            if (!reader.Read()) return new RelatorioEstoque();

            return new RelatorioEstoque
            {
                TotalProdutos = reader.IsDBNull(0) ? 0 : reader.GetInt32(0), // IsDBNull utilizado para ver se a coluna acessada é uma coluna vazia
                TotalItens = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                ValorTotalEstoque = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2)
            };
        }

        // Classe de representação do Relatório
        public class RelatorioEstoque
        {
            public int TotalProdutos { get; set; }
            public int TotalItens { get; set; }
            public decimal ValorTotalEstoque { get; set; }
        }
    }
}
