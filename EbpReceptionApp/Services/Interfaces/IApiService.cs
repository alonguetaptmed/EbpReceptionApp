using EbpReceptionApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EbpReceptionApp.Services.Interfaces
{
    public interface IApiService
    {
        Task<ApiResponse<User>> LoginAsync(string username, string password);
        
        Task<ApiResponse<List<Commande>>> GetCommandesAsync(string depot = null, string statut = null, string searchQuery = null);
        
        Task<ApiResponse<Commande>> GetCommandeDetailAsync(string commandeId);
        
        Task<ApiResponse<bool>> ReceptionnerCommandeAsync(Commande commande, bool marquerSoldee = false);
        
        Task<ApiResponse<bool>> ReceptionnerLigneAsync(string commandeId, LigneCommande ligne, bool marquerSoldee = false);
        
        Task<ApiResponse<bool>> MettreAJourBlFournisseurAsync(string commandeId, string numeroBl);
        
        Task<ApiResponse<bool>> MarquerCommandeASolderAsync(string commandeId);
        
        Task<ApiResponse<bool>> CreerEtiquettesAsync(string commandeId, string ligneId, List<Etiquette> etiquettes);
        
        Task<ApiResponse<bool>> ImprimerEtiquettesAsync(List<string> etiquetteIds, string imprimanteId);
        
        Task<ApiResponse<bool>> ImprimerDocumentsAsync(string commandeId, bool imprimerCf = true, bool imprimerCm = false, string imprimanteId = null);
        
        Task<ApiResponse<List<Printer>>> GetImprimentesDisponiblesAsync();
    }
}