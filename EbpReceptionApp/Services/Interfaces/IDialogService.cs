using System;
using System.Threading.Tasks;

namespace EbpReceptionApp.Services.Interfaces
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationAsync(string title, string message, string acceptButton = "Oui", string cancelButton = "Non");
        
        Task ShowAlertAsync(string title, string message, string cancelButton = "OK");
        
        Task<string> ShowActionSheetAsync(string title, string cancelButton, string destructionButton, params string[] buttons);
        
        Task<string> ShowPromptAsync(string title, string message, string initialValue = "", string acceptButton = "OK", string cancelButton = "Annuler");
        
        void ShowLoading(string message = "Chargement...");
        
        void HideLoading();
        
        void ShowToast(string message, ToastDuration duration = ToastDuration.Short);
    }
    
    public enum ToastDuration
    {
        Short,
        Long
    }
}