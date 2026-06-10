using Borne_de_Commande.Models;
using MySql.Data.MySqlClient;

namespace Borne_de_Commande.DAL;

/// <summary>
/// CRUD sur la table <c>categories</c>.
/// </summary>
public class CategorieDAO
{
    // ── READ ALL ──────────────────────────────────────────────────────────────
    public List<Categorie> GetAll()
    {
        var list = new List<Categorie>();
        using var conn   = DatabaseConnection.GetOpenConnection();
        using var cmd    = new MySqlCommand("SELECT id, nom, description FROM categories ORDER BY nom", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    // ── READ ONE ──────────────────────────────────────────────────────────────
    public Categorie? GetById(int id)
    {
        using var conn   = DatabaseConnection.GetOpenConnection();
        using var cmd    = new MySqlCommand("SELECT id, nom, description FROM categories WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    // ── INSERT ────────────────────────────────────────────────────────────────
    public void Insert(Categorie c)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand("INSERT INTO categories (nom, description) VALUES (@nom, @desc)", conn);
        cmd.Parameters.AddWithValue("@nom",  c.Nom);
        cmd.Parameters.AddWithValue("@desc", c.Description);
        cmd.ExecuteNonQuery();
        c.Id = (int)cmd.LastInsertedId;
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────
    public void Update(Categorie c)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand("UPDATE categories SET nom = @nom, description = @desc WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@nom",  c.Nom);
        cmd.Parameters.AddWithValue("@desc", c.Description);
        cmd.Parameters.AddWithValue("@id",   c.Id);
        cmd.ExecuteNonQuery();
    }

    // ── DELETE ────────────────────────────────────────────────────────────────
    public void Delete(int id)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand("DELETE FROM categories WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static Categorie Map(MySqlDataReader r) => new()
    {
        Id          = r.GetInt32("id"),
        Nom         = r.GetString("nom"),
        Description = r.IsDBNull(r.GetOrdinal("description")) ? "" : r.GetString("description")
    };
}
