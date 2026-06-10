using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Borne_de_Commande.DAL;
using Borne_de_Commande.Models;

namespace Borne_de_Commande.ViewModels;

/// <summary>
/// Interface admin : gestion des utilisateurs (clients invites + inscrits).
/// </summary>
public class GestionUtilisateursViewModel : BaseViewModel
{
    private readonly ClientDAO _dao = new();

    // ── Liste ─────────────────────────────────────────────────────────────────
    public ObservableCollection<Client> Clients { get; } = [];

    private Client? _selected;
    public Client? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    // ── Filtre ────────────────────────────────────────────────────────────────
    private string _filtre = string.Empty;
    public string Filtre
    {
        get => _filtre;
        set { SetProperty(ref _filtre, value); AppliquerFiltre(); }
    }

    private List<Client> _tousClients = [];

    // ── Formulaire inscription ────────────────────────────────────────────────
    private string _nomF      = string.Empty;
    private string _prenomF   = string.Empty;
    private string _emailF    = string.Empty;
    private string _mdpF      = string.Empty;
    private string _telF      = string.Empty;
    private string _adresseF  = string.Empty;
    private string _pseudoF   = string.Empty;

    public string NomF      { get => _nomF;     set => SetProperty(ref _nomF, value); }
    public string PrenomF   { get => _prenomF;  set => SetProperty(ref _prenomF, value); }
    public string EmailF    { get => _emailF;   set => SetProperty(ref _emailF, value); }
    public string MdpF      { get => _mdpF;     set => SetProperty(ref _mdpF, value); }
    public string TelF      { get => _telF;     set => SetProperty(ref _telF, value); }
    public string AdresseF  { get => _adresseF; set => SetProperty(ref _adresseF, value); }
    public string PseudoF   { get => _pseudoF;  set => SetProperty(ref _pseudoF, value); }

    // ── Commandes ─────────────────────────────────────────────────────────────
    public ICommand InscrireCommand  { get; }
    public ICommand SupprimerCommand { get; }
    public ICommand NouveauCommand   { get; }

    public GestionUtilisateursViewModel()
    {
        InscrireCommand  = new RelayCommand(Inscrire,
            () => !string.IsNullOrWhiteSpace(NomF) && !string.IsNullOrWhiteSpace(EmailF) && !string.IsNullOrWhiteSpace(MdpF));
        SupprimerCommand = new RelayCommand(Supprimer, () => Selected is not null);
        NouveauCommand   = new RelayCommand(Nouveau);
    }

    // ── Chargement ────────────────────────────────────────────────────────────
    public void Load()
    {
        try
        {
            _tousClients = _dao.GetAll();
            AppliquerFiltre();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void AppliquerFiltre()
    {
        Clients.Clear();
        var filtre = Filtre.Trim().ToLower();
        foreach (var c in _tousClients)
        {
            if (string.IsNullOrEmpty(filtre) ||
                c.Pseudo.ToLower().Contains(filtre) ||
                c.NumeroClient.ToLower().Contains(filtre) ||
                (c.Inscrit?.Email.ToLower().Contains(filtre) ?? false))
                Clients.Add(c);
        }
    }

    // ── Actions ───────────────────────────────────────────────────────────────
    private void Nouveau()
    {
        Selected = null;
        NomF = PrenomF = EmailF = MdpF = TelF = AdresseF = PseudoF = string.Empty;
    }

    private void Inscrire()
    {
        try
        {
            var inscrit = new ClientInscrit
            {
                Nom       = NomF.Trim(),
                Prenom    = PrenomF.Trim(),
                Email     = EmailF.Trim(),
                MotDePasse = MdpF,
                Telephone = TelF.Trim(),
                Adresse   = AdresseF.Trim()
            };
            string pseudo = string.IsNullOrWhiteSpace(PseudoF)
                ? $"{PrenomF.Trim()}{NomF.Trim()[0]}"
                : PseudoF.Trim();
            _dao.InsererInscrit(inscrit, pseudo);
            Load();
            Nouveau();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void Supprimer()
    {
        if (Selected is null) return;
        var r = MessageBox.Show(
            $"Supprimer le client « {Selected.Pseudo} » ?",
            "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (r != MessageBoxResult.Yes) return;
        try { _dao.Delete(Selected.IdClient); Load(); Nouveau(); }
        catch (Exception ex) { ShowError(ex); }
    }

    private static void ShowError(Exception ex)
        => MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
}
