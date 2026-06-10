using System.Globalization;
using System.Windows.Data;

namespace Borne_de_Commande.Views;

/// <summary>
/// Convertit un booleen "ModeTableau" en libelle de bouton.
/// true  → "📋 Mode liste"
/// false → "📊 Mode tableau"
/// </summary>
public class BoolToModeConverter : IValueConverter
{
    public static readonly BoolToModeConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? "📋  Mode liste" : "📊  Mode tableau";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Convertit une string en bool (true si egale au parametre).
/// Utilise pour les RadioButtons de selection du mode de paiement.
/// </summary>
public class StringEqualityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.ToString() == parameter?.ToString();

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
