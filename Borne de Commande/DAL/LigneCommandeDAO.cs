using Borne_de_Commande.Models;
using MySql.Data.MySqlClient;

namespace Borne_de_Commande.DAL;

/// <summary>
/// Acces aux <c>lignes_commande</c>.
/// L'insertion accepte une transaction externe (utilisee par CommandeDAO).
/// </summary>
public class LigneCommandeDAO
{
    // ── READ BY COMMANDE ──────────────────────────────────────────────────────
    public List<LigneCommande> GetByCommande(int commandeId)
    {
        var list = new List<LigneCommande>();
        using var conn   = DatabaseConnection.GetOpenConnection();
        using var cmd    = new MySqlCommand(
            "SELECT id, commande_id, produit_id, nom_produit, prix_unitaire, quantite " +
            "FROM lignes_commande WHERE commande_id = @cid", conn);
        cmd.Parameters.AddWithValue("@cid", commandeId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    // ── INSERT (avec transaction externe) ─────────────────────────────────────
    public void Insert(LigneCommande l, MySqlConnection conn, MySqlTransaction tran)
    {
        using var cmd = new MySqlCommand(
            "INSERT INTO lignes_commande (commande_id, produit_id, nom_produit, prix_unitaire, quantite) " +
            "VALUES (@cid, @pid, @nom, @pu, @q)", conn, tran);
        cmd.Parameters.AddWithValue("@cid", l.CommandeId);
        cmd.Parameters.AddWithValue("@pid", (object?)l.ProduitId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@nom", l.NomProduit);
        cmd.Parameters.AddWithValue("@pu",  l.PrixUnitaire);
        cmd.Parameters.AddWithValue("@q",   l.Quantite);
        cmd.ExecuteNonQuery();
        l.Id = (int)cmd.LastInsertedId;
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static LigneCommande Map(MySqlDataReader r) => new()
    {
        Id           = r.GetInt32("id"),
        CommandeId   = r.GetInt32("commande_id"),
        ProduitId    = r.IsDBNull(r.GetOrdinal("produit_id")) ? null : r.GetInt32("produit_id"),
        NomProduit   = r.GetString("nom_produit"),
        PrixUnitaire = r.GetDecimal("prix_unitaire"),
        Quantite     = r.GetInt32("quantite")
    };
}
