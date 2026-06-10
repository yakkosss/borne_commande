namespace Borne_de_Commande.Models;

public class Categorie
{
    public int    Id          { get; set; }
    public string Nom         { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public override string ToString() => Nom;
}
