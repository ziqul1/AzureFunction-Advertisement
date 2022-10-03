using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvertisementFunction.Models
{
    public class Advertisement : TableEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
    }
}
