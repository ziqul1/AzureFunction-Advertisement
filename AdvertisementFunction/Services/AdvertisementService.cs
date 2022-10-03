using AdvertisementFunction.Models;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvertisementFunction.Services
{
    public class AdvertisementService : IAdvertisementService
    {
        public async Task<List<Advertisement>> GetAllAsync(CloudTable tableInStorageAccount)
        {
            var query = new TableQuery<Advertisement>();
            var entities = await tableInStorageAccount.ExecuteQuerySegmentedAsync<Advertisement>(query, null);

            return entities.Results.ToList();
        }

        public async Task<Advertisement> GetSingleAsync(string id, CloudTable tableInStorageAccount)
        {
            var query = new TableQuery<Advertisement>();
            var entities = await tableInStorageAccount.ExecuteQuerySegmentedAsync<Advertisement>(query, null);
            // przyrównujemy RowKey do id, bo RowKey jest unikalny (może być tylko jeden konkretny)
            var advertisementDTO = entities.Results.FirstOrDefault(x => x.RowKey == id);

            if (advertisementDTO == null)
                return null;

            return new Advertisement
            {
                PartitionKey = advertisementDTO.PartitionKey,
                RowKey = advertisementDTO.RowKey,
                Title = advertisementDTO.Title,
                Description = advertisementDTO.Description,
                Type = advertisementDTO.Type
            };
        }

        public async Task CreateAsync(Advertisement advertisement, CloudTable tableInStorageAccount)
        {
            TableOperation tableOperation = TableOperation.InsertOrMerge(advertisement);
            await tableInStorageAccount.ExecuteAsync(tableOperation);
        }

        public async Task<bool> DeleteAsync(string id, CloudTable tableInStorageAccount)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<Advertisement>(id, id);
            TableResult tableResult = await tableInStorageAccount.ExecuteAsync(retrieveOperation);
            var getDeletingRecord = tableResult.Result as Advertisement;
            TableOperation deleteOperation = TableOperation.Delete(getDeletingRecord);
            await tableInStorageAccount.ExecuteAsync(deleteOperation);

            return true;
        }

        public async Task UpdateAsync(Advertisement advertisement, CloudTable tableInStorageAccount)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<Advertisement>(advertisement.PartitionKey, advertisement.RowKey);
            TableResult tableResult = await tableInStorageAccount.ExecuteAsync(retrieveOperation);
            var getEditingRecord = tableResult.Result as Advertisement;
            getEditingRecord.Title = advertisement.Title;
            getEditingRecord.Description = advertisement.Description;
            getEditingRecord.Type = advertisement.Type;
            TableOperation updateOperation = TableOperation.Replace(getEditingRecord);
            await tableInStorageAccount.ExecuteAsync(updateOperation);
        }
    }
}
