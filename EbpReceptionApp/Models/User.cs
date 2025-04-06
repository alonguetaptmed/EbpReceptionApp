using Newtonsoft.Json;

namespace EbpReceptionApp.Models
{
    public class User
    {
        [JsonProperty("userId")]
        public string Id { get; set; }
        
        [JsonProperty("username")]
        public string Username { get; set; }
        
        [JsonProperty("fullName")]
        public string FullName { get; set; }
        
        [JsonProperty("depot")]
        public string Depot { get; set; }
        
        [JsonProperty("imprimanteId")]
        public string ImprimanteId { get; set; }
        
        [JsonProperty("imprimanteNom")]
        public string ImprimanteNom { get; set; }
        
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}