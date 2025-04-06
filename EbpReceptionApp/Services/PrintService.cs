using EbpReceptionApp.Models;
using EbpReceptionApp.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace EbpReceptionApp.Services
{
    public class PrintService : IPrintService
    {
        private readonly IApiService _apiService;
        private readonly ISessionService _sessionService;

        public PrintService(IApiService apiService, ISessionService sessionService)
        {
            _apiService = apiService;
            _sessionService = sessionService;
        }

        public async Task<bool> ImprimerEtiquettesAsync(List<Etiquette> etiquettes, string imprimanteId = null)
        {
            if (etiquettes == null || !etiquettes.Any())
                return false;

            var etiquetteIds = etiquettes.Select(e => e.Id).ToList();
            var response = await _apiService.ImprimerEtiquettesAsync(etiquetteIds, imprimanteId ?? _sessionService.GetImprimanteId());
            return response.Success;
        }

        public async Task<bool> ImprimerBonReceptionAsync(string commandeId, string imprimanteId = null)
        {
            var response = await _apiService.ImprimerDocumentsAsync(
                commandeId, 
                imprimerCf: false, 
                imprimerCm: false, 
                imprimanteId: imprimanteId ?? _sessionService.GetImprimanteId());
            return response.Success;
        }

        public async Task<bool> ImprimerCommandeFournisseurAsync(string commandeId, string imprimanteId = null)
        {
            var response = await _apiService.ImprimerDocumentsAsync(
                commandeId, 
                imprimerCf: true, 
                imprimerCm: false, 
                imprimanteId: imprimanteId ?? _sessionService.GetImprimanteId());
            return response.Success;
        }

        public async Task<bool> ImprimerCommandeContremarqueAsync(string commandeId, string ligneId, string imprimanteId = null)
        {
            // Pour une ligne spécifique, nous utilisons une autre route API
            var data = new
            {
                commandeId = commandeId,
                ligneId = ligneId,
                imprimanteId = imprimanteId ?? _sessionService.GetImprimanteId()
            };

            // En pratique, cela serait géré par l'API avec un autre endpoint
            var response = await _apiService.ImprimerDocumentsAsync(
                commandeId, 
                imprimerCf: false, 
                imprimerCm: true, 
                imprimanteId: imprimanteId ?? _sessionService.GetImprimanteId());
            return response.Success;
        }

        public async Task<List<Printer>> GetImprimentesDisponiblesAsync()
        {
            var response = await _apiService.GetImprimentesDisponiblesAsync();
            return response.Success ? response.Data : new List<Printer>();
        }

        public async Task<Printer> GetDefaultImprimanteAsync()
        {
            var imprimanteId = _sessionService.GetImprimanteId();
            if (string.IsNullOrEmpty(imprimanteId))
                return null;

            var imprimantes = await GetImprimentesDisponiblesAsync();
            return imprimantes.FirstOrDefault(p => p.Id == imprimanteId);
        }

        public async Task SetDefaultImprimanteAsync(string imprimanteId)
        {
            var user = _sessionService.CurrentUser;
            if (user != null)
            {
                user.ImprimanteId = imprimanteId;
                _sessionService.SetCurrentUser(user);
            }
        }
    }
}