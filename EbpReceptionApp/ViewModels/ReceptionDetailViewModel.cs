using EbpReceptionApp.Models;
using EbpReceptionApp.Services.Interfaces;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EbpReceptionApp.ViewModels
{
    public class ReceptionDetailViewModel : BaseViewModel
    {
        private readonly IApiService _apiService;
        private readonly IPrintService _printService;

        private string _commandeId;
        private string _ligneId;

        private Commande _commande;
        public Commande Commande
        {
            get => _commande;
            set => SetProperty(ref _commande, value);
        }

        private LigneCommande _ligne;
        public LigneCommande Ligne
        {
            get => _ligne;
            set => SetProperty(ref _ligne, value);
        }

        private decimal _quantiteCommandee;
        public decimal QuantiteCommandee
        {
            get => _quantiteCommandee;
            set => SetProperty(ref _quantiteCommandee, value);
        }

        private decimal _quantiteReliquat;
        public decimal QuantiteReliquat
        {
            get => _quantiteReliquat;
            set => SetProperty(ref _quantiteReliquat, value);
        }

        private decimal _quantiteEnCoursReception;
        public decimal QuantiteEnCoursReception
        {
            get => _quantiteEnCoursReception;
            set
            {
                SetProperty(ref _quantiteEnCoursReception, value);
                RaisePropertyChanged(nameof(CanValidate));
            }
        }

        private ObservableCollection<Etiquette> _etiquettes;
        public ObservableCollection<Etiquette> Etiquettes
        {
            get => _etiquettes;
            set => SetProperty(ref _etiquettes, value);
        }

        private bool _marquerASolder;
        public bool MarquerASolder
        {
            get => _marquerASolder;
            set => SetProperty(ref _marquerASolder, value);
        }

        public bool CanValidate => QuantiteEnCoursReception > 0 && QuantiteEnCoursReception <= QuantiteReliquat;

        public ICommand AjouterEtiquetteCommand { get; }
        public ICommand SupprimerEtiquetteCommand { get; }
        public ICommand ImprimerEtiquettesCommand { get; }
        public ICommand ValiderReceptionCommand { get; }

        public ReceptionDetailViewModel(INavigationService navigationService, IDialogService dialogService,
            IApiService apiService, IPrintService printService)
            : base(navigationService, dialogService)
        {
            _apiService = apiService;
            _printService = printService;

            Title = "Réception détaillée";
            Etiquettes = new ObservableCollection<Etiquette>();

            AjouterEtiquetteCommand = new DelegateCommand(async () => await ExecuteAjouterEtiquetteCommand());
            SupprimerEtiquetteCommand = new DelegateCommand<Etiquette>(ExecuteSupprimerEtiquetteCommand);
            ImprimerEtiquettesCommand = new DelegateCommand(async () => await ExecuteImprimerEtiquettesCommand());
            ValiderReceptionCommand = new DelegateCommand(async () => await ExecuteValiderReceptionCommand(), () => CanValidate)
                .ObservesProperty(() => QuantiteEnCoursReception);
        }

        private async Task ExecuteAjouterEtiquetteCommand()
        {
            var metresLineaires = await DialogService.ShowPromptAsync("Mètres linéaires", "Entrez le nombre de mètres linéaires:", "0");
            if (string.IsNullOrEmpty(metresLineaires) || !decimal.TryParse(metresLineaires, out decimal ml) || ml <= 0)
            {
                await DialogService.ShowAlertAsync("Erreur", "Veuillez saisir un nombre valide de mètres linéaires.");
                return;
            }

            var nbRouleaux = await DialogService.ShowPromptAsync("Nombre de rouleaux", "Entrez le nombre de rouleaux:", "1");
            if (string.IsNullOrEmpty(nbRouleaux) || !int.TryParse(nbRouleaux, out int nb) || nb <= 0)
            {
                await DialogService.ShowAlertAsync("Erreur", "Veuillez saisir un nombre valide de rouleaux.");
                return;
            }

            var etiquette = new Etiquette
            {
                Id = Guid.NewGuid().ToString(),
                IdLigneCommande = Ligne.Id,
                NumeroEtiquette = $"ETQ-{DateTime.Now:yyyyMMdd}-{Etiquettes.Count + 1}",
                NombreMetresLineaires = ml,
                NombreRouleaux = nb,
                DateCreation = DateTime.Now
            };

            Etiquettes.Add(etiquette);
            CalculerQuantiteReceptionnee();
        }

        private void ExecuteSupprimerEtiquetteCommand(Etiquette etiquette)
        {
            if (etiquette == null)
                return;

            Etiquettes.Remove(etiquette);
            CalculerQuantiteReceptionnee();
        }

        private async Task ExecuteImprimerEtiquettesCommand()
        {
            if (Etiquettes.Count == 0)
            {
                await DialogService.ShowAlertAsync("Information", "Aucune étiquette à imprimer.");
                return;
            }

            await ExecuteCommandAsync(async () =>
            {
                // Créer les étiquettes côté serveur d'abord
                var response = await _apiService.CreerEtiquettesAsync(_commandeId, _ligneId, Etiquettes.ToList());
                if (!response.Success)
                {
                    await DialogService.ShowAlertAsync("Erreur", response.Message);
                    return;
                }

                // Puis imprimer
                var success = await _printService.ImprimerEtiquettesAsync(Etiquettes.ToList());
                if (success)
                {
                    DialogService.ShowToast("Impression des étiquettes en cours");
                    foreach (var etiquette in Etiquettes)
                    {
                        etiquette.EstImprimee = true;
                    }
                }
                else
                {
                    await DialogService.ShowAlertAsync("Erreur", "Impossible d'imprimer les étiquettes.");
                }
            }, true, "Impression en cours...");
        }

        private async Task ExecuteValiderReceptionCommand()
        {
            if (!CanValidate)
            {
                await DialogService.ShowAlertAsync("Erreur", "La quantité à réceptionner n'est pas valide.");
                return;
            }

            await ExecuteCommandAsync(async () =>
            {
                // Mettre à jour la ligne
                Ligne.QuantiteEnCoursReception = QuantiteEnCoursReception;
                Ligne.Etiquettes = Etiquettes;
                
                var response = await _apiService.ReceptionnerLigneAsync(_commandeId, Ligne, MarquerASolder);

                if (response.Success)
                {
                    // Option pour imprimer le bon de réception
                    var imprimerBR = await DialogService.ShowConfirmationAsync("Impression", "Voulez-vous imprimer le bon de réception ?");
                    if (imprimerBR)
                    {
                        await _printService.ImprimerBonReceptionAsync(_commandeId);
                    }

                    // Revenir à la page précédente
                    var navParams = new NavigationParameters
                    {
                        { "Refresh", true }
                    };
                    await NavigationService.GoBackAsync(navParams);
                }
                else
                {
                    await DialogService.ShowAlertAsync("Erreur", response.Message);
                }
            }, true, "Validation en cours...");
        }

        private void CalculerQuantiteReceptionnee()
        {
            decimal totalMetresLineaires = 0;

            foreach (var etiquette in Etiquettes)
            {
                totalMetresLineaires += etiquette.NombreMetresLineaires * etiquette.NombreRouleaux;
            }

            QuantiteEnCoursReception = totalMetresLineaires;
        }

        private async Task LoadLigne()
        {
            if (string.IsNullOrEmpty(_commandeId) || string.IsNullOrEmpty(_ligneId))
                return;

            await ExecuteCommandAsync(async () =>
            {
                var response = await _apiService.GetCommandeDetailAsync(_commandeId);

                if (response.Success && response.Data != null)
                {
                    Commande = response.Data;
                    Ligne = Commande.Lignes.FirstOrDefault(l => l.Id == _ligneId);

                    if (Ligne == null)
                    {
                        await DialogService.ShowAlertAsync("Erreur", "Ligne de commande non trouvée.");
                        await NavigationService.GoBackAsync();
                        return;
                    }

                    QuantiteCommandee = Ligne.QuantiteCommandee;
                    QuantiteReliquat = Ligne.QuantiteReliquat;
                    QuantiteEnCoursReception = Ligne.QuantiteReliquat;

                    // Récupérer les étiquettes existantes
                    if (Ligne.Etiquettes != null && Ligne.Etiquettes.Any())
                    {
                        Etiquettes = new ObservableCollection<Etiquette>(Ligne.Etiquettes);
                    }
                }
                else
                {
                    await DialogService.ShowAlertAsync("Erreur", response.Message);
                    await NavigationService.GoBackAsync();
                }
            });
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            if (parameters.ContainsKey("CommandeId") && parameters.ContainsKey("LigneId"))
            {
                _commandeId = parameters["CommandeId"] as string;
                _ligneId = parameters["LigneId"] as string;
                await LoadLigne();
            }
            else
            {
                await DialogService.ShowAlertAsync("Erreur", "Paramètres manquants.");
                await NavigationService.GoBackAsync();
            }
        }
    }
}