using AdvertisementFunction.Models;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvertisementFunction.Services
{
    public interface IAdvertisementService
    {
        public Task<List<Advertisement>> GetAllAsync(CloudTable tableInStorageAccount);
        public Task<Advertisement> GetSingleAsync(string id, CloudTable tableInStorageAccount);
        public Task CreateAsync(Advertisement advertisement, CloudTable tableInStorageAccount);
        public Task<bool> DeleteAsync(string id, CloudTable tableInStorageAccount);
        public Task UpdateAsync(Advertisement advertisement, CloudTable tableInStorageAccount);
    }
}
