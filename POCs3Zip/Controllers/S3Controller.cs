using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace POCs3Zip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class S3Controller : ControllerBase
    {

        private readonly string _keyId;
        private readonly string _keySecret;
        private readonly string _bucketName;
        private readonly string _baseDownloadLocation;
        private readonly RegionEndpoint regionEndpoint = RegionEndpoint.USEast2;

        private readonly ILogger<S3Controller> _logger;

        public S3Controller(ILogger<S3Controller> logger)
        {
            _keyId = Environment.GetEnvironmentVariable("AWS_KEY_ID");
            _keySecret = Environment.GetEnvironmentVariable("AWS_KEY_SECRET");
            _bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME");
            _baseDownloadLocation = Path.GetTempPath();

            logger.LogInformation($"AWS_KEY_ID={_keyId}");
            logger.LogInformation($"AWS_KEY_SECRET={_keySecret}");
            logger.LogInformation($"AWS_BUCKET_NAME={_bucketName}");
            _logger = logger;
        }

        [HttpGet("download/directory")]
        public async Task<IActionResult> DownloadDirectoryFromS3()
        {
            _logger.LogInformation($"AWS_KEY_ID={_keyId}");
            _logger.LogInformation($"AWS_KEY_SECRET={_keySecret}");
            _logger.LogInformation($"AWS_BUCKET_NAME={_bucketName}");

            string downloadLocation = _baseDownloadLocation + Guid.NewGuid().ToString();
            using (var client = new AmazonS3Client(_keyId, _keySecret, regionEndpoint))
            {
                return await DownloadDirectoryFromS3Async(client, downloadLocation);
            }
        }

        [HttpGet("download/searchByKeys")]
        public async Task<IActionResult> GetObjectsFromS3([FromQuery]string[] keys)
        {
            string downloadLocation = _baseDownloadLocation + Guid.NewGuid().ToString();

            if (!keys.Any())
                return BadRequest("Infome alguma chave para buscar");

            using (var client = new AmazonS3Client(_keyId, _keySecret, regionEndpoint))
            {
                return await SearchByKeysAndDownloadDirectoryFromS3Async(client, keys, downloadLocation);
            }
        }

        private async Task<IActionResult> DownloadDirectoryFromS3Async(AmazonS3Client client, string downloadLocation)
        {
            string zipFolder = downloadLocation + ".zip";
            if (!Directory.Exists(downloadLocation))
                Directory.CreateDirectory(downloadLocation);

            var transferUtility = new TransferUtility(client);
            await transferUtility.DownloadDirectoryAsync(_bucketName, "\\", downloadLocation);
            return CreateZip(downloadLocation, zipFolder);
        }

        private async Task<IActionResult> SearchByKeysAndDownloadDirectoryFromS3Async(AmazonS3Client client, string[] keys, string downloadLocation)
        {
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                MaxKeys = keys.Length,
            };

            string zipFolder = downloadLocation + ".zip";
            if (!Directory.Exists(downloadLocation))
                Directory.CreateDirectory(downloadLocation);

            IEnumerable<Task> tasks = keys.Select(key =>
            {
                try
                {
                    string filename = downloadLocation + "\\" + key;

                    if (System.IO.File.Exists(filename)) return Task.CompletedTask;

                    GetObjectResponse objResponse = client.GetObjectAsync(_bucketName, key, default).GetAwaiter().GetResult();
                    return objResponse.WriteResponseStreamToFileAsync(filename, true, default);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw ex;
                }

            });

            await Task.WhenAll(tasks);

            return CreateZip(downloadLocation, zipFolder);
        }

        private IActionResult CreateZip(string source, string zipFolder)
        {
            ZipFile.CreateFromDirectory(source, zipFolder, CompressionLevel.Fastest, false);

            string contentType = "application/zip";
            HttpContext.Response.ContentType = contentType;


            var result = new FileContentResult(System.IO.File.ReadAllBytes(zipFolder), contentType)
            {
                FileDownloadName = $"teste.zip"
            };

            Directory.Delete(source, true);
            System.IO.File.Delete(zipFolder);

            return result;
        }

    }
}