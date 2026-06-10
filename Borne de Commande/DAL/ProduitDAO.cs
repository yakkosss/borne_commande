using Borne_de_Commande.Models;
using MySql.Data.MySqlClient;

namespace Borne_de_Commande.DAL;

/// <summary>
/// CRUD sur la table <c>produits</c>.
/// Chaque produit est joint a sa categorie (LEFT JOIN).
/// </summary>
public class ProduitDAO
{
    private const string SelectSql =
        """
        SELECT p.id, p.nom, p.description, p.prix, p.image_path, p.categorie_id,
               c.nom AS categorie_nom
        FROM produits p
        LEFT JOIN categories c ON c.id = p.categorie_id
        """;

    // ── READ ALL ──────────────────────────────────────────────────────────────
    public List<Produit> GetAll()
    {
        var list = new List<Produit>();
        using var conn   = DatabaseConnection.GetOpenConnection();
        using var cmd    = new MySqlCommand(SelectSql + " ORDER BY p.nom", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    // ── READ BY CATEGORIE ─────────────────────────────────────────────────────
    public List<Produit> GetByCategorie(int categorieId)
    {
        var list = new List<Produit>();
        using var conn   = DatabaseConnection.GetOpenConnection();
        using var cmd    = new MySqlCommand(SelectSql + " WHERE p.categorie_id = @cid ORDER BY p.nom", conn);
        cmd.Parameters.AddWithValue("@cid", categorieId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    // ── INSERT ────────────────────────────────────────────────────────────────
    public void Insert(Produit p)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand(
            "INSERT INTO produits (nom, description, prix, image_path, categorie_id) VALUES (@nom, @desc, @prix, @img, @cid)", conn);
        cmd.Parameters.AddWithValue("@nom",  p.Nom);
        cmd.Parameters.AddWithValue("@desc", p.Description);
        cmd.Parameters.AddWithValue("@prix", p.Prix);
        cmd.Parameters.AddWithValue("@img",  string.IsNullOrWhiteSpace(p.ImagePath) ? (object)DBNull.Value : p.ImagePath);
        cmd.Parameters.AddWithValue("@cid",  (object?)p.CategorieId ?? DBNull.Value);
        cmd.ExecuteNonQuery();
        p.Id = (int)cmd.LastInsertedId;
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────
    public void Update(Produit p)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand(
            "UPDATE produits SET nom=@nom, description=@desc, prix=@prix, image_path=@img, categorie_id=@cid WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("@nom",  p.Nom);
        cmd.Parameters.AddWithValue("@desc", p.Description);
        cmd.Parameters.AddWithValue("@prix", p.Prix);
        cmd.Parameters.AddWithValue("@img",  string.IsNullOrWhiteSpace(p.ImagePath) ? (object)DBNull.Value : p.ImagePath);
        cmd.Parameters.AddWithValue("@cid",  (object?)p.CategorieId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id",   p.Id);
        cmd.ExecuteNonQuery();
    }

    // ── DELETE ────────────────────────────────────────────────────────────────
    public void Delete(int id)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand("DELETE FROM produits WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static Produit Map(MySqlDataReader r)
    {
        int? catId = r.IsDBNull(r.GetOrdinal("categorie_id")) ? null : r.GetInt32("categorie_id");
        return new Produit
        {
            Id          = r.GetInt32("id"),
            Nom         = r.GetString("nom"),
            Description = r.IsDBNull(r.GetOrdinal("description")) ? "" : r.GetString("description"),
            Prix        = r.GetDecimal("prix"),
            ImagePath   = r.IsDBNull(r.GetOrdinal("image_path"))  ? "" : r.GetString("image_path"),
            CategorieId = catId,
            Categorie   = catId is null ? null : new Categorie
            {
                Id  = catId.Value,
                Nom = r.IsDBNull(r.GetOrdinal("categorie_nom")) ? "" : r.GetString("categorie_nom")
            }
        };
    }
}
