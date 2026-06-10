namespace Borne_de_Commande.Models;

public class Produit
{
    public int       Id          { get; set; }
    public string    Nom         { get; set; } = string.Empty;
    public string    Description { get; set; } = string.Empty;
    public decimal   Prix        { get; set; }
    public int?      CategorieId { get; set; }
    public Categorie? Categorie  { get; set; }

    /// <summary>
    /// Chemin relatif ou URL de l'image du produit.
    /// Ex : "Images/burgers/classic.jpg" ou vide si pas d'image.
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;

    public override string ToString() => Nom;
}
