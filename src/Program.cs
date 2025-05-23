using System;
using System.Data.SQLite;
using System.Globalization;
using Serilog;

class Program
{
    // String de conexão com o banco
    static string connectionString = "Data Source=filmes.db";

    static void Main()
    {
        // Configura o Serilog para console e arquivo txt
        Log.Logger = new LoggerConfiguration()
        .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();

        try
        {
            CriarTabela(); // Cria a tabela se não existir

            // Menu principal
            while (true)
            {
                Console.WriteLine("\n====== Menu Filmes =====");
                Console.WriteLine("1. Listar Filmes");
                Console.WriteLine("2. Buscar Filme por ID");
                Console.WriteLine("3. Cadastrar Filme");
                Console.WriteLine("4. Atualizar Filme");
                Console.WriteLine("5. Deletar Filme");
                Console.WriteLine("0. Sair");
                Console.Write("Escolha uma opção: ");
                var opcao = Console.ReadLine();

                // Escolhe o que fazer com base na opção
                switch (opcao)
                {
                    case "1": ListarFilmes(); break;
                    case "2": BuscarFilmePorId(); break;
                    case "3": CadastrarFilme(); break;
                    case "4": AtualizarFilme(); break;
                    case "5": DeletarFilme(); break;
                    case "0": return;
                    default: Console.WriteLine("Opção inválida!"); break;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Erro fatal no programa, Razão: " + ex.InnerException);
        }
        finally
        {
            Log.CloseAndFlush(); // Fecha o log certinho
        }
    }

    /// <summary>
    /// Criação de Tabela no banco de dados caso ela não exista
    /// </summary>
    static void CriarTabela()
    {
        // Cria a tabela no banco se ainda não existir
        try
        {
            Log.Debug("Entrou na função Criar Tabela");

            using var con = new SQLiteConnection(connectionString);
            con.Open();
            
            //Query para criação da Tabela no banco de dados
            var sql = @"
            CREATE TABLE IF NOT EXISTS Filmes (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Titulo TEXT NOT NULL,
            Diretor TEXT NOT NULL,
            Duracao INTEGER NOT NULL,
            DataLancamento TEXT,
            Genero TEXT
            )";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.ExecuteNonQuery();

            Log.Information("Tabela criada/verificada com sucesso.");
        }
        catch (Exception ex)
        {
            Log.Error("Criação do Banco de dados gerou um Erro: " + ex.Message + " | A Exception detalhada: " + ex.InnerException);
            throw new Exception();
        }
    }

    /// <summary>
    /// Realiza a listagem de todos os filmes registrados no banco.
    /// </summary>
    static void ListarFilmes()
    {
        try
        {
            Log.Debug("Entrou na função Listar Filmes");

            // Conexão com o Banco de Dados
            using var con = new SQLiteConnection(connectionString);
            con.Open();

            var sql = "SELECT * FROM Filmes";
            using var cmd = new SQLiteCommand(sql, con);
            using var reader = cmd.ExecuteReader();
            Console.WriteLine("\n=== Lista de Filmes ===");

            //Escreve a lista de Filmes buscados no banco
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader["Id"]} | Título: {reader["Titulo"]} | Diretor: {reader["Diretor"]} | Duração: {reader["Duracao"]} min | Lançamento: {reader["DataLancamento"]} | Gênero: {reader["Genero"]}");
            }

            Log.Information("Filmes listados com sucesso.");
        }
        catch (Exception ex)
        {
            Log.Error("Ocorreu um erro na listagem de filme: " + ex.Message + " | A Exception detalhada: "+ ex.InnerException);
            throw new Exception();
        }
    }

    /// <summary>
    /// Realiza a listagem de filme buscado pelo seu Id
    /// </summary>
    static void BuscarFilmePorId()
    {
        try
        {
            Log.Debug("Entrou na função Buscar Filme Por Id");

            Console.WriteLine("\n=== Filme por ID ===");
            Console.Write("Digite o ID do filme: ");

            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Log.Information("Id informado na busca de filme é inválido.");
                return;
            } //Caso o Id informado não seja Int ele ira retornar ao menu principal

            // Conexão com o Banco de Dados
            using var con = new SQLiteConnection(connectionString);
            con.Open();

            var sql = "SELECT * FROM Filmes WHERE Id = @id";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();

            // Listagem do Filme buscado no banco de dados
            if (reader.Read())
            {
                Console.WriteLine($"Título: {reader["Titulo"]}");
                Console.WriteLine($"Diretor: {reader["Diretor"]}");
                Console.WriteLine($"Duração: {reader["Duracao"]} minutos");
                Console.WriteLine($"Data de Lançamento: {reader["DataLancamento"]}");
                Console.WriteLine($"Gênero: {reader["Genero"]}");

                Log.Information("Filme encontrado com ID {Id}", id);
            }
            else
            {
                Console.WriteLine("Filme não encontrado.");
                Log.Warning("Tentativa de buscar filme inexistente com ID {Id}", id);
            }
        }
        catch (Exception ex)
        {
            Log.Error("Ocorreu um erro na listagem de filme utilizando Id: " + ex.Message + " | A Exception detalhada: " + ex.InnerException);
            throw new Exception();
        }
    }

    /// <summary>
    /// Cadastro de filmes.
    /// Os Do While garantem que a informação seja inserida corretamente.
    /// </summary>
    static void CadastrarFilme()
    {
        try
        {
            Log.Debug("Entrou na função Cadastrar Filme");

            // Pede o título
            string titulo;
            do
            {
                Console.Write("Título: ");
                titulo = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(titulo))
                {
                    Console.WriteLine("Título é obrigatório.");
                    Log.Information("Usuário tentou cadastrar sem informar o título");
                }
            } while (string.IsNullOrWhiteSpace(titulo));

            // Pede o diretor
            string diretor;
            do
            {
                Console.Write("Diretor: ");
                diretor = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(diretor))
                {
                    Console.WriteLine("Diretor é obrigatório.");
                    Log.Information("Usuário tentou cadastrar sem informar o diretor");
                }
            } while (string.IsNullOrWhiteSpace(diretor));

            // Pede a duração
            int duracao;
            do
            {
                Console.Write("Duração (em minutos): ");
                if (!int.TryParse(Console.ReadLine(), out duracao))
                {
                    Console.WriteLine("Duração inválida.");
                    Log.Information("Usuário informou duração inválida");
                }
            } while (duracao <= 0);

            // Pede a data de lançamento (opcional)
            Console.Write("Data de Lançamento (ddMMyyyy, opcional): ");
            var dataInput = Console.ReadLine();
            DateOnly? dataLancamento = null;
            if (!string.IsNullOrWhiteSpace(dataInput) && DateOnly.TryParseExact(dataInput, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                dataLancamento = data;

            // Pede o gênero (opcional)
            Console.Write("Gênero (opcional): ");
            var genero = Console.ReadLine();

            // Conexão com o Banco de Dados
            using var con = new SQLiteConnection(connectionString);
            con.Open();

            // Insere no banco o novo filme
            var sql = "INSERT INTO Filmes (Titulo, Diretor, Duracao, DataLancamento, Genero) VALUES (@titulo, @diretor, @duracao, @data, @genero)";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@titulo", titulo);
            cmd.Parameters.AddWithValue("@diretor", diretor);
            cmd.Parameters.AddWithValue("@duracao", duracao);
            cmd.Parameters.AddWithValue("@data", dataLancamento?.ToString("dd-MM-yyyy") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@genero", string.IsNullOrWhiteSpace(genero) ? (object)DBNull.Value : genero);
            cmd.ExecuteNonQuery();
            Console.WriteLine("Filme cadastrado com sucesso.");

            Log.Information("Novo filme cadastrado: {Titulo}", titulo);
        }
        catch (Exception ex)
        {
            Log.Error("Ocorreu um erro no Cadastro de Filme: " + ex.Message + " | A Exception detalhada: " + ex.InnerException);
            throw new Exception();
        }
    }

    /// <summary>
    /// Atualização de filme.
    /// Os Do While garantem que a informação seja inseridas.
    /// </summary>
    static void AtualizarFilme()
    {
        try
        {
            Log.Debug("Entrou na função Atualizar Filme");

            Console.Write("ID do filme a atualizar: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Log.Information("Id informado na atualização é inválido.");
                return;
            } //Caso o Id informado não seja Int ele ira retornar ao menu principal

            // Pede novo título
            string titulo;
            do
            {
                Console.Write("Novo Título: ");
                titulo = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(titulo))
                {
                    Log.Information("Usuário tentou atualizar sem informar o título");
                }
            } while (string.IsNullOrWhiteSpace(titulo));

            // Pede novo diretor
            string diretor;
            do
            {
                Console.Write("Novo Diretor: ");
                diretor = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(diretor))
                {
                    Log.Information("Usuário tentou atualizar sem informar o diretor");
                }
            } while (string.IsNullOrWhiteSpace(diretor));

            // Pede nova duração
            int duracao;
            do
            {
                Console.Write("Nova Duração: ");
                if (!int.TryParse(Console.ReadLine(), out duracao))
                {
                    Console.WriteLine("Duração inválida.");
                    Log.Information("Usuário informou nova duração inválida");
                }
            } while (duracao <= 0);

            // Nova data de lançamento
            Console.Write("Nova Data de Lançamento (ddMMyyyy): ");
            var dataInput = Console.ReadLine();
            DateOnly? dataLancamento = null;
            if (!string.IsNullOrWhiteSpace(dataInput) && DateOnly.TryParseExact(dataInput, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                dataLancamento = data;

            // Novo gênero
            Console.Write("Novo Gênero: ");
            var genero = Console.ReadLine();

            // Conexão com o Banco de Dados
            using var con = new SQLiteConnection(connectionString);
            con.Open();

            // Atualiza no banco o filme
            var sql = @"UPDATE Filmes SET 
                    Titulo = @titulo, Diretor = @diretor, Duracao = @duracao, 
                    DataLancamento = @data, Genero = @genero WHERE Id = @id";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@titulo", titulo);
            cmd.Parameters.AddWithValue("@diretor", diretor);
            cmd.Parameters.AddWithValue("@duracao", duracao);
            cmd.Parameters.AddWithValue("@data", dataLancamento?.ToString("dd-MM-yyyy") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@genero", string.IsNullOrWhiteSpace(genero) ? (object)DBNull.Value : genero);
            cmd.Parameters.AddWithValue("@id", id);
            var linhas = cmd.ExecuteNonQuery();
            Console.WriteLine(linhas > 0 ? "Filme atualizado." : "Filme não encontrado.");

            Log.Information("Atualização de filme. ID: {Id} - Sucesso: {Sucesso}", id, linhas > 0);
        }
        catch (Exception ex)
        {
            Log.Error("Ocorreu um erro na Atualização de Filme: " + ex.Message + " | A Exception detalhada: " + ex.InnerException);
            throw new Exception();
        }
    }

    /// <summary>
    /// Executa a Deleção de Filmes utilizando o Id informado
    /// </summary>
    static void DeletarFilme()
    {
        try
        {
            Log.Debug("Entrou na função Deletar Filme");

            Console.Write("ID do filme a deletar: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Log.Information("Id informado na deleção é inválido.");
                return;
            } //Caso o Id informado não seja Int ele ira retornar ao menu principal

            // Conexão com o Banco de Dados
            using var con = new SQLiteConnection(connectionString);
            con.Open();

            // Deleção do filme no banco de dados
            var sql = "DELETE FROM Filmes WHERE Id = @id";
            using var cmd = new SQLiteCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", id);
            var linhas = cmd.ExecuteNonQuery();
            Console.WriteLine(linhas > 0 ? "Filme deletado." : "Filme não encontrado.");

            Log.Information("Tentativa de deletar filme. ID: {Id} - Sucesso: {Sucesso}", id, linhas > 0);
        }
        catch (Exception ex)
        {
            Log.Error("Ocorreu um erro na Deleção de Filmes: " + ex.Message + " | A Exception detalhada: " + ex.InnerException);
            throw new Exception();
        }
    }
}
