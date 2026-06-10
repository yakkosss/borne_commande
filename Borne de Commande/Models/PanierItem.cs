using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Borne_de_Commande.Models;

/// <summary>
/// Represente un produit dans le panier temporaire (non persiste).
/// Implemente INotifyPropertyChanged pour que la vue se mette a jour
/// automatiquement quand la quantite change.
/// </summary>
public class PanierItem : INotifyPropertyChanged
{
    public Produit Produit { get; }

    private int _quantite;
    public int Quantite
    {
        get => _quantite;
        set
        {
            _quantite = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SousTotal));
        }
    }

    public decimal SousTotal => Produit.Prix * Quantite;

    public PanierItem(Produit produit, int quantite = 1)
    {
        Produit  = produit;
        _quantite = quantite;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
