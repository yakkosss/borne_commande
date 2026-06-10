using System.Windows;
using System.Windows.Controls;
using Borne_de_Commande.Models;
using Borne_de_Commande.Services;

namespace Borne_de_Commande.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => PwdBox.Focus();
    }

    private void BtnConnexion_Click(object sender, RoutedEventArgs e)
    {
        var pwd = PwdBox.Password;

        if (string.IsNullOrWhiteSpace(pwd))
        {
            ShowError("Veuillez saisir un mot de passe.");
            return;
        }

        var tag  = (RoleBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "admin";
        var role = tag == "cuisine" ? UserRole.cuisine : UserRole.admin;

        if (AuthService.Login(role, pwd))
        {
            DialogResult = true;
            Close();
        }
        else
        {
            ShowError("Mot de passe incorrect.");
            PwdBox.Clear();
            PwdBox.Focus();
        }
    }

    private void ShowError(string msg)
    {
        ErrorText.Text       = msg;
        ErrorText.Visibility = Visibility.Visible;
    }
}
