using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Borne_de_Commande.DAL;
using Borne_de_Commande.Models;
using Borne_de_Commande.Views;

namespace Borne_de_Commande.ViewModels;

/// <summary>
/// Gere le paiement : recapitulatif → mode de paiement → confirmation → tickets.
/// </summary>
public class PaiementViewModel : BaseViewModel
{
    private readonly MainViewModel _main;
    private readonly CommandeDAO   _dao   = new();
    private readonly Client?       _client;

    // ── Recapitulatif ─────────────────────────────────────────────────────────
    public ObservableCollection<PanierItem> Items { get; }
    public decimal Total => Items.Sum(i => i.SousTotal);

    public string InfoClient => _client is null
        ? "Client invite"
        : _client.TypeClient == TypeClient.inscrit
            ? $"{_client.Pseudo} ({_client.NumeroClient})"
            : $"Invite {_client.NumeroClient}";

    // ── Mode de paiement (3 modes selon exigences) ────────────────────────────
    private string _modePaiement = "Carte bancaire";
    public string ModePaiement
    {
        get => _modePaiement;
        set => SetProperty(ref _modePaiement, value);
    }

    public List<string> ModesPaiement { get; } = ["Carte bancaire", "Especes", "Ticket restaurant"];

    // ── Commandes ─────────────────────────────────────────────────────────────
    public ICommand ConfirmerCommand { get; }
    public ICommand AnnulerCommand   { get; }

    public PaiementViewModel(MainViewModel main, List<PanierItem> items, Client? client = null)
    {
        _main   = main;
        _client = client;
        Items   = new ObservableCollection<PanierItem>(items);

        ConfirmerCommand = new RelayCommand(Confirmer);
        AnnulerCommand   = new RelayCommand(() => _main.RetourBorne());
    }

    // ── Confirmation ─────────────────────────────────────────────────────────
    private void Confirmer()
    {
        try
        {
            var commande = new Commande
            {
                NumeroCommande = Commande.GenererNumero(),
                ClientId       = _client?.IdClient,
                Client         = _client,
                DateCreation   = DateTime.Now,
                Statut         = StatutCommande.en_attente,
                ModePaiement   = ModePaiement,
                Total          = Total,
                Lignes         = Items.Select(i => new LigneCommande
                {
                    ProduitId    = i.Produit.Id,
                    NomProduit   = i.Produit.Nom,
                    PrixUnitaire = i.Produit.Prix,
                    Quantite     = i.Quantite
                }).ToList()
            };

            _dao.Insert(commande);

            // Ticket client uniquement (le ticket cuisine est visible depuis la vue cuisine)
            var ticketClient = new TicketWindow(commande, TicketType.Client);
            ticketClient.ShowDialog();

            _main.RetourBorne();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
