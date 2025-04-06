using Newtonsoft.Json;

namespace EbpReceptionApp.Models
{
    public class Printer
    {
        [JsonProperty("printerId")]
        public string Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("address")]
        public string Address { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("isActive")]
        public bool IsActive { get; set; }
    }
}