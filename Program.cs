using System;
using System.Data.SQLite;
using System.Globalization;

class Program
{
    static string connectionString = "Data Source=filmes.db";

    static void Main()
    {
        CriarTabela();

        while (true)
        {
            Console.WriteLine("\n===== Menu Filmes =====");
            Console.WriteLine("1. Listar Filmes");
            Console.WriteLine("2. Buscar Filme por ID");
            Console.WriteLine("3. Cadastrar Filme");
            Console.WriteLine("4. Atualizar Filme");
            Console.WriteLine("5. Deletar Filme");
            Console.WriteLine("0. Sair");
            Console.Write("Escolha uma opção: ");
            var opcao = Console.ReadLine();

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

    static void CriarTabela()
    {
        using var con = new SQLiteConnection(connectionString);
        con.Open();
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
    }

    static void ListarFilmes()
    {
        using var con = new SQLiteConnection(connectionString);
        con.Open();
        var sql = "SELECT * FROM Filmes";
        using var cmd = new SQLiteCommand(sql, con);
        using var reader = cmd.ExecuteReader();
        Console.WriteLine("\n=== Lista de Filmes ===");
        while (reader.Read())
        {
            Console.WriteLine($"ID: {reader["Id"]} | Título: {reader["Titulo"]} | Diretor: {reader["Diretor"]} | Duração: {reader["Duracao"]} min | Lançamento: {reader["DataLancamento"]} | Gênero: {reader["Genero"]}");
        }
    }

    static void BuscarFilmePorId()
    {
        Console.WriteLine("\n=== Filme por ID ===");
        Console.Write("Digite o ID do filme: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) return;

        using var con = new SQLiteConnection(connectionString);
        con.Open();
        var sql = "SELECT * FROM Filmes WHERE Id = @id";
        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            Console.WriteLine($"Título: {reader["Titulo"]}");
            Console.WriteLine($"Diretor: {reader["Diretor"]}");
            Console.WriteLine($"Duração: {reader["Duracao"]} minutos");
            Console.WriteLine($"Data de Lançamento: {reader["DataLancamento"]}");
            Console.WriteLine($"Gênero: {reader["Genero"]}");
        }
        else Console.WriteLine("Filme não encontrado.");
    }

    static void CadastrarFilme()
    {
        string titulo;
        do
        {
            Console.Write("Título: ");
            titulo = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(titulo))
            {
                Console.WriteLine("Título é obrigatório.");
            }
        } while (string.IsNullOrWhiteSpace(titulo));

        string diretor;
        do
        {
            Console.Write("Diretor: ");
            diretor = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(diretor))
            {
                Console.WriteLine("Diretor é obrigatório.");
            }
        } while (string.IsNullOrWhiteSpace(diretor));

        int duracao;
        do
        {
            Console.Write("Duração (em minutos): ");
            if (!int.TryParse(Console.ReadLine(), out duracao))
            {
                Console.WriteLine("Duração inválida.");
            }
        } while (duracao <= 0);

        Console.Write("Data de Lançamento (dd-MM-yyyy, opcional): ");
        var dataInput = Console.ReadLine();
        DateOnly? dataLancamento = null;
        if (!string.IsNullOrWhiteSpace(dataInput) && DateOnly.TryParseExact(dataInput, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            dataLancamento = data;

        Console.Write("Gênero (opcional): ");
        var genero = Console.ReadLine();

        using var con = new SQLiteConnection(connectionString);
        con.Open();
        var sql = "INSERT INTO Filmes (Titulo, Diretor, Duracao, DataLancamento, Genero) VALUES (@titulo, @diretor, @duracao, @data, @genero)";
        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@titulo", titulo);
        cmd.Parameters.AddWithValue("@diretor", diretor);
        cmd.Parameters.AddWithValue("@duracao", duracao);
        cmd.Parameters.AddWithValue("@data", dataLancamento?.ToString("dd-MM-yyyy") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@genero", string.IsNullOrWhiteSpace(genero) ? (object)DBNull.Value : genero);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Filme cadastrado com sucesso.");
    }

    static void AtualizarFilme()
    {
        Console.Write("ID do filme a atualizar: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) return;

        string titulo;
        do
        {
            Console.Write("Novo Título: ");
            titulo = Console.ReadLine();
        } while (string.IsNullOrWhiteSpace(titulo));

        string diretor;
        do
        {
            Console.Write("Novo Diretor: ");
            diretor = Console.ReadLine();
        } while (string.IsNullOrWhiteSpace(diretor));

        int duracao;
        do
        {
            Console.Write("Nova Duração: ");
            if (!int.TryParse(Console.ReadLine(), out duracao))
            {
                Console.WriteLine("Duração inválida.");
            }
        } while (duracao <= 0);

        Console.Write("Nova Data de Lançamento (dd-MM-yyyy): ");
        var dataInput = Console.ReadLine();
        DateOnly? dataLancamento = null;
        if (!string.IsNullOrWhiteSpace(dataInput) && DateOnly.TryParseExact(dataInput, "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            dataLancamento = data;

        Console.Write("Novo Gênero: ");
        var genero = Console.ReadLine();

        using var con = new SQLiteConnection(connectionString);
        con.Open();
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
    }

    static void DeletarFilme()
    {
        Console.Write("ID do filme a deletar: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) return;

        using var con = new SQLiteConnection(connectionString);
        con.Open();
        var sql = "DELETE FROM Filmes WHERE Id = @id";
        using var cmd = new SQLiteCommand(sql, con);
        cmd.Parameters.AddWithValue("@id", id);
        var linhas = cmd.ExecuteNonQuery();
        Console.WriteLine(linhas > 0 ? "Filme deletado." : "Filme não encontrado.");
    }
}
