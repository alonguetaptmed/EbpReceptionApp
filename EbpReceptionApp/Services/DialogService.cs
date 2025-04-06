using EbpReceptionApp.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;

namespace EbpReceptionApp.Services
{
    public class DialogService : IDialogService
    {
        public Task<bool> ShowConfirmationAsync(string title, string message, string acceptButton = "Oui", string cancelButton = "Non")
        {
            return Device.InvokeOnMainThreadAsync(async () =>
            {
                return await Application.Current.MainPage.DisplayAlert(title, message, acceptButton, cancelButton);
            });
        }

        public Task ShowAlertAsync(string title, string message, string cancelButton = "OK")
        {
            return Device.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancelButton);
            });
        }

        public Task<string> ShowActionSheetAsync(string title, string cancelButton, string destructionButton, params string[] buttons)
        {
            return Device.InvokeOnMainThreadAsync(async () =>
            {
                return await Application.Current.MainPage.DisplayActionSheet(title, cancelButton, destructionButton, buttons);
            });
        }

        public Task<string> ShowPromptAsync(string title, string message, string initialValue = "", string acceptButton = "OK", string cancelButton = "Annuler")
        {
            return Device.InvokeOnMainThreadAsync(async () =>
            {
                return await Application.Current.MainPage.DisplayPromptAsync(title, message, acceptButton, cancelButton, initialValue);
            });
        }

        private bool _isLoading;
        private IDisposable _loadingDialog;

        public void ShowLoading(string message = "Chargement...")
        {
            if (_isLoading)
                return;

            Device.BeginInvokeOnMainThread(() =>
            {
                _loadingDialog = Application.Current.MainPage.DisplayLoading(message);
                _isLoading = true;
            });
        }

        public void HideLoading()
        {
            if (!_isLoading)
                return;

            Device.BeginInvokeOnMainThread(() =>
            {
                _loadingDialog?.Dispose();
                _isLoading = false;
            });
        }

        public void ShowToast(string message, ToastDuration duration = ToastDuration.Short)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var durationEnum = duration == ToastDuration.Short 
                    ? Xamarin.CommunityToolkit.UI.Views.ToastDuration.Short 
                    : Xamarin.CommunityToolkit.UI.Views.ToastDuration.Long;
                
                Application.Current.MainPage.DisplayToast(message, durationEnum);
            });
        }
    }
}