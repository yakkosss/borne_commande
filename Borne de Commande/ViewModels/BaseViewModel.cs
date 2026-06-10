using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Borne_de_Commande.ViewModels;

/// <summary>
/// Classe de base pour tous les ViewModels.
/// Implemente INotifyPropertyChanged pour que la vue se
/// mette a jour automatiquement via le Data Binding.
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
