using Prism.Mvvm;
using Prism.Navigation;
using System.Threading.Tasks;
using EbpReceptionApp.Services.Interfaces;
using System.Windows.Input;
using Xamarin.Forms;
using System;

namespace EbpReceptionApp.ViewModels
{
    public class BaseViewModel : BindableBase, INavigationAware, IDestructible
    {
        protected INavigationService NavigationService { get; }
        protected IDialogService DialogService { get; }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public BaseViewModel(INavigationService navigationService, IDialogService dialogService)
        {
            NavigationService = navigationService;
            DialogService = dialogService;
        }

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public virtual void Destroy()
        {
        }

        protected async Task ExecuteCommandAsync(Func<Task> action, bool showLoading = true, string loadingMessage = "Chargement...")
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                if (showLoading)
                    DialogService.ShowLoading(loadingMessage);

                await action();
            }
            catch (Exception ex)
            {
                await DialogService.ShowAlertAsync("Erreur", $"Une erreur est survenue: {ex.Message}");
            }
            finally
            {
                if (showLoading)
                    DialogService.HideLoading();

                IsBusy = false;
            }
        }
    }
}