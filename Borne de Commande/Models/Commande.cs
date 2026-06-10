namespace Borne_de_Commande.Models;

/// <summary>
/// RG2 : les statuts possibles sont uniquement en_attente, en_preparation, prete.
/// </summary>
public enum StatutCommande
{
    en_attente,
    en_preparation,
    prete
}

public class Commande
{
    public int Id { get; set; }
    public string NumeroCommande  { get; set; } = string.Empty;  // CMD-YYYYMMDD-XXX
    public int? ClientId { get; set; }
    public Client? Client { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.Now;
    public StatutCommande Statut { get; set; } = StatutCommande.en_attente;
    public string ModePaiement { get; set; } = string.Empty;
    public decimal Total { get; set; }

    public List<LigneCommande> Lignes { get; set; } = [];

    public string StatutLibelle => Statut switch
    {
        StatutCommande.en_attente => "En attente",
        StatutCommande.en_preparation => "En preparation",
        StatutCommande.prete => "Prete",
        _ => Statut.ToString()
    };

    /// <summary>Genere un numero de commande unique.</summary>
    public static string GenererNumero()
    {
        int n = Random.Shared.Next(100, 999);
        return $"CMD-{DateTime.Now:yyyyMMdd}-{n}";
    }
}
