using MySql.Data.MySqlClient;

namespace Borne_de_Commande.DAL;

/// <summary>
/// Fournit une connexion a la base de donnees MySQL.
/// Modifier ConnectionString pour adapter a votre environnement.
/// </summary>
public static class DatabaseConnection
{
    // ─── A ADAPTER selon votre configuration MySQL ────────────────────────────
    public static string ConnectionString { get; set; } =
        "Server=localhost;Port=3306;Database=borne_commande;Uid=root;Pwd=;CharSet=utf8mb4;";
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Retourne une connexion ouverte.</summary>
    public static MySqlConnection GetOpenConnection()
    {
        var conn = new MySqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }
}
