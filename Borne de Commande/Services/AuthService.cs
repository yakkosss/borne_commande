using Borne_de_Commande.DAL;
using Borne_de_Commande.Models;

namespace Borne_de_Commande.Services;

/// <summary>
/// Service d'authentification (statique — une session par processus).
/// Protege les sections Cuisine et Administration selon le role.
/// </summary>
public static class AuthService
{
    public static Client?   CurrentUser  { get; private set; }
    // StoredRole garantit que IsCuisine/IsAdmin fonctionnent meme si Inscrit est null en base
    public static UserRole? StoredRole   { get; private set; }
    public static UserRole? CurrentRole  => CurrentUser?.Inscrit?.Role ?? StoredRole;

    public static bool IsAuthenticated => StoredRole.HasValue;
    public static bool IsAdmin         => CurrentRole == UserRole.admin;
    public static bool IsCuisine       => CurrentRole == UserRole.cuisine;

    public static string CurrentUserName =>
        CurrentUser?.Inscrit?.NomComplet.Trim() is { Length: > 0 } full ? full :
        CurrentUser?.Pseudo is { Length: > 0 } pseudo                   ? pseudo :
        StoredRole?.ToString() ?? string.Empty;

    /// <summary>Tente la connexion via role + mot de passe.</summary>
    public static bool Login(UserRole role, string password)
    {
        try
        {
            var dao = new ClientDAO();
            if (!dao.VerifierMotDePasseParRole(role, password)) return false;

            StoredRole  = role;
            CurrentUser = dao.GetByRole(role);
            return true;
        }
        catch { return false; }
    }

    /// <summary>Deconnecte l'utilisateur courant.</summary>
    public static void Logout()
    {
        CurrentUser = null;
        StoredRole  = null;
    }
}
