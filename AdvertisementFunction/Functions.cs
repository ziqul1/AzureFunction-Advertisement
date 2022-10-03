using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using AdvertisementFunction.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using AdvertisementFunction.Models;

namespace AdvertisementFunction
{
    public class Functions
    {
        private readonly string _advertisementFuncSACS;
        private readonly string _tableReference;
        private readonly IAdvertisementService _advertisementService;

        public Functions(IConfiguration configuration, IAdvertisementService advertisementService)
        {
            // instead of the line below we can use configuration.GetConnectionString
            _advertisementFuncSACS = "";
            _tableReference = "";
            _advertisementService = advertisementService;
        }

        [FunctionName("GetAllAsync")]
        public async Task<IActionResult> GetAllAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetAllAsync")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            CloudStorageAccount advertisementFuncSA = CloudStorageAccount.Parse(_advertisementFuncSACS);
            CloudTableClient tableClient = advertisementFuncSA.CreateCloudTableClient();
            CloudTable tableInStorageAccount = tableClient.GetTableReference(_tableReference);

            var advertisements = await _advertisementService.GetAllAsync(tableInStorageAccount);

            return new OkObjectResult(advertisements);
        }

        [FunctionName("GetSingleAsync")]
        public async Task<IActionResult> GetSingleAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetSingleAsync")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            CloudStorageAccount advertisementFuncSA = CloudStorageAccount.Parse(_advertisementFuncSACS);
            CloudTableClient tableClient = advertisementFuncSA.CreateCloudTableClient();
            CloudTable tableInStorageAccount = tableClient.GetTableReference(_tableReference);

            string id = req.Query["id"];

            var advertisement = await _advertisementService.GetSingleAsync(id, tableInStorageAccount);

            return new OkObjectResult(advertisement);
        }

        [FunctionName("CreateAsync")]
        public async Task<IActionResult> CreateAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CreateAsync")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            CloudStorageAccount advertisementFuncSA = CloudStorageAccount.Parse(_advertisementFuncSACS);
            CloudTableClient tableClient = advertisementFuncSA.CreateCloudTableClient();
            CloudTable tableInStorageAccount = tableClient.GetTableReference(_tableReference);

            var content = await new StreamReader(req.Body).ReadToEndAsync();
            var query = JsonConvert.DeserializeObject<Advertisement>(content);

            // sprawdzenie czy juz mamy taki rekord o RowKey podanym przez u¿ytkownika
            var isInStorageTable = await _advertisementService.GetSingleAsync(query.RowKey, tableInStorageAccount);
            if (isInStorageTable != null)
                throw new Exception("Rowkey is existing");
            
            await _advertisementService.CreateAsync(query, tableInStorageAccount);
            return new OkObjectResult(query);
        }

        [FunctionName("DeleteAsync")]
        public async Task<IActionResult> DeleteAsync(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "DeleteAsync")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            CloudStorageAccount advertisementFuncSA = CloudStorageAccount.Parse(_advertisementFuncSACS);
            CloudTableClient tableClient = advertisementFuncSA.CreateCloudTableClient();
            CloudTable tableInStorageAccount = tableClient.GetTableReference(_tableReference);

            string id = req.Query["id"];

            var response = await _advertisementService.DeleteAsync(id, tableInStorageAccount);
            if (response == false)
                return new BadRequestResult();

            return new OkObjectResult(response);
        }

        [FunctionName("UpdateAsync")]
        public async Task<IActionResult> UpdateAsync(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "UpdateAsync")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            CloudStorageAccount advertisementFuncSA = CloudStorageAccount.Parse(_advertisementFuncSACS);
            CloudTableClient tableClient = advertisementFuncSA.CreateCloudTableClient();
            CloudTable tableInStorageAccount = tableClient.GetTableReference(_tableReference);

            var content = await new StreamReader(req.Body).ReadToEndAsync();
            var query = JsonConvert.DeserializeObject<Advertisement>(content);

            await _advertisementService.UpdateAsync(query, tableInStorageAccount);

            return new OkObjectResult(query);
        }
    }
}
