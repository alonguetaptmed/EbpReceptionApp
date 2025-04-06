using EbpReceptionApp.Services.Interfaces;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;

namespace EbpReceptionApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly ISessionService _sessionService;

        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool _saveCredentials;
        public bool SaveCredentials
        {
            get => _saveCredentials;
            set => SetProperty(ref _saveCredentials, value);
        }

        private string _serverUrl;
        public string ServerUrl
        {
            get => _serverUrl;
            set => SetProperty(ref _serverUrl, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand SaveServerUrlCommand { get; }

        public LoginViewModel(INavigationService navigationService, IDialogService dialogService,
            IApiService apiService, ISessionService sessionService)
            : base(navigationService, dialogService)
        {
            _apiService = apiService;
            _sessionService = sessionService;

            Title = "Connexion";
            ServerUrl = Preferences.Get("ApiBaseUrl", "https://webhook.n8n.aptmed.fr");
            SaveCredentials = Preferences.Get("SaveCredentials", false);

            if (SaveCredentials)
            {
                Username = Preferences.Get("Username", string.Empty);
                Password = Preferences.Get("Password", string.Empty);
            }

            LoginCommand = new DelegateCommand(async () => await ExecuteLoginCommand());
            SaveServerUrlCommand = new DelegateCommand(async () => await ExecuteSaveServerUrlCommand());
        }

        private async Task ExecuteLoginCommand()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                await DialogService.ShowAlertAsync("Erreur", "Veuillez saisir votre nom d'utilisateur et votre mot de passe.");
                return;
            }

            await ExecuteCommandAsync(async () =>
            {
                var response = await _apiService.LoginAsync(Username, Password);

                if (response.Success && response.Data != null)
                {
                    // Sauvegarder les identifiants si demandé
                    if (SaveCredentials)
                    {
                        Preferences.Set("Username", Username);
                        Preferences.Set("Password", Password);
                        Preferences.Set("SaveCredentials", true);
                    }
                    else
                    {
                        Preferences.Remove("Username");
                        Preferences.Remove("Password");
                        Preferences.Set("SaveCredentials", false);
                    }

                    // Enregistrer l'utilisateur dans la session
                    _sessionService.SetCurrentUser(response.Data);

                    // Naviguer vers la page principale
                    await NavigationService.NavigateAsync("/NavigationPage/CommandeListPage");
                }
                else
                {
                    await DialogService.ShowAlertAsync("Erreur de connexion", response.Message);
                }
            }, true, "Connexion en cours...");
        }

        private async Task ExecuteSaveServerUrlCommand()
        {
            if (string.IsNullOrEmpty(ServerUrl))
            {
                await DialogService.ShowAlertAsync("Erreur", "L'URL du serveur ne peut pas être vide.");
                return;
            }

            Preferences.Set("ApiBaseUrl", ServerUrl);
            await DialogService.ShowAlertAsync("Succès", "L'URL du serveur a été enregistrée.");
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            // Si l'utilisateur est déjà connecté, naviguer vers la page principale
            if (_sessionService.IsAuthenticated)
            {
                NavigationService.NavigateAsync("/NavigationPage/CommandeListPage");
            }
        }
    }
}