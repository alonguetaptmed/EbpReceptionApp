using EbpReceptionApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EbpReceptionApp.Services.Interfaces
{
    public interface IPrintService
    {
        Task<bool> ImprimerEtiquettesAsync(List<Etiquette> etiquettes, string imprimanteId = null);
        
        Task<bool> ImprimerBonReceptionAsync(string commandeId, string imprimanteId = null);
        
        Task<bool> ImprimerCommandeFournisseurAsync(string commandeId, string imprimanteId = null);
        
        Task<bool> ImprimerCommandeContremarqueAsync(string commandeId, string ligneId, string imprimanteId = null);
        
        Task<List<Printer>> GetImprimentesDisponiblesAsync();
        
        Task<Printer> GetDefaultImprimanteAsync();
        
        Task SetDefaultImprimanteAsync(string imprimanteId);
    }
}