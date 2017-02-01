#r "Newtonsoft.Json"

using System;
using Newtonsoft.Json;
using System.Configuration;

public static async Task Run(TimerInfo myTimer, dynamic inputDocument, TraceWriter log)
{
    using (var client = new HttpClient())
    {
        var response = await client.GetAsync(
            "https://graph.facebook.com/v2.8/1504549153159226/feed?" + 
            "fields=story,updated_time,message,description,link,from,name&" + 
            "access_token=" + ConfigurationManager.AppSettings["FacebookGraphAccessToken"]
            );

        string jsonResult = await response.Content.ReadAsStringAsync();

        var newFeeds = JsonConvert.DeserializeObject<FacebookGroupFeed>(jsonResult);
        var existingFeeds = JsonConvert.DeserializeObject<FacebookGroupFeed>(inputDocument.ToString());

        var finalizedFeeds = new List<FacebookFeed>(existingFeeds.Feeds);

        foreach(var newFeed in newFeeds.Feeds) {
            var isFeedExist = false;

            foreach(var existingFeed in existingFeeds.Feeds) {
                if (existingFeed.Id == newFeed.Id) {
                    isFeedExist = true;

                    break;
                }
            }

            if (!isFeedExist) {
                finalizedFeeds.Add(newFeed);
            }
        }

        existingFeeds.Feeds = finalizedFeeds;

        string finalizedJson = JsonConvert.SerializeObject(existingFeeds);

        log.Info($"Result Returned: {finalizedJson}.");

        inputDocument.data = existingFeeds.Feeds;
    }  
}

public class FacebookGroupFeed
{
    [JsonProperty(PropertyName="data")]
    public IEnumerable<FacebookFeed> Feeds { get; set; }
}

public class FacebookFeed
{
    [JsonProperty(PropertyName="id")]
    public string Id { get; set; }

    [JsonProperty(PropertyName="story")]
    public string Story { get; set; }

    [JsonProperty(PropertyName="message")]
    public string Message { get; set; }

    [JsonProperty(PropertyName="name")]
    public string ArticleName { get; set; }

    [JsonProperty(PropertyName="description")]
    public string ArticleDescription { get; set; }

    [JsonProperty(PropertyName="link")]
    public string ArticleUrl { get; set; }

    [JsonProperty(PropertyName="from")]
    public FacebookProfile Author { get; set; }

    [JsonProperty(PropertyName="updated_time")]
    public DateTimeOffset UpdatedTime { get; set; }
}

public class FacebookProfile
{
    [JsonProperty(PropertyName="id")]
    public string Id { get; set; }

    [JsonProperty(PropertyName="name")]
    public string Name { get; set; }
}