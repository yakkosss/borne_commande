using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Borne_de_Commande.DAL;
using Borne_de_Commande.Models;

namespace Borne_de_Commande.ViewModels;

/// <summary>
/// Interface cuisine.
/// - Rafraichissement automatique toutes les 10 secondes (multi-instances).
/// - Les commandes "prete" disparaissent de la vue apres 20 secondes
///   (elles restent en base de donnees).
/// </summary>
public class CuisineViewModel : BaseViewModel
{
    private readonly CommandeDAO    _dao  = new();
    private readonly DispatcherTimer _timerRefresh;

    // Suivi de l'expiration visuelle des commandes "prete"
    // Cle = Id commande, Valeur = heure a laquelle elle est entree dans "prete"
    private readonly Dictionary<int, DateTime>         _pretesDepuis     = new();
    private readonly Dictionary<int, DispatcherTimer>  _timersExpiration = new();

    // ── Collections par statut ────────────────────────────────────────────────
    public ObservableCollection<Commande> EnAttente     { get; } = [];
    public ObservableCollection<Commande> EnPreparation { get; } = [];
    public ObservableCollection<Commande> Pretes        { get; } = [];

    // ── Commande selectionnee ─────────────────────────────────────────────────
    private Commande? _selected;
    public Commande? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    // ── Actions ───────────────────────────────────────────────────────────────
    public ICommand RemettrEnAttenteCommand { get; }
    public ICommand PrendreEnChargeCommand  { get; }
    public ICommand MarquerPreteCommand     { get; }
    public ICommand ActualiserCommand       { get; }

    public CuisineViewModel()
    {
        RemettrEnAttenteCommand = new RelayCommand(p => ChangerStatut(p as Commande, StatutCommande.en_attente));
        PrendreEnChargeCommand  = new RelayCommand(p => ChangerStatut(p as Commande, StatutCommande.en_preparation));
        MarquerPreteCommand     = new RelayCommand(p => ChangerStatut(p as Commande, StatutCommande.prete));
        ActualiserCommand       = new RelayCommand(Load);

        // Rafraichissement automatique toutes les 10 secondes
        _timerRefresh = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
        _timerRefresh.Tick += (_, _) => Load();
        _timerRefresh.Start();
    }

    // ── Chargement ────────────────────────────────────────────────────────────
    public void Load()
    {
        try
        {
            Reload(EnAttente,     StatutCommande.en_attente);
            Reload(EnPreparation, StatutCommande.en_preparation);
            ReloadPretes();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Reload(ObservableCollection<Commande> col, StatutCommande statut)
    {
        col.Clear();
        foreach (var c in _dao.GetByStatut(statut))
        {
            c.Lignes = new CommandeDAO().GetById(c.Id)?.Lignes ?? [];
            col.Add(c);
        }
    }

    /// <summary>
    /// Recharge la colonne "Prete" en gerant l'expiration visuelle.
    /// Une commande disparait de la vue 20 secondes apres y etre apparue,
    /// sans etre supprimee de la base de donnees.
    /// </summary>
    private void ReloadPretes()
    {
        var commandes = _dao.GetByStatut(StatutCommande.prete);
        Pretes.Clear();

        foreach (var c in commandes)
        {
            c.Lignes = new CommandeDAO().GetById(c.Id)?.Lignes ?? [];

            // Premiere apparition : enregistrer l'heure et demarrer le timer
            if (!_pretesDepuis.ContainsKey(c.Id))
            {
                _pretesDepuis[c.Id] = DateTime.Now;
                DemarrerTimerExpiration(c);
            }

            // N'afficher que si < 20 secondes
            if ((DateTime.Now - _pretesDepuis[c.Id]).TotalSeconds < 20)
                Pretes.Add(c);
        }

        // Nettoyer le suivi pour les commandes qui ont change de statut en DB
        var idsEnDB = commandes.Select(c => c.Id).ToHashSet();
        foreach (var id in _pretesDepuis.Keys.Except(idsEnDB).ToList())
            SupprimerSuiviExpiration(id);
    }

    // ── Changement de statut ──────────────────────────────────────────────────
    private void ChangerStatut(Commande? commande, StatutCommande nouveau)
    {
        if (commande is null) return;
        try
        {
            // Si on retire une commande de "prete", effacer son suivi d'expiration
            if (nouveau != StatutCommande.prete && _pretesDepuis.ContainsKey(commande.Id))
                SupprimerSuiviExpiration(commande.Id);

            _dao.UpdateStatut(commande.Id, nouveau);
            Load();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ── Timer d'expiration visuelle ───────────────────────────────────────────

    /// <summary>
    /// Demarre un timer de 20 secondes. A expiration, retire la commande
    /// de la collection Pretes (vue seulement — pas de suppression en DB).
    /// </summary>
    private void DemarrerTimerExpiration(Commande commande)
    {
        if (_timersExpiration.ContainsKey(commande.Id)) return;

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(20) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            _timersExpiration.Remove(commande.Id);
            var item = Pretes.FirstOrDefault(x => x.Id == commande.Id);
            if (item != null) Pretes.Remove(item);
        };
        _timersExpiration[commande.Id] = timer;
        timer.Start();
    }

    private void SupprimerSuiviExpiration(int commandeId)
    {
        _pretesDepuis.Remove(commandeId);
        if (_timersExpiration.TryGetValue(commandeId, out var t))
        {
            t.Stop();
            _timersExpiration.Remove(commandeId);
        }
    }
}
