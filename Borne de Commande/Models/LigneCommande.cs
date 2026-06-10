namespace Borne_de_Commande.Models;

public class LigneCommande
{
    public int     Id           { get; set; }
    public int     CommandeId   { get; set; }
    public int?    ProduitId    { get; set; }
    public string  NomProduit   { get; set; } = string.Empty;
    public decimal PrixUnitaire { get; set; }
    public int     Quantite     { get; set; }

    public decimal SousTotal => PrixUnitaire * Quantite;
}
