using Borne_de_Commande.Models;
using MySql.Data.MySqlClient;

namespace Borne_de_Commande.DAL;

/// <summary>
/// CRUD sur la table <c>commandes</c> et ses <c>lignes_commande</c>.
/// </summary>
public class CommandeDAO
{
    private readonly LigneCommandeDAO _ligneDAO = new();

    private const string SelectSql =
        "SELECT id, numero_commande, client_id, date_creation, statut, mode_paiement, total FROM commandes";

    // ── READ ALL (sans lignes) ────────────────────────────────────────────────
    public List<Commande> GetAll()
    {
        var list = new List<Commande>();
        using var conn   = DatabaseConnection.GetOpenConnection();
        using var cmd    = new MySqlCommand(SelectSql + " ORDER BY date_creation DESC", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    // ── READ BY STATUT ────────────────────────────────────────────────────────
    public List<Commande> GetByStatut(StatutCommande statut)
    {
        var list = new List<Commande>();
        using var conn   = DatabaseConnection.GetOpenConnection();
        using var cmd    = new MySqlCommand(SelectSql + " WHERE statut = @s ORDER BY date_creation", conn);
        cmd.Parameters.AddWithValue("@s", statut.ToString());
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) list.Add(Map(reader));
        return list;
    }

    // ── READ ONE WITH LIGNES ──────────────────────────────────────────────────
    public Commande? GetById(int id)
    {
        Commande? obj = null;
        using var conn   = DatabaseConnection.GetOpenConnection();
        using var cmd    = new MySqlCommand(SelectSql + " WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read()) obj = Map(reader);
        if (obj is not null) obj.Lignes = _ligneDAO.GetByCommande(id);
        return obj;
    }

    // ── INSERT (+ lignes, dans une transaction) ───────────────────────────────
    public void Insert(Commande c)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var tran = conn.BeginTransaction();
        try
        {
            using var cmd = new MySqlCommand(
                """
                INSERT INTO commandes (numero_commande, client_id, date_creation, statut, mode_paiement, total)
                VALUES (@num, @cid, @d, @s, @mp, @t)
                """, conn, tran);
            cmd.Parameters.AddWithValue("@num", c.NumeroCommande);
            cmd.Parameters.AddWithValue("@cid", (object?)c.ClientId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@d",   c.DateCreation);
            cmd.Parameters.AddWithValue("@s",   c.Statut.ToString());
            cmd.Parameters.AddWithValue("@mp",  c.ModePaiement);
            cmd.Parameters.AddWithValue("@t",   c.Total);
            cmd.ExecuteNonQuery();
            c.Id = (int)cmd.LastInsertedId;

            foreach (var ligne in c.Lignes)
            {
                ligne.CommandeId = c.Id;
                _ligneDAO.Insert(ligne, conn, tran);
            }
            tran.Commit();
        }
        catch { tran.Rollback(); throw; }
    }

    // ── UPDATE STATUT ─────────────────────────────────────────────────────────
    public void UpdateStatut(int id, StatutCommande statut)
    {
        using var conn = DatabaseConnection.GetOpenConnection();
        using var cmd  = new MySqlCommand("UPDATE commandes SET statut = @s WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@s",  statut.ToString());
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ── Mapper ────────────────────────────────────────────────────────────────
    private static Commande Map(MySqlDataReader r)
    {
        Enum.TryParse<StatutCommande>(r.GetString("statut"), out var statut);
        return new Commande
        {
            Id              = r.GetInt32("id"),
            NumeroCommande  = r.IsDBNull(r.GetOrdinal("numero_commande")) ? "" : r.GetString("numero_commande"),
            ClientId        = r.IsDBNull(r.GetOrdinal("client_id")) ? null : r.GetInt32("client_id"),
            DateCreation    = r.GetDateTime("date_creation"),
            Statut          = statut,
            ModePaiement    = r.IsDBNull(r.GetOrdinal("mode_paiement")) ? "" : r.GetString("mode_paiement"),
            Total           = r.GetDecimal("total")
        };
    }
}
