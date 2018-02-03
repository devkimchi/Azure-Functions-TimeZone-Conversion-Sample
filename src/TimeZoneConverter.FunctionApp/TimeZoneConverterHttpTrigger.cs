using System;
using System.Globalization;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Newtonsoft.Json;

namespace TimeZoneConverter.FunctionApp
{
    /// <summary>
    /// This represents the trigger entity for time zone converter.
    /// </summary>
    public static class TimeZoneConverterHttpTrigger
    {
        /// <summary>
        /// Invokes the HTTP trigger function.
        /// </summary>
        /// <param name="req"><see cref="HttpRequest"/> instance.</param>
        /// <param name="req"><see cref="TraceWriter" /> instance.</param>
        /// <returns></returns>
        [FunctionName("TimeZoneConverterHttpTrigger")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "convert/timezone")]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Suppress Date/Time string to be converted to DateTime object.
            var settings = new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None };

            dynamic body = JsonConvert.DeserializeObject(await req.ReadAsStringAsync().ConfigureAwait(false), settings);
            var input = (string)body?.input;

            if (string.IsNullOrWhiteSpace(input))
            {
                return new BadRequestObjectResult("No date/time value");
            }

            try
            {
                var utc = DateTimeOffset.TryParse(input, null, DateTimeStyles.AssumeUniversal, out DateTimeOffset result) ? result : DateTimeOffset.MinValue;
                if (utc == DateTimeOffset.MinValue)
                {
                    var error = new { messagae = "Invalid Date/Time" };

                    return new BadRequestObjectResult(error);
                }

                var aest = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
                var output = TimeZoneInfo.ConvertTime(utc, aest);

                var response = new { input = input, output = output };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}