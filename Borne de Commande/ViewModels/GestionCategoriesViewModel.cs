using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Borne_de_Commande.DAL;
using Borne_de_Commande.Models;

namespace Borne_de_Commande.ViewModels;

/// <summary>
/// Gestion CRUD des categories.
/// ObservableCollection notifie automatiquement la vue lors des ajouts/suppressions.
/// </summary>
public class GestionCategoriesViewModel : BaseViewModel
{
    private readonly CategorieDAO _dao = new();

    // ── Liste affichee ─────────────────────────────────────────────────────────
    public ObservableCollection<Categorie> Categories { get; } = [];

    // ── Selection et formulaire ────────────────────────────────────────────────
    private Categorie? _selected;
    public Categorie? Selected
    {
        get => _selected;
        set
        {
            SetProperty(ref _selected, value);
            if (value is not null) NomEdition = value.Nom;
        }
    }

    private string _nomEdition = string.Empty;
    public string NomEdition
    {
        get => _nomEdition;
        set => SetProperty(ref _nomEdition, value);
    }

    // ── Commandes ──────────────────────────────────────────────────────────────
    public ICommand AjouterCommand   { get; }
    public ICommand ModifierCommand  { get; }
    public ICommand SupprimerCommand { get; }
    public ICommand NouveauCommand   { get; }

    public GestionCategoriesViewModel()
    {
        AjouterCommand   = new RelayCommand(Ajouter,  () => !string.IsNullOrWhiteSpace(NomEdition));
        ModifierCommand  = new RelayCommand(Modifier, () => Selected is not null && !string.IsNullOrWhiteSpace(NomEdition));
        SupprimerCommand = new RelayCommand(Supprimer,() => Selected is not null);
        NouveauCommand   = new RelayCommand(Nouveau);
    }

    // ── Chargement ────────────────────────────────────────────────────────────
    public void Load()
    {
        try
        {
            Categories.Clear();
            foreach (var c in _dao.GetAll()) Categories.Add(c);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    // ── Actions ───────────────────────────────────────────────────────────────
    private void Nouveau()
    {
        Selected    = null;
        NomEdition  = string.Empty;
    }

    private void Ajouter()
    {
        try
        {
            var cat = new Categorie { Nom = NomEdition.Trim() };
            _dao.Insert(cat);
            Categories.Add(cat);
            Nouveau();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void Modifier()
    {
        if (Selected is null) return;
        try
        {
            Selected.Nom = NomEdition.Trim();
            _dao.Update(Selected);
            // Rafraichit la liste pour que le nouveau nom s'affiche
            Load();
            Nouveau();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void Supprimer()
    {
        if (Selected is null) return;
        var result = MessageBox.Show(
            $"Supprimer la categorie « {Selected.Nom} » ?",
            "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;
        try
        {
            _dao.Delete(Selected.Id);
            Categories.Remove(Selected);
            Nouveau();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private static void ShowError(Exception ex)
        => MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
}
