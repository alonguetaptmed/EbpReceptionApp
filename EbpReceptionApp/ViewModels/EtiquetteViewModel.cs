using EbpReceptionApp.Models;
using EbpReceptionApp.Services.Interfaces;
using Prism.Commands;
using Prism.Navigation;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EbpReceptionApp.ViewModels
{
    public class EtiquetteViewModel : BaseViewModel
    {
        private readonly IPrintService _printService;

        private Etiquette _etiquette;
        public Etiquette Etiquette
        {
            get => _etiquette;
            set => SetProperty(ref _etiquette, value);
        }

        public ICommand ImprimerCommand { get; }
        public ICommand FermerCommand { get; }

        public EtiquetteViewModel(INavigationService navigationService, IDialogService dialogService,
            IPrintService printService)
            : base(navigationService, dialogService)
        {
            _printService = printService;

            Title = "Détail Étiquette";

            ImprimerCommand = new DelegateCommand(async () => await ExecuteImprimerCommand());
            FermerCommand = new DelegateCommand(async () => await ExecuteFermerCommand());
        }

        private async Task ExecuteImprimerCommand()
        {
            if (Etiquette == null)
                return;

            await ExecuteCommandAsync(async () =>
            {
                var success = await _printService.ImprimerEtiquettesAsync(new System.Collections.Generic.List<Etiquette> { Etiquette });

                if (success)
                {
                    DialogService.ShowToast("Impression en cours");
                    Etiquette.EstImprimee = true;
                }
                else
                {
                    await DialogService.ShowAlertAsync("Erreur", "Impossible d'imprimer l'étiquette.");
                }
            }, true, "Impression en cours...");
        }

        private async Task ExecuteFermerCommand()
        {
            await NavigationService.GoBackAsync();
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            if (parameters.ContainsKey("Etiquette"))
            {
                Etiquette = parameters["Etiquette"] as Etiquette;
            }
        }
    }
}