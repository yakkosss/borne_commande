namespace Borne_de_Commande.Models;

public enum TypeClient { invite, inscrit }

/// <summary>
/// RG1 : Un client peut etre invite (anonyme) ou inscrit.
/// Les invites ont un numero aleatoire (ex. INV-7829) et un pseudo genere.
/// </summary>
public class Client
{
    public int         IdClient      { get; set; }
    public string      NumeroClient  { get; set; } = string.Empty;  // INV-XXXX ou USR-XXXX
    public string      Pseudo        { get; set; } = string.Empty;
    public TypeClient  TypeClient    { get; set; } = TypeClient.invite;
    public DateTime    DateCreation  { get; set; } = DateTime.Now;

    /// <summary>Donnees supplementaires si le client est inscrit.</summary>
    public ClientInscrit? Inscrit    { get; set; }

    public override string ToString() => $"{Pseudo} ({NumeroClient})";

    /// <summary>Genere un numero aleatoire pour un client invite.</summary>
    public static string GenererNumeroInvite()
    {
        int n = Random.Shared.Next(1000, 9999);
        return $"INV-{n}";
    }
}
