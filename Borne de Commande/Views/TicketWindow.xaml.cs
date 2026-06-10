using System.Windows;
using System.Windows.Media;
using Borne_de_Commande.Models;

namespace Borne_de_Commande.Views;

public enum TicketType { Client, Cuisine }

/// <summary>
/// Ticket client : recapitulatif complet avec prix et mode de paiement.
/// Ticket cuisine : liste des articles uniquement (pas de prix ni paiement).
/// </summary>
public partial class TicketWindow : Window
{
    private readonly TicketType _type;

    public TicketWindow(Commande commande, TicketType type = TicketType.Client)
    {
        InitializeComponent();
        _type       = type;
        DataContext = commande;
        ConfigurerType();
    }

    private void ConfigurerType()
    {
        if (_type == TicketType.Cuisine)
        {
            TitreTicket.Text          = "🍳  TICKET CUISINE";
            HeaderBorder.Background   = new SolidColorBrush(Color.FromRgb(0xE9, 0x45, 0x60));
            PaiementGrid.Visibility   = Visibility.Collapsed;
            TotalGrid.Visibility      = Visibility.Collapsed;
            Title                     = "Ticket Cuisine";
        }
        else
        {
            TitreTicket.Text          = "🧾  TICKET CLIENT";
            HeaderBorder.Background   = new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x71));
            Title                     = "Ticket Client";
        }
    }

    private void FermerClick(object sender, RoutedEventArgs e) => Close();
}
