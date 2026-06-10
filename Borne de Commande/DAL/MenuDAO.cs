using Borne_de_Commande.Models;
using MySql.Data.MySqlClient;

namespace Borne_de_Commande.DAL;

/// <summary>CRUD sur les tables <c>menus</c> et <c>menu_produits</c>.</summary>
public class MenuDAO
{
    // ── READ ALL ──────────────────────────────────────────────────────────────
    public List<Menu> GetAll()
    {
        var list = new List<Menu>();
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand(
            "SELECT id, nom, description, prix, image_path FROM menus ORDER BY nom", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapMenu(r));
        return list;
    }

    // ── READ ONE (avec ses produits) ──────────────────────────────────────────
    public Menu? GetById(int id)
    {
        Menu? menu;
        using (var conn = DatabaseConnection.GetOpenConnection())
        {
            using var cmd = new MySqlCommand(
                "SELECT id, nom, description, prix, image_path FROM menus WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            menu = MapMenu(r);
        }

        using (var conn2 = DatabaseConnection.GetOpenConnection())
        {
            using var cmd2 = new MySqlCommand(
                """
                SELECT mp.produit_id, p.nom AS nom_produit, mp.quantite
                FROM menu_produits mp
                JOIN produits p ON p.id = mp.produit_id
                WHERE mp.menu_id = @mid
                """, conn2);
            cmd2.Parameters.AddWithValue("@mid", id);
            using var r2 = cmd2.ExecuteReader();
            while (r2.Read())
                menu.Produits.Add(new MenuProduit
                {
                    MenuId     = id,
                    ProduitId  = r2.GetInt32("produit_id"),
                    NomProduit = r2.GetString("nom_produit"),
                    Quantite   = r2.GetInt32("quantite")
                });
        }
        return menu;
    }

    // ── INSERT ────────────────────────────────────────────────────────────────
    public void Insert(Menu m)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand(
            "INSERT INTO menus (nom, description, prix, image_path) VALUES (@n, @d, @p, @img)", conn);
        cmd.Parameters.AddWithValue("@n",   m.Nom);
        cmd.Parameters.AddWithValue("@d",   m.Description);
        cmd.Parameters.AddWithValue("@p",   m.Prix);
        cmd.Parameters.AddWithValue("@img", m.ImagePath);
        cmd.ExecuteNonQuery();
        m.Id = (int)cmd.LastInsertedId;
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────
    public void Update(Menu m)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand(
            "UPDATE menus SET nom=@n, description=@d, prix=@p, image_path=@img WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("@n",   m.Nom);
        cmd.Parameters.AddWithValue("@d",   m.Description);
        cmd.Parameters.AddWithValue("@p",   m.Prix);
        cmd.Parameters.AddWithValue("@img", m.ImagePath);
        cmd.Parameters.AddWithValue("@id",  m.Id);
        cmd.ExecuteNonQuery();
    }

    // ── DELETE ────────────────────────────────────────────────────────────────
    public void Delete(int id)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand("DELETE FROM menus WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static Menu MapMenu(MySqlDataReader r) => new()
    {
        Id          = r.GetInt32("id"),
        Nom         = r.GetString("nom"),
        Description = r.IsDBNull(r.GetOrdinal("description")) ? "" : r.GetString("description"),
        Prix        = r.GetDecimal("prix"),
        ImagePath   = r.IsDBNull(r.GetOrdinal("image_path"))  ? "" : r.GetString("image_path"),
    };
}
