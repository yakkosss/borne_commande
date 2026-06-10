using Borne_de_Commande.Models;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Borne_de_Commande.DAL;

/// <summary>
/// CRUD sur les tables <c>client</c> et <c>client_inscrit</c>.
/// </summary>
public class ClientDAO
{
    // ── READ ALL ──────────────────────────────────────────────────────────────
    public List<Client> GetAll()
    {
        var list = new List<Client>();
        using var conn   = DatabaseConnection.GetOpenConnection();
        using var cmd    = new MySqlCommand(
            """
            SELECT c.id_client, c.numero_client, c.pseudo, c.type_client, c.date_creation,
                   ci.nom, ci.prenom, ci.email, ci.telephone, ci.adresse, ci.role
            FROM client c
            LEFT JOIN client_inscrit ci ON ci.id_client = c.id_client
            ORDER BY c.date_creation DESC
            """, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    // ── READ BY EMAIL (connexion) ─────────────────────────────────────────────
    public Client? GetByEmail(string email)
    {
        using var conn   = DatabaseConnection.GetOpenConnection();
        using var cmd    = new MySqlCommand(
            """
            SELECT c.id_client, c.numero_client, c.pseudo, c.type_client, c.date_creation,
                   ci.nom, ci.prenom, ci.email, ci.telephone, ci.adresse, ci.role
            FROM client c
            JOIN client_inscrit ci ON ci.id_client = c.id_client
            WHERE ci.email = @email
            """, conn);
        cmd.Parameters.AddWithValue("@email", email);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    // ── VERIFY PASSWORD (par email) ───────────────────────────────────────────
    public bool VerifierMotDePasse(string email, string motDePasse)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand(
            "SELECT mot_de_passe FROM client_inscrit WHERE email = @e", conn);
        cmd.Parameters.AddWithValue("@e", email);
        var hash = cmd.ExecuteScalar()?.ToString();
        return hash == HashMotDePasse(motDePasse);
    }

    // ── VERIFY PASSWORD PAR ROLE ──────────────────────────────────────────────
    public bool VerifierMotDePasseParRole(UserRole role, string motDePasse)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand(
            "SELECT mot_de_passe FROM client_inscrit WHERE role = @role LIMIT 1", conn);
        cmd.Parameters.AddWithValue("@role", role.ToString());
        var hash = cmd.ExecuteScalar()?.ToString();
        return hash is not null && hash == HashMotDePasse(motDePasse);
    }

    // ── READ BY ROLE ──────────────────────────────────────────────────────────
    public Client? GetByRole(UserRole role)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand(
            """
            SELECT c.id_client, c.numero_client, c.pseudo, c.type_client, c.date_creation,
                   ci.nom, ci.prenom, ci.email, ci.telephone, ci.adresse, ci.role
            FROM client c
            JOIN client_inscrit ci ON ci.id_client = c.id_client
            WHERE ci.role = @role
            LIMIT 1
            """, conn);
        cmd.Parameters.AddWithValue("@role", role.ToString());
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    // ── INSERT INVITE ─────────────────────────────────────────────────────────
    public Client InsererInvite(string pseudo)
    {
        var client = new Client
        {
            NumeroClient = Client.GenererNumeroInvite(),
            Pseudo       = pseudo,
            TypeClient   = TypeClient.invite,
            DateCreation = DateTime.Now
        };
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand(
            "INSERT INTO client (numero_client, pseudo, type_client) VALUES (@n, @p, 'invite')", conn);
        cmd.Parameters.AddWithValue("@n", client.NumeroClient);
        cmd.Parameters.AddWithValue("@p", client.Pseudo);
        cmd.ExecuteNonQuery();
        client.IdClient = (int)cmd.LastInsertedId;
        return client;
    }

    // ── INSERT INSCRIT ────────────────────────────────────────────────────────
    public Client InsererInscrit(ClientInscrit inscrit, string pseudo)
    {
        var client = new Client
        {
            NumeroClient = $"USR-{Random.Shared.Next(1000, 9999):D4}",
            Pseudo       = pseudo,
            TypeClient   = TypeClient.inscrit,
            DateCreation = DateTime.Now
        };
        using var conn = DatabaseConnection.GetOpenConnection();
        using var tran = conn.BeginTransaction();
        try
        {
            using var cmd1 = new MySqlCommand(
                "INSERT INTO client (numero_client, pseudo, type_client) VALUES (@n, @p, 'inscrit')", conn, tran);
            cmd1.Parameters.AddWithValue("@n", client.NumeroClient);
            cmd1.Parameters.AddWithValue("@p", client.Pseudo);
            cmd1.ExecuteNonQuery();
            client.IdClient = (int)cmd1.LastInsertedId;
            inscrit.IdClient = client.IdClient;

            using var cmd2 = new MySqlCommand(
                """
                INSERT INTO client_inscrit (id_client, nom, prenom, email, mot_de_passe, telephone, adresse, role)
                VALUES (@id, @nom, @prenom, @email, @mdp, @tel, @adr, @role)
                """, conn, tran);
            cmd2.Parameters.AddWithValue("@id",     inscrit.IdClient);
            cmd2.Parameters.AddWithValue("@nom",    inscrit.Nom);
            cmd2.Parameters.AddWithValue("@prenom", inscrit.Prenom);
            cmd2.Parameters.AddWithValue("@email",  inscrit.Email);
            cmd2.Parameters.AddWithValue("@mdp",    HashMotDePasse(inscrit.MotDePasse));
            cmd2.Parameters.AddWithValue("@tel",    inscrit.Telephone);
            cmd2.Parameters.AddWithValue("@adr",    inscrit.Adresse);
            cmd2.Parameters.AddWithValue("@role",   inscrit.Role.ToString());
            cmd2.ExecuteNonQuery();

            tran.Commit();
            client.Inscrit = inscrit;
            return client;
        }
        catch { tran.Rollback(); throw; }
    }

    // ── DELETE ────────────────────────────────────────────────────────────────
    public void Delete(int idClient)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand("DELETE FROM client WHERE id_client = @id", conn);
        cmd.Parameters.AddWithValue("@id", idClient);
        cmd.ExecuteNonQuery();
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static Client Map(MySqlDataReader r)
    {
        Enum.TryParse<TypeClient>(r.GetString("type_client"), out var type);
        var client = new Client
        {
            IdClient     = r.GetInt32("id_client"),
            NumeroClient = r.GetString("numero_client"),
            Pseudo       = r.GetString("pseudo"),
            TypeClient   = type,
            DateCreation = r.GetDateTime("date_creation")
        };
        if (type == TypeClient.inscrit && !r.IsDBNull(r.GetOrdinal("email")))
        {
            Enum.TryParse<UserRole>(
                r.IsDBNull(r.GetOrdinal("role")) ? "admin" : r.GetString("role"),
                out var role);

            client.Inscrit = new ClientInscrit
            {
                IdClient  = client.IdClient,
                Nom       = r.IsDBNull(r.GetOrdinal("nom"))       ? "" : r.GetString("nom"),
                Prenom    = r.IsDBNull(r.GetOrdinal("prenom"))    ? "" : r.GetString("prenom"),
                Email     = r.IsDBNull(r.GetOrdinal("email"))     ? "" : r.GetString("email"),
                Telephone = r.IsDBNull(r.GetOrdinal("telephone")) ? "" : r.GetString("telephone"),
                Adresse   = r.IsDBNull(r.GetOrdinal("adresse"))   ? "" : r.GetString("adresse"),
                Role      = role,
            };
        }
        return client;
    }

    // ── Hash simple (SHA-256) ─────────────────────────────────────────────────
    // En production, preferer BCrypt (NuGet BCrypt.Net-Next)
    private static string HashMotDePasse(string mdp)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(mdp));
        return Convert.ToHexString(bytes).ToLower();
    }
}
