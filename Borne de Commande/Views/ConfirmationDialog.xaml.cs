using System.Windows;

namespace Borne_de_Commande.Views;

public partial class ConfirmationDialog : Window
{
    public ConfirmationDialog(string title, string message)
    {
        InitializeComponent();
        TitleText.Text   = title;
        MessageText.Text = message;
    }

    private void BtnOui_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void BtnNon_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
