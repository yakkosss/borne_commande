using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Borne_de_Commande.DAL;
using Borne_de_Commande.Models;

namespace Borne_de_Commande.ViewModels;

/// <summary>
/// Interface client de la borne.
/// Parcours : categorie → produits → panier → paiement.
/// </summary>
public class BorneViewModel : BaseViewModel
{
    private readonly MainViewModel  _main;
    private readonly CategorieDAO   _catDAO   = new();
    private readonly ProduitDAO     _prodDAO  = new();
    private readonly ClientDAO      _clientDAO = new();

    /// <summary>
    /// RG1 : client invite cree automatiquement a l'ouverture de la borne.
    /// Un vrai compte est utilise si l'utilisateur s'est connecte.
    /// </summary>
    public Client? ClientActif { get; private set; }

    // ── Donnees ───────────────────────────────────────────────────────────────
    public ObservableCollection<Categorie>  Categories { get; } = [];
    public ObservableCollection<Produit>    Produits   { get; } = [];
    public ObservableCollection<PanierItem> Panier     { get; } = [];

    // ── Categorie selectionnee ─────────────────────────────────────────────────
    private Categorie? _categorieActive;
    public Categorie? CategorieActive
    {
        get => _categorieActive;
        set
        {
            SetProperty(ref _categorieActive, value);
            ChargerProduits();
        }
    }

    // ── Total panier ──────────────────────────────────────────────────────────
    public decimal Total => Panier.Sum(i => i.SousTotal);

    // ── Commandes ─────────────────────────────────────────────────────────────
    public ICommand SelectionnerCategorieCommand { get; }
    public ICommand AjouterAuPanierCommand       { get; }
    public ICommand RetirerDuPanierCommand       { get; }
    public ICommand AugmenterQuantiteCommand     { get; }
    public ICommand DiminuerQuantiteCommand      { get; }
    public ICommand ValiderPanierCommand         { get; }
    public ICommand ViderPanierCommand           { get; }

    public BorneViewModel(MainViewModel main)
    {
        _main = main;

        SelectionnerCategorieCommand = new RelayCommand(p => CategorieActive = p as Categorie);
        AjouterAuPanierCommand       = new RelayCommand(p => AjouterAuPanier(p as Produit));
        RetirerDuPanierCommand       = new RelayCommand(p => RetirerDuPanier(p as PanierItem));
        AugmenterQuantiteCommand     = new RelayCommand(p => Augmenter(p as PanierItem));
        DiminuerQuantiteCommand      = new RelayCommand(p => Diminuer(p as PanierItem));
        ValiderPanierCommand         = new RelayCommand(ValiderPanier, () => Panier.Count > 0);
        ViderPanierCommand           = new RelayCommand(ViderPanier,   () => Panier.Count > 0);
    }

    // ── Chargement ────────────────────────────────────────────────────────────
    public void ResetCommande()
    {
        // RG1 : creer un client invite anonyme avec numero aleatoire
        try { ClientActif = _clientDAO.InsererInvite($"Invite"); }
        catch { /* si la DB est inaccessible, on continue sans client */ }

        try
        {
            Categories.Clear();
            foreach (var c in _catDAO.GetAll()) Categories.Add(c);
            CategorieActive = Categories.FirstOrDefault();
        }
        catch (Exception ex) { ShowError(ex); }
        ViderPanier();
    }

    private void ChargerProduits()
    {
        Produits.Clear();
        if (_categorieActive is null) return;
        try
        {
            foreach (var p in _prodDAO.GetByCategorie(_categorieActive.Id)) Produits.Add(p);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    // ── Actions panier ────────────────────────────────────────────────────────
    private void AjouterAuPanier(Produit? produit)
    {
        if (produit is null) return;
        var item = Panier.FirstOrDefault(i => i.Produit.Id == produit.Id);
        if (item is not null)
            item.Quantite++;
        else
            Panier.Add(new PanierItem(produit));
        NotifierTotal();
    }

    private void RetirerDuPanier(PanierItem? item)
    {
        if (item is null) return;
        Panier.Remove(item);
        NotifierTotal();
    }

    private void Augmenter(PanierItem? item)
    {
        if (item is null) return;
        item.Quantite++;
        NotifierTotal();
    }

    private void Diminuer(PanierItem? item)
    {
        if (item is null) return;
        if (item.Quantite <= 1) Panier.Remove(item);
        else item.Quantite--;
        NotifierTotal();
    }

    private void ViderPanier()
    {
        Panier.Clear();
        NotifierTotal();
    }

    private void ValiderPanier()
    {
        if (Panier.Count == 0) return;
        var paiementVM = new PaiementViewModel(_main, new List<PanierItem>(Panier), ClientActif);
        _main.NavigateToPaiement(paiementVM);
    }

    private void NotifierTotal() => OnPropertyChanged(nameof(Total));

    private static void ShowError(Exception ex)
        => MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
}
