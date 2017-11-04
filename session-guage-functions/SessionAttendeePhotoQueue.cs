using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace SessionSuccessGaugeFunctions
{
    public static class SessionAttendeePhotoQueue
    {

        /// <summary>
        /// SessionAttendeePhotoQueue:  Function that monitors photo requests coming into the designated queue
        /// TIP: If you use more generic types for your input/output such as Stream/ICollector (for Blob/Table)
        /// it makes it easier to build unit tests. In fact you can can have your function not even reference those
        /// specific SDKs reducing dependencies and making it simple to use mock data in unit tests.
        /// PARAMETERS: Because we are using attributed input\output parameters the functions runtime will handle 
        /// hydrating those objects and values as required based on the parameter values - saves us a lot of coding!
        /// </summary>
        /// <param name="photoRequestMessage"> Trigger Parameter - this will contain the message - in our case the GUID 
        /// of the blob being inserted but this could be json in a string for more complex objects</param>
        /// <param name="photoBlob">Input parameter that is automatically built for us based on the name of the container retrieved from appsettings (signifiged by % % )</param>
        /// <param name="resultsTable">Output Parmater that represents our Azure Storage Table.</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("SessionAttendeePhotoQueue")]
        [StorageAccount("SessionGuageStorage")]
        public static async Task Run(
            [QueueTrigger("%SessionGuageQueue%", Connection = "SessionGuageStorage")]string photoRequestMessage,
            [Blob("%SessionGuageStorageContainer%/{QueueTrigger}", FileAccess.Read)]Stream photoBlob, 
            [Table("%SessionGuageStorageTable%")] ICollector<AttendeeAnalysisResult> resultsTable,
            TraceWriter log)
        {
            var result = await RequestEmotionAnalysis(photoBlob);

            // RowKey & PartitionKey Required for Table Storage
            result.RowKey = photoRequestMessage;
            result.PartitionKey = "SessionResult";
            resultsTable.Add(result);

            log.Info($"C# Queue trigger function processed: {photoRequestMessage}");
        }


        /// <summary>
        /// Responsible for querying Azure Cognitive Services REST API 
        /// </summary>
        /// <param name="photo"></param>
        /// <returns></returns>
        private static async Task<AttendeeAnalysisResult> RequestEmotionAnalysis(Stream photo)
        {
            //Carefull w/ instantiating client in this manner - should be static variable
            //so it can be shared or risk HttpClient Port Exhaustion (google it :) ) 
            var client = new HttpClient();

            var subscriptionUrl = ConfigurationManager.AppSettings["computervisionApiUrl"];
            var subscriptionKey = ConfigurationManager.AppSettings["computervisionkey"];

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            HttpResponseMessage response;
            string responseContent;

            // Request body. Try this sample with a locally stored JPEG image.
            byte[] byteData = photo.ToByteArray();

            using (var content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(subscriptionUrl, content);
                responseContent = await response.Content.ReadAsStringAsync();

                List<Face> faces = JsonConvert.DeserializeObject<List<Face>>(responseContent);

                AttendeeAnalysisResult result = new AttendeeAnalysisResult();
                result.FaceCount = faces.Count();

                //Provide a 0-100 score instead of a double decimal score
                result.Anger = (int)Math.Floor(faces.Average(r => r.scores.anger) * 100);
                result.Contempt= (int)Math.Floor(faces.Average(r => r.scores.contempt) * 100);
                result.Disgust = (int)Math.Floor(faces.Average(r => r.scores.disgust) * 100);
                result.Fear = (int)Math.Floor(faces.Average(r => r.scores.fear) * 100);
                result.Happiness = (int)Math.Floor(faces.Average(r => r.scores.happiness) * 100);
                result.Neutral = (int)Math.Floor(faces.Average(r => r.scores.neutral) * 100);
                result.Sadness = (int)Math.Floor(faces.Average(r => r.scores.sadness) * 100);
                result.Surprise= (int)Math.Floor(faces.Average(r => r.scores.surprise) * 100);

                return result;

            }

        }

        public static byte[] ToByteArray(this Stream stream)
        {
            using (stream)
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);
                    return memStream.ToArray();
                }
            }
        }
    }
}
