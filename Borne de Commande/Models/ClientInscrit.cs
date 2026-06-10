namespace Borne_de_Commande.Models;

/// <summary>Role d'un compte inscrit.</summary>
public enum UserRole { admin, cuisine }

/// <summary>
/// Specialisation de Client pour les clients ayant un compte.
/// Lie a <see cref="Client"/> par la meme cle primaire (id_client).
/// </summary>
public class ClientInscrit
{
    public int      IdClient   { get; set; }
    public string   Nom        { get; set; } = string.Empty;
    public string   Prenom     { get; set; } = string.Empty;
    public string   Email      { get; set; } = string.Empty;
    public string   MotDePasse { get; set; } = string.Empty;  // stocke hashe
    public string   Telephone  { get; set; } = string.Empty;
    public string   Adresse    { get; set; } = string.Empty;
    public UserRole Role       { get; set; } = UserRole.admin;

    public string NomComplet => $"{Prenom} {Nom}";
}
