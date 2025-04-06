using EbpReceptionApp.Models;
using EbpReceptionApp.Services.Interfaces;
using Prism.Commands;
using Prism.Navigation;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;

namespace EbpReceptionApp.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IPrintService _printService;
        private readonly ISessionService _sessionService;

        private string _serverUrl;
        public string ServerUrl
        {
            get => _serverUrl;
            set => SetProperty(ref _serverUrl, value);
        }

        private ObservableCollection<Printer> _imprimantes;
        public ObservableCollection<Printer> Imprimantes
        {
            get => _imprimantes;
            set => SetProperty(ref _imprimantes, value);
        }

        private Printer _selectedImprimante;
        public Printer SelectedImprimante
        {
            get => _selectedImprimante;
            set => SetProperty(ref _selectedImprimante, value);
        }

        public ICommand SaveServerUrlCommand { get; }
        public ICommand SaveImprimanteCommand { get; }
        public ICommand RefreshImprimentesCommand { get; }

        public SettingsViewModel(INavigationService navigationService, IDialogService dialogService,
            IPrintService printService, ISessionService sessionService)
            : base(navigationService, dialogService)
        {
            _printService = printService;
            _sessionService = sessionService;

            Title = "Paramètres";
            
            // Récupérer l'URL du serveur depuis les préférences
            ServerUrl = Preferences.Get("ApiBaseUrl", "https://webhook.n8n.aptmed.fr");
            
            Imprimantes = new ObservableCollection<Printer>();

            SaveServerUrlCommand = new DelegateCommand(async () => await ExecuteSaveServerUrlCommand());
            SaveImprimanteCommand = new DelegateCommand(async () => await ExecuteSaveImprimanteCommand());
            RefreshImprimentesCommand = new DelegateCommand(async () => await ExecuteRefreshImprimentesCommand());
        }

        private async Task ExecuteSaveServerUrlCommand()
        {
            if (string.IsNullOrEmpty(ServerUrl))
            {
                await DialogService.ShowAlertAsync("Erreur", "L'URL du serveur ne peut pas être vide.");
                return;
            }

            Preferences.Set("ApiBaseUrl", ServerUrl);
            await DialogService.ShowAlertAsync("Succès", "L'URL du serveur a été enregistrée. L'application doit être redémarrée pour appliquer les changements.");
        }

        private async Task ExecuteSaveImprimanteCommand()
        {
            if (SelectedImprimante == null)
            {
                await DialogService.ShowAlertAsync("Erreur", "Veuillez sélectionner une imprimante.");
                return;
            }

            await ExecuteCommandAsync(async () =>
            {
                await _printService.SetDefaultImprimanteAsync(SelectedImprimante.Id);
                DialogService.ShowToast("Imprimante par défaut enregistrée");
            });
        }

        private async Task ExecuteRefreshImprimentesCommand()
        {
            await LoadImprimantes();
        }

        private async Task LoadImprimantes()
        {
            await ExecuteCommandAsync(async () =>
            {
                var imprimantes = await _printService.GetImprimentesDisponiblesAsync();
                var imprimanteDefaut = await _printService.GetDefaultImprimanteAsync();

                Imprimantes.Clear();
                foreach (var imprimante in imprimantes)
                {
                    Imprimantes.Add(imprimante);
                }

                if (imprimanteDefaut != null)
                {
                    SelectedImprimante = Imprimantes.FirstOrDefault(i => i.Id == imprimanteDefaut.Id);
                }
            });
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            await LoadImprimantes();
        }
    }
}