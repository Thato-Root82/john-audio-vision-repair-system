using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace John_Audio_Vision_FromsApp_
{
    public class RepairJob
    {
        public string ClientName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Issue { get; set; } = string.Empty;
        public string JobDone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public DateTime Date { get; set; }
    }

}
