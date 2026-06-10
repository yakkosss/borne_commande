using System.Windows.Controls;
using Borne_de_Commande.ViewModels;

namespace Borne_de_Commande.Views;

public partial class PaiementView : UserControl
{
    public PaiementView()
    {
        InitializeComponent();
    }

    // Le seul bout de code-behind acceptable : gerer les RadioButtons
    // (WPF ne gere pas bien le binding bidirectionnel sur RadioButton IsChecked avec string)
    private void RadioButton_Checked(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is RadioButton rb && DataContext is PaiementViewModel vm)
            vm.ModePaiement = rb.Tag?.ToString() ?? string.Empty;
    }
}
