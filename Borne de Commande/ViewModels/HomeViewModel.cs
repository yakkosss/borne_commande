using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Borne_de_Commande.DAL;
using Borne_de_Commande.Models;

namespace Borne_de_Commande.ViewModels;

/// <summary>
/// Dashboard / ecran d'accueil de la borne.
/// Affiche les categories et permet de demarrer une commande.
/// </summary>
public class HomeViewModel : BaseViewModel
{
    private readonly MainViewModel _main;
    private readonly CategorieDAO  _catDAO = new();

    public ObservableCollection<Categorie> Categories { get; } = [];

    // ── Commandes ──────────────────────────────────────────────────────────────
    public ICommand CommencerCommandeCommand        { get; }
    public ICommand CommanderDepuisCategorieCommand { get; }

    public HomeViewModel(MainViewModel main)
    {
        _main = main;

        CommencerCommandeCommand        = new RelayCommand(CommencerCommande);
        CommanderDepuisCategorieCommand = new RelayCommand(p => CommanderDepuisCategorie(p as Categorie));
    }

    public void Load()
    {
        try
        {
            Categories.Clear();
            foreach (var c in _catDAO.GetAll()) Categories.Add(c);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CommencerCommande()
    {
        _main.BorneVM.ResetCommande();
        _main.CurrentViewModel = _main.BorneVM;
    }

    private void CommanderDepuisCategorie(Categorie? cat)
    {
        _main.BorneVM.ResetCommande();
        if (cat is not null)
            _main.BorneVM.CategorieActive = cat;
        _main.CurrentViewModel = _main.BorneVM;
    }
}
