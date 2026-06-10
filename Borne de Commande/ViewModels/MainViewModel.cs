using System.Windows;
using System.Windows.Input;
using Borne_de_Commande.Services;
using Borne_de_Commande.Views;

namespace Borne_de_Commande.ViewModels;

/// <summary>
/// ViewModel racine. Gere la navigation et les roles :
///   - Non connecte  → vue client uniquement
///   - Role cuisine  → redirige vers CuisineView, section CUISINE visible
///   - Role admin    → acces complet (CUISINE + ADMINISTRATION)
/// </summary>
public class MainViewModel : BaseViewModel
{
    // ── Vue courante ──────────────────────────────────────────────────────────
    private BaseViewModel _currentViewModel = null!;
    public BaseViewModel CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    // ── Instances des ViewModels ───────────────────────────────────────────────
    public HomeViewModel                HomeVM     { get; }
    public BorneViewModel               BorneVM    { get; }
    public GestionCategoriesViewModel   CatsVM     { get; }
    public GestionProduitsViewModel     ProduitsVM { get; }
    public CuisineViewModel             CuisineVM  { get; }
    public GestionUtilisateursViewModel UsersVM    { get; }

    // ── Etat d'authentification ────────────────────────────────────────────────
    public bool   IsAuthenticated       => AuthService.IsAuthenticated;
    public bool   IsNotAuthenticated    => !AuthService.IsAuthenticated;
    public bool   IsAdmin               => AuthService.IsAdmin;
    /// <summary>Faux si connecte en tant que cuisine — bloque l'acces aux vues client.</summary>
    public bool   IsClientAccessAllowed => !AuthService.IsCuisine;
    public string UserDisplayName    => AuthService.IsAuthenticated
        ? $"[{AuthService.CurrentRole}]  {AuthService.CurrentUserName}"
        : "Non connecte";

    // ── Commandes de navigation ────────────────────────────────────────────────
    public ICommand NavigateHomeCommand       { get; }
    public ICommand NavigateBorneCommand      { get; }
    public ICommand NavigateCategoriesCommand { get; }
    public ICommand NavigateProduitsCommand   { get; }
    public ICommand NavigateCuisineCommand    { get; }
    public ICommand NavigateUsersCommand      { get; }
    public ICommand LogoutCommand             { get; }
    public ICommand ConnexionCommand          { get; }

    public MainViewModel()
    {
        HomeVM     = new HomeViewModel(this);
        BorneVM    = new BorneViewModel(this);
        CatsVM     = new GestionCategoriesViewModel();
        ProduitsVM = new GestionProduitsViewModel();
        CuisineVM  = new CuisineViewModel();
        UsersVM    = new GestionUtilisateursViewModel();

        // Navigation libre (client) — bloquee pour le role cuisine
        NavigateHomeCommand = new RelayCommand(() =>
        {
            if (AuthService.IsCuisine) return;

            bool commandeEnCours =
                (CurrentViewModel == BorneVM    && BorneVM.Panier.Count > 0) ||
                (CurrentViewModel is PaiementViewModel);

            if (commandeEnCours)
            {
                var dlg = new Views.ConfirmationDialog(
                    "Commande en cours",
                    "Vous avez une commande en cours.\nVoulez-vous l'abandonner ?")
                    { Owner = Application.Current.MainWindow };
                if (dlg.ShowDialog() != true) return;
            }
            HomeVM.Load();
            CurrentViewModel = HomeVM;
        });

        NavigateBorneCommand = new RelayCommand(() =>
        {
            if (AuthService.IsCuisine) return;
            BorneVM.ResetCommande();
            CurrentViewModel = BorneVM;
        });

        // Navigation cuisine (acces : admin OU cuisine)
        NavigateCuisineCommand = new RelayCommand(() =>
            NavigateWithAuth(() => { CuisineVM.Load(); CurrentViewModel = CuisineVM; }, adminOnly: false));

        // Navigation administration (acces : admin uniquement)
        NavigateCategoriesCommand = new RelayCommand(() =>
            NavigateWithAuth(() => { CatsVM.Load();     CurrentViewModel = CatsVM; }, adminOnly: true));
        NavigateProduitsCommand   = new RelayCommand(() =>
            NavigateWithAuth(() => { ProduitsVM.Load(); CurrentViewModel = ProduitsVM; }, adminOnly: true));
        NavigateUsersCommand      = new RelayCommand(() =>
            NavigateWithAuth(() => { UsersVM.Load();    CurrentViewModel = UsersVM; }, adminOnly: true));

        // Deconnexion
        LogoutCommand = new RelayCommand(() =>
        {
            AuthService.Logout();
            RefreshAuth();
            HomeVM.Load();
            CurrentViewModel = HomeVM;
        });

        // Connexion explicite depuis la sidebar
        ConnexionCommand = new RelayCommand(() =>
        {
            var login = new LoginWindow { Owner = Application.Current.MainWindow };
            if (login.ShowDialog() == true)
            {
                RefreshAuth();
                RouteAfterLogin();
            }
        });

        HomeVM.Load();
        CurrentViewModel = HomeVM;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Ouvre la fenetre de login si necessaire, puis execute l'action.
    /// adminOnly = true  → refuse l'acces au role cuisine.
    /// adminOnly = false → accepte admin ET cuisine.
    /// </summary>
    private void NavigateWithAuth(Action action, bool adminOnly)
    {
        bool ok = adminOnly ? AuthService.IsAdmin : AuthService.IsAuthenticated;
        if (ok) { action(); return; }

        var login = new LoginWindow { Owner = Application.Current.MainWindow };
        if (login.ShowDialog() != true) return;

        RefreshAuth();
        RouteAfterLogin();

        bool nowOk = adminOnly ? AuthService.IsAdmin : AuthService.IsAuthenticated;
        if (nowOk)
            action();
        else if (AuthService.IsAuthenticated && adminOnly)
            MessageBox.Show(
                "Acces reserve aux administrateurs.",
                "Acces refuse",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
    }

    /// <summary>
    /// Apres connexion : redirige automatiquement vers la vue
    /// correspondant au role (cuisine → CuisineView).
    /// </summary>
    private void RouteAfterLogin()
    {
        if (AuthService.IsCuisine)
        {
            CuisineVM.Load();
            CurrentViewModel = CuisineVM;
        }
        // Admin : reste sur la vue courante, les sections apparaissent dans la sidebar
    }

    private void RefreshAuth()
    {
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(IsNotAuthenticated));
        OnPropertyChanged(nameof(IsAdmin));
        OnPropertyChanged(nameof(IsClientAccessAllowed));
        OnPropertyChanged(nameof(UserDisplayName));
    }

    // ── Navigation paiement ───────────────────────────────────────────────────
    public void NavigateToPaiement(PaiementViewModel vm) => CurrentViewModel = vm;

    public void RetourBorne()
    {
        BorneVM.ResetCommande();
        CurrentViewModel = BorneVM;
    }

    public void RetourHome()
    {
        HomeVM.Load();
        CurrentViewModel = HomeVM;
    }
}
