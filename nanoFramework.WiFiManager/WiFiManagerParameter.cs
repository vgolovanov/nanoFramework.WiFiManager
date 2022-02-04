using System;
using System.Text;

namespace nanoFramework.WiFiManager
{
    public class WiFiManagerParameter
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public string Placeholder { get; set; } 
        public string CustomHTML { get; set; }

        public WiFiManagerParameter(string CustomHTML)
        {            
            this.CustomHTML = CustomHTML;
        }
     
        public WiFiManagerParameter(string Id, string Placeholder, string DefaultValue)
        {
            this.Id = Id;
            this.Placeholder = Placeholder;
            this.CustomHTML = CustomHTML;
        }

        public WiFiManagerParameter(string Id, string Placeholder, string DefaultValue, string CustomHTML)
        {    
            this.Id = Id;
            this.Placeholder = Placeholder;
            this.Value = DefaultValue;
            this.CustomHTML = CustomHTML;
        }
    }
}
