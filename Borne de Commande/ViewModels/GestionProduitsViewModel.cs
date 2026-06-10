using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Borne_de_Commande.DAL;
using Borne_de_Commande.Models;

namespace Borne_de_Commande.ViewModels;

/// <summary>
/// Gestion CRUD des produits avec filtrage par categorie.
/// Propose deux modes d'affichage : liste (ListBox) et tableau (DataGrid).
/// </summary>
public class GestionProduitsViewModel : BaseViewModel
{
    private readonly ProduitDAO  _produitDAO  = new();
    private readonly CategorieDAO _catDAO     = new();

    // ── Donnees ───────────────────────────────────────────────────────────────
    public ObservableCollection<Produit>   Produits       { get; } = [];
    public ObservableCollection<Categorie> Categories     { get; } = [];

    // ── Filtre ────────────────────────────────────────────────────────────────
    private Categorie? _filtreCategorie;
    public Categorie? FiltreCategorie
    {
        get => _filtreCategorie;
        set { SetProperty(ref _filtreCategorie, value); AppliquerFiltre(); }
    }

    // ── Selection ─────────────────────────────────────────────────────────────
    private Produit? _selected;
    public Produit? Selected
    {
        get => _selected;
        set
        {
            SetProperty(ref _selected, value);
            if (value is not null)
            {
                NomEdition         = value.Nom;
                DescriptionEdition = value.Description;
                PrixEdition        = value.Prix;
                CategorieEdition   = Categories.FirstOrDefault(c => c.Id == value.CategorieId);
            }
        }
    }

    // ── Formulaire ────────────────────────────────────────────────────────────
    private string    _nomEdition         = string.Empty;
    private string    _descriptionEdition = string.Empty;
    private decimal   _prixEdition;
    private Categorie? _categorieEdition;

    public string    NomEdition         { get => _nomEdition;         set => SetProperty(ref _nomEdition, value); }
    public string    DescriptionEdition { get => _descriptionEdition; set => SetProperty(ref _descriptionEdition, value); }
    public decimal   PrixEdition        { get => _prixEdition;        set => SetProperty(ref _prixEdition, value); }
    public Categorie? CategorieEdition  { get => _categorieEdition;   set => SetProperty(ref _categorieEdition, value); }

    // ── Mode d'affichage ──────────────────────────────────────────────────────
    private bool _modeTableau = true;
    public bool ModeTableau
    {
        get => _modeTableau;
        set { SetProperty(ref _modeTableau, value); OnPropertyChanged(nameof(ModeListe)); }
    }
    public bool ModeListe => !ModeTableau;

    // ── Commandes ─────────────────────────────────────────────────────────────
    public ICommand AjouterCommand      { get; }
    public ICommand ModifierCommand     { get; }
    public ICommand SupprimerCommand    { get; }
    public ICommand NouveauCommand      { get; }
    public ICommand BasculerModeCommand { get; }

    public GestionProduitsViewModel()
    {
        AjouterCommand      = new RelayCommand(Ajouter,   () => !string.IsNullOrWhiteSpace(NomEdition) && PrixEdition > 0);
        ModifierCommand     = new RelayCommand(Modifier,  () => Selected is not null && !string.IsNullOrWhiteSpace(NomEdition));
        SupprimerCommand    = new RelayCommand(Supprimer, () => Selected is not null);
        NouveauCommand      = new RelayCommand(Nouveau);
        BasculerModeCommand = new RelayCommand(() => ModeTableau = !ModeTableau);
    }

    // ── Chargement ────────────────────────────────────────────────────────────
    public void Load()
    {
        try
        {
            Categories.Clear();
            Categories.Add(new Categorie { Id = 0, Nom = "(Toutes)" });
            foreach (var c in _catDAO.GetAll()) Categories.Add(c);
            FiltreCategorie = Categories[0];
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void AppliquerFiltre()
    {
        try
        {
            Produits.Clear();
            var liste = (_filtreCategorie is null || _filtreCategorie.Id == 0)
                ? _produitDAO.GetAll()
                : _produitDAO.GetByCategorie(_filtreCategorie.Id);
            foreach (var p in liste) Produits.Add(p);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    // ── Actions ───────────────────────────────────────────────────────────────
    private void Nouveau()
    {
        Selected           = null;
        NomEdition         = string.Empty;
        DescriptionEdition = string.Empty;
        PrixEdition        = 0;
        CategorieEdition   = null;
    }

    private void Ajouter()
    {
        try
        {
            var p = new Produit
            {
                Nom         = NomEdition.Trim(),
                Description = DescriptionEdition.Trim(),
                Prix        = PrixEdition,
                CategorieId = CategorieEdition?.Id == 0 ? null : CategorieEdition?.Id,
                Categorie   = CategorieEdition?.Id == 0 ? null : CategorieEdition
            };
            _produitDAO.Insert(p);
            AppliquerFiltre();
            Nouveau();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void Modifier()
    {
        if (Selected is null) return;
        try
        {
            Selected.Nom         = NomEdition.Trim();
            Selected.Description = DescriptionEdition.Trim();
            Selected.Prix        = PrixEdition;
            Selected.CategorieId = CategorieEdition?.Id == 0 ? null : CategorieEdition?.Id;
            _produitDAO.Update(Selected);
            AppliquerFiltre();
            Nouveau();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void Supprimer()
    {
        if (Selected is null) return;
        var result = MessageBox.Show(
            $"Supprimer le produit « {Selected.Nom} » ?",
            "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;
        try
        {
            _produitDAO.Delete(Selected.Id);
            Produits.Remove(Selected);
            Nouveau();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private static void ShowError(Exception ex)
        => MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
}
