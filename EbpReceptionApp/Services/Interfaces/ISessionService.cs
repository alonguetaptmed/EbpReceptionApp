using EbpReceptionApp.Models;

namespace EbpReceptionApp.Services.Interfaces
{
    public interface ISessionService
    {
        User CurrentUser { get; }
        
        bool IsAuthenticated { get; }
        
        void SetCurrentUser(User user);
        
        void Logout();
        
        string GetToken();
        
        string GetDepot();
        
        string GetImprimanteId();
    }
}