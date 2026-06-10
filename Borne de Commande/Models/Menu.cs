namespace Borne_de_Commande.Models;

/// <summary>
/// Represent un menu (formule) compose de plusieurs produits a un prix fixe.
/// </summary>
public class Menu
{
    public int     Id          { get; set; }
    public string  Nom         { get; set; } = string.Empty;
    public string  Description { get; set; } = string.Empty;
    public decimal Prix        { get; set; }
    public string  ImagePath   { get; set; } = string.Empty;

    /// <summary>Produits inclus dans ce menu.</summary>
    public List<MenuProduit> Produits { get; set; } = [];

    public override string ToString() => Nom;
}

/// <summary>Ligne de composition d'un menu.</summary>
public class MenuProduit
{
    public int     MenuId    { get; set; }
    public int     ProduitId { get; set; }
    public string  NomProduit { get; set; } = string.Empty;
    public int     Quantite  { get; set; } = 1;
}
