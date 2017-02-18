#r "Newtonsoft.Json"

using System;
using Newtonsoft.Json;
using System.Configuration;

private const string SG_DOT_NET_COMMUNITY_FB_GROUP_ID = "1504549153159226";
private const string SG_AZURE_COMMUNITY_FB_GROUP_ID = "774384765970833";

public static async Task Run(TimerInfo myTimer, dynamic inputDocument, TraceWriter log)
{
    string sgDotNetCommunityFacebookGroupFeedsJson = await GetFacebookGroupFeedsAsJsonAsync(SG_DOT_NET_COMMUNITY_FB_GROUP_ID);

    string sgAzureCommunityFacebookGroupFeedsJson = await GetFacebookGroupFeedsAsJsonAsync(SG_AZURE_COMMUNITY_FB_GROUP_ID);

    var newSgDotNetCommunityFacebookGroupFeeds = JsonConvert.DeserializeObject<FacebookGroupFeed>(sgDotNetCommunityFacebookGroupFeedsJson);

    var newAzureCommunityFacebookGroupFeeds = JsonConvert.DeserializeObject<FacebookGroupFeed>(sgAzureCommunityFacebookGroupFeedsJson);

    var existingFeeds = JsonConvert.DeserializeObject<FacebookGroupFeed>(inputDocument.ToString());

    var finalizedFeeds = UnionLists(existingFeeds.Feeds, newSgDotNetCommunityFacebookGroupFeeds.Feeds);

    finalizedFeeds = UnionLists(finalizedFeeds, newAzureCommunityFacebookGroupFeeds.Feeds);

    existingFeeds.Feeds = finalizedFeeds;

    string finalizedJson = JsonConvert.SerializeObject(existingFeeds);

    log.Info($"Result Returned: {finalizedJson}.");

    inputDocument.data = existingFeeds.Feeds;
}

private static async Task<string> GetFacebookGroupFeedsAsJsonAsync(string facebookGroupId) 
{
    using (var client = new HttpClient())
    {
        var response = await client.GetAsync(
            "https://graph.facebook.com/v2.8/" + facebookGroupId + "/feed?" + 
            "fields=story,updated_time,message,description,link,from,name&" + 
            "access_token=" + ConfigurationManager.AppSettings["FacebookGraphAccessToken"]
            );

        string jsonResult = await response.Content.ReadAsStringAsync();

        return jsonResult;
    }
    
}

private static List<FacebookFeed> UnionLists(List<FacebookFeed> targetList, List<FacebookFeed> sourceList)
{
    var finalizedFeeds = new List<FacebookFeed>(targetList);

    foreach(var newItem in sourceList) 
    {
        var isFeedExist = false;

        foreach(var existingItem in targetList) 
        {
            if (existingItem.Id == newItem.Id || 
            (existingItem.ArticleUrl != null && newItem.ArticleUrl != null && existingItem.ArticleUrl.Trim() == newItem.ArticleUrl.Trim())) {

                isFeedExist = true;

                break;
            }
        }

        if (!isFeedExist) {
            finalizedFeeds.Add(newItem);
        }
    }

    return finalizedFeeds;
}

public class FacebookGroupFeed
{
    [JsonProperty(PropertyName="data")]
    public List<FacebookFeed> Feeds { get; set; }
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