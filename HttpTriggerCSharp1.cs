using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Covid19;
using System.Collections.Generic;

namespace Covid19ApiCore
{
    public static class HttpTriggerCSharp1
    {
        
        [FunctionName("GetData")]
        public static async Task<string> GetData([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req, ILogger log)
        {
            await Task.Delay(10);
            string FilterString = req.Query["filter"];
            string latString = req.Query["latitude"];
            string lonString = req.Query["longitude"];

            log.LogInformation("Filter: " + FilterString);
            log.LogInformation("Latitude: " + latString);
            log.LogInformation("Longitude: " + lonString);

            List<string> FilterParts = new List<string>();
            if (FilterString != null)
            {
                List<string> Splitter = new List<string>();
                Splitter.Add(",");
                string[] parts = FilterString.Split(Splitter.ToArray(), StringSplitOptions.None);
                foreach (string p in parts)
                {
                    FilterParts.Add(p.Replace("_"," "));
                }
                log.LogInformation(FilterParts.Count.ToString() + " filters");
            }
            else
            {
                log.LogInformation("No filters supplied!");
            }

            log.LogInformation("Downloading data...");
            CovidDataHelper cdh = new CovidDataHelper();
            Area global = await cdh.GetGlobalDataAsync();

            //Filtering based on the filters
            Area core = null;
            if (FilterParts.Count > 0)
            {
                log.LogInformation("Filtering data...");
                core = cdh.ChainFilter(global, FilterParts.ToArray());
            }

            if (core == null)
            {
                return "Fatal failure while filtering with filter query '" + FilterString + "'.";
            }


            //Sort by distance
            if (latString != null && lonString != null)
            {
                
                log.LogInformation("Converting lat & lon to float...");
                float mlat = 0;
                float mlon = 0;
                try
                {
                    mlat = Convert.ToSingle(latString);
                    mlon = Convert.ToSingle(lonString);
                }
                catch
                {
                    return "Fatal error while converting latitude '" + latString + "' and longitude '" + lonString + "' to numbers.";
                }
                
                log.LogInformation("Sorting based on provided latitude and longitude...");
                Area[] sorted = cdh.DistanceSort(core.areas, mlat, mlon);

                //Set it as the areas child now
                core.areas = sorted;
            }
            else
            {
                log.LogInformation("No need to sort by distance because a lat and long were not provided.");
            }

            string ToReturn = JsonConvert.SerializeObject(core);

            return ToReturn;
        }

        [FunctionName("GetDataV2")]
        public static async Task<Area> GetDataV2([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req, ILogger log)
        {
            await Task.Delay(10);
            string FilterString = req.Query["filter"];
            string latString = req.Query["latitude"];
            string lonString = req.Query["longitude"];

            log.LogInformation("Filter: " + FilterString);
            log.LogInformation("Latitude: " + latString);
            log.LogInformation("Longitude: " + lonString);

            List<string> FilterParts = new List<string>();
            if (FilterString != null)
            {
                List<string> Splitter = new List<string>();
                Splitter.Add(",");
                string[] parts = FilterString.Split(Splitter.ToArray(), StringSplitOptions.None);
                foreach (string p in parts)
                {
                    FilterParts.Add(p.Replace("_"," "));
                }
                log.LogInformation(FilterParts.Count.ToString() + " filters");
            }
            else
            {
                log.LogInformation("No filters supplied!");
            }

            log.LogInformation("Downloading data...");
            CovidDataHelper cdh = new CovidDataHelper();
            Area global = await cdh.GetGlobalDataAsync();

            //Filtering based on the filters
            Area core = null;
            if (FilterParts.Count > 0)
            {
                log.LogInformation("Filtering data...");
                core = cdh.ChainFilter(global, FilterParts.ToArray());
            }

            if (core == null)
            {
                throw new Exception();
            }


            //Sort by distance
            if (latString != null && lonString != null)
            {
                
                log.LogInformation("Converting lat & lon to float...");
                float mlat = 0;
                float mlon = 0;
                try
                {
                    mlat = Convert.ToSingle(latString);
                    mlon = Convert.ToSingle(lonString);
                }
                catch
                {
                    throw new Exception();
                }
                
                log.LogInformation("Sorting based on provided latitude and longitude...");
                Area[] sorted = cdh.DistanceSort(core.areas, mlat, mlon);

                //Set it as the areas child now
                core.areas = sorted;
            }
            else
            {
                log.LogInformation("No need to sort by distance because a lat and long were not provided.");
            }

            return core;
        }


    }
}
