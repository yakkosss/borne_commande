using System.Windows.Controls;
using Borne_de_Commande.ViewModels;

namespace Borne_de_Commande.Views;

public partial class GestionUtilisateursView : UserControl
{
    public GestionUtilisateursView()
    {
        InitializeComponent();
    }

    // PasswordBox ne supporte pas le Data Binding direct (securite WPF)
    // On transmet le mot de passe au ViewModel via l'evenement PasswordChanged.
    private void PwdBox_Changed(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is GestionUtilisateursViewModel vm)
            vm.MdpF = PwdBox.Password;
    }
}
