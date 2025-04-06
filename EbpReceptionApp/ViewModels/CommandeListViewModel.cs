using EbpReceptionApp.Models;
using EbpReceptionApp.Services.Interfaces;
using Prism.Commands;
using Prism.Navigation;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace EbpReceptionApp.ViewModels
{
    public class CommandeListViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly ISessionService _sessionService;
        private readonly IPrintService _printService;

        private ObservableCollection<Commande> _commandes;
        public ObservableCollection<Commande> Commandes
        {
            get => _commandes;
            set => SetProperty(ref _commandes, value);
        }

        private ObservableCollection<Commande> _commandesFiltered;
        public ObservableCollection<Commande> CommandesFiltered
        {
            get => _commandesFiltered;
            set => SetProperty(ref _commandesFiltered, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        private string _selectedStatutFilter = "Toutes";
        public string SelectedStatutFilter
        {
            get => _selectedStatutFilter;
            set
            {
                SetProperty(ref _selectedStatutFilter, value);
                ApplyFilter();
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        private bool _areCommandesSelected;
        public bool AreCommandesSelected
        {
            get => _areCommandesSelected;
            set => SetProperty(ref _areCommandesSelected, value);
        }

        private string _depot;
        public string Depot
        {
            get => _depot;
            set => SetProperty(ref _depot, value);
        }

        public ObservableCollection<string> StatutFilters { get; } = new ObservableCollection<string>
        {
            "Toutes",
            "Non réceptionnées",
            "Réceptionnées partiellement",
            "En cours de réception",
            "N'est pas en cours de réception",
            "Soldées"
        };

        public ICommand RefreshCommand { get; }
        public ICommand SelectCommandeCommand { get; }
        public ICommand OpenCommandeCommand { get; }
        public ICommand ImprimerDocumentsCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand SettingsCommand { get; }

        public CommandeListViewModel(INavigationService navigationService, IDialogService dialogService,
            IApiService apiService, ISessionService sessionService, IPrintService printService)
            : base(navigationService, dialogService)
        {
            _apiService = apiService;
            _sessionService = sessionService;
            _printService = printService;

            Title = "Commandes fournisseurs";
            Depot = _sessionService.GetDepot();

            Commandes = new ObservableCollection<Commande>();
            CommandesFiltered = new ObservableCollection<Commande>();

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            SelectCommandeCommand = new DelegateCommand<Commande>(ExecuteSelectCommandeCommand);
            OpenCommandeCommand = new DelegateCommand<Commande>(async (commande) => await ExecuteOpenCommandeCommand(commande));
            ImprimerDocumentsCommand = new DelegateCommand(async () => await ExecuteImprimerDocumentsCommand());
            LogoutCommand = new DelegateCommand(async () => await ExecuteLogoutCommand());
            SettingsCommand = new DelegateCommand(async () => await ExecuteSettingsCommand());
        }

        private async Task ExecuteRefreshCommand()
        {
            IsRefreshing = true;

            await LoadCommandes();

            IsRefreshing = false;
        }

        private void ExecuteSelectCommandeCommand(Commande commande)
        {
            if (commande == null)
                return;

            commande.EstEnCoursReception = !commande.EstEnCoursReception;
            AreCommandesSelected = Commandes.Any(c => c.EstEnCoursReception);
        }

        private async Task ExecuteOpenCommandeCommand(Commande commande)
        {
            if (commande == null)
                return;

            var parameters = new NavigationParameters
            {
                { "CommandeId", commande.Id }
            };

            await NavigationService.NavigateAsync("CommandeDetailPage", parameters);
        }

        private async Task ExecuteImprimerDocumentsCommand()
        {
            var selectedCommandes = Commandes.Where(c => c.EstEnCoursReception).ToList();
            if (!selectedCommandes.Any())
            {
                await DialogService.ShowAlertAsync("Information", "Veuillez sélectionner au moins une commande.");
                return;
            }

            var options = new string[] { "Imprimer CF", "Imprimer CF et CM" };
            var result = await DialogService.ShowActionSheetAsync("Options d'impression", "Annuler", null, options);

            if (result == "Imprimer CF" || result == "Imprimer CF et CM")
            {
                bool imprimerCm = result == "Imprimer CF et CM";

                await ExecuteCommandAsync(async () =>
                {
                    foreach (var commande in selectedCommandes)
                    {
                        await _apiService.ImprimerDocumentsAsync(commande.Id, true, imprimerCm);
                    }

                    DialogService.ShowToast($"Impression des documents en cours pour {selectedCommandes.Count} commande(s)");
                }, true, "Impression en cours...");
            }
        }

        private async Task ExecuteLogoutCommand()
        {
            var confirm = await DialogService.ShowConfirmationAsync("Déconnexion", "Êtes-vous sûr de vouloir vous déconnecter ?");
            if (confirm)
            {
                _sessionService.Logout();
                await NavigationService.NavigateAsync("/NavigationPage/LoginPage");
            }
        }

        private async Task ExecuteSettingsCommand()
        {
            await NavigationService.NavigateAsync("SettingsPage");
        }

        private async Task LoadCommandes()
        {
            await ExecuteCommandAsync(async () =>
            {
                var response = await _apiService.GetCommandesAsync(_sessionService.GetDepot());

                if (response.Success && response.Data != null)
                {
                    Commandes.Clear();
                    foreach (var commande in response.Data)
                    {
                        Commandes.Add(commande);
                    }

                    ApplyFilter();
                }
                else
                {
                    await DialogService.ShowAlertAsync("Erreur", response.Message);
                }
            }, !IsRefreshing);
        }

        private void ApplyFilter()
        {
            var filteredCommandes = Commandes.ToList();

            // Appliquer filtre de statut
            switch (SelectedStatutFilter)
            {
                case "Non réceptionnées":
                    filteredCommandes = filteredCommandes.Where(c => c.Statut == StatutCommande.NonReceptionnee).ToList();
                    break;
                case "Réceptionnées partiellement":
                    filteredCommandes = filteredCommandes.Where(c => c.Statut == StatutCommande.ReceptionneePartiellement).ToList();
                    break;
                case "En cours de réception":
                    filteredCommandes = filteredCommandes.Where(c => c.EstEnCoursReception).ToList();
                    break;
                case "N'est pas en cours de réception":
                    filteredCommandes = filteredCommandes.Where(c => !c.EstEnCoursReception).ToList();
                    break;
                case "Soldées":
                    filteredCommandes = filteredCommandes.Where(c => c.Statut == StatutCommande.Soldee).ToList();
                    break;
            }

            // Appliquer filtre de recherche
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchText = SearchText.ToLower();
                filteredCommandes = filteredCommandes.Where(c =>
                    c.NumeroCommande?.ToLower().Contains(searchText) == true ||
                    c.Fournisseur?.ToLower().Contains(searchText) == true ||
                    c.DateCommande.ToString("dd/MM/yyyy").Contains(searchText) ||
                    c.NumeroBL?.ToLower().Contains(searchText) == true
                ).ToList();
            }

            CommandesFiltered = new ObservableCollection<Commande>(filteredCommandes);
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            // Vérifier si l'utilisateur est connecté
            if (!_sessionService.IsAuthenticated)
            {
                await NavigationService.NavigateAsync("/NavigationPage/LoginPage");
                return;
            }

            if (parameters.ContainsKey("Refresh") && (bool)parameters["Refresh"])
            {
                await LoadCommandes();
            }
            else if (Commandes.Count == 0)
            {
                await LoadCommandes();
            }
        }
    }
}