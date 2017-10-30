
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;

namespace SessionSuccessGaugeFunctions
{
    public static class SessionAttendeePhotos
    {
        /// <summary>
        /// Responsible for accepting an http request to either get session results or post new 
        /// photos to be analyzed.
        /// TIP:  In this example I'm using SDK Specific data types for my input\output parameters
        /// these will require a reference to the appropriate SDK (Nuget Package). Look at the Queue
        /// Function to look for alternative data types to provide a more generic and testable interface.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="outputPhotoBlob"></param>
        /// <param name="outputQueue"></param>
        /// <param name="resultsTable"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("SessionAttendeePhotos")]
        [StorageAccount("SessionGuageStorage")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "get")]HttpRequestMessage req,
            [Blob("%SessionGuageStorageContainer%/{rand-guid}", FileAccess.ReadWrite)]ICloudBlob outputPhotoBlob, 
            [Queue("%SessionGuageQueue%")]CloudQueue outputQueue,
            [Table("%SessionGuageStorageTable%")] CloudTable resultsTable,
            TraceWriter log)
        {

            var response = req.CreateResponse(HttpStatusCode.OK);

            if (req.Method == HttpMethod.Post)
            {
                response = await PostPhoto(req, outputPhotoBlob, outputQueue);
            }
            else if (req.Method == HttpMethod.Get)
            {
                response = await GetFaceAnalsys(req, resultsTable);
            }

            return response; 
        }

        /// <summary>
        /// Place the photo into blob storage and then add the request to the queue to be handled by the SessionAttendeePhotoQueue
        /// </summary>
        /// <param name="req"></param>
        /// <param name="outputPhotoBlob"></param>
        /// <param name="outputQueue"></param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> PostPhoto(HttpRequestMessage req, ICloudBlob outputPhotoBlob, CloudQueue outputQueue)
        {
            //We receive our image as a base64 from the browser camera
            var data = await req.Content.ReadAsStringAsync();
            data = data.Replace("data:image/png;base64,", "");
            var image = Convert.FromBase64String(data);

            // Add the photo to blob storage

            await outputPhotoBlob.UploadFromByteArrayAsync(image, 0, image.Length);

            // Add the request to the queue to be processed
            // NOTE: We could just place this into Blob without Queue and have a function listen 
            //       on the blob storage insert but we want it processed right away and blob 
            //       storage triggers may be delayed up to 10 minutes
            outputQueue.AddMessage(new CloudQueueMessage(outputPhotoBlob.Name));

            var response = req.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        /// <summary>
        /// Retrieve a simple summary (average) of the results being retrieved
        /// </summary>
        /// <param name="req"></param>
        /// <param name="resultsTable"></param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> GetFaceAnalsys(HttpRequestMessage req, CloudTable resultsTable)
        {

            var results = resultsTable.ExecuteQuery<AttendeeAnalysisResult>(new TableQuery<AttendeeAnalysisResult>()).ToList();
            var data = new List<GaugeData>();

            if (results.Count() > 0)
            {
                data.Add(new GaugeData { name = "Anger", value = (int)Math.Floor(results.Average(r => r.Anger)) });
                data.Add(new GaugeData { name = "Contempt", value = (int)Math.Floor(results.Average(r => r.Contempt)) });
                data.Add(new GaugeData { name = "Disgust", value = (int)Math.Floor(results.Average(r => r.Disgust)) });
                data.Add(new GaugeData { name = "Fear", value = (int)Math.Floor(results.Average(r => r.Fear)) });
                data.Add(new GaugeData { name = "Happiness", value = (int)Math.Floor(results.Average(r => r.Happiness)) }); ;
                data.Add(new GaugeData { name = "Neutral", value = (int)Math.Floor(results.Average(r => r.Neutral)) });
                data.Add(new GaugeData { name = "Sadness", value = (int)Math.Floor(results.Average(r => r.Sadness)) });
                data.Add(new GaugeData { name = "Suprise", value = (int)Math.Floor(results.Average(r => r.Suprise)) });
            }

            var response = req.CreateResponse(HttpStatusCode.OK, data);
            return response;

        }
    }
}
