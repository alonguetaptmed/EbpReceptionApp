using Newtonsoft.Json;
using System;

namespace EbpReceptionApp.Models
{
    public class Etiquette
    {
        [JsonProperty("idEtiquette")]
        public string Id { get; set; }
        
        [JsonProperty("idLigneCommande")]
        public string IdLigneCommande { get; set; }
        
        [JsonProperty("numeroEtiquette")]
        public string NumeroEtiquette { get; set; }
        
        [JsonProperty("nombreMetresLineaires")]
        public decimal NombreMetresLineaires { get; set; }
        
        [JsonProperty("nombreRouleaux")]
        public int NombreRouleaux { get; set; }
        
        [JsonProperty("dateCreation")]
        public DateTime DateCreation { get; set; } = DateTime.Now;
        
        [JsonProperty("estImprimee")]
        public bool EstImprimee { get; set; }
        
        [JsonIgnore]
        public decimal TotalMetresLineaires => NombreMetresLineaires * NombreRouleaux;
    }
}