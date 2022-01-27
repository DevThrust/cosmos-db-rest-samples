using System.Dynamic;
using System.Net;
using System.Text.Json;

var cosmosKey = Environment.GetEnvironmentVariable("Cosmos:Key");
var accountName = Environment.GetEnvironmentVariable("Cosmos:AccountName");
var databaseName = Environment.GetEnvironmentVariable("Cosmos:DatabaseName");
var containerName = Environment.GetEnvironmentVariable("Cosmos:ContainerName");

if (string.IsNullOrEmpty(cosmosKey) || string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(containerName))
{
    Console.WriteLine("Missing one or more configuration values. Please make sure to set them in the `environmenVariables` section");
    return;
}

var baseUrl = $"https://{accountName}.documents.azure.com";
var httpClient = new HttpClient();

//Document operations

var item1 = new ItemDto("id1", "pk1", "value1");
var item11 = new ItemDto("id11", "pk1", "value-11");
var item2 = new ItemDto("id2", "pk1", "value2");
var item3 = new ItemDto("id3", "pk2", "value3");

await CreateDocument(item1);
await CreateDocument(item2);
await CreateDocument(item3);

await PatchDocument(id: item1.id, partitionKey: item1.pk);
await ReplaceDocument(id: item1.id, newItem: item11); //cannot change partitionKey in a replace operation, but can update id


await ListDocuments(partitionKey: item1.pk);
await GetDocument(id: item2.id, partitionKey: item2.pk);

await QueryDocuments(partitionKey: item1.pk);
await QueryDocumentsCrossPartition();

await DeleteDocument(id: item1.id, partitionKey: item1.pk);
await DeleteDocument(id: item11.id, partitionKey: item11.pk);
await DeleteDocument(id: item2.id, partitionKey: item2.pk);
await DeleteDocument(id: item3.id, partitionKey: item3.pk);


async Task CreateDocument(ItemDto item)
{
    var method = HttpMethod.Post;

    var resourceType = ResourceType.docs;
    var resourceLink = $"dbs/{databaseName}/colls/{containerName}";
    var requestDateString = DateTime.UtcNow.ToString("r");
    var auth = GenerateMasterKeyAuthorizationSignature(method, resourceType, resourceLink, requestDateString, cosmosKey);

    httpClient.DefaultRequestHeaders.Clear();
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    httpClient.DefaultRequestHeaders.Add("authorization", auth);
    httpClient.DefaultRequestHeaders.Add("x-ms-date", requestDateString);
    httpClient.DefaultRequestHeaders.Add("x-ms-documentdb-is-upsert", "True");
    httpClient.DefaultRequestHeaders.Add("x-ms-version", "2018-12-31");
    httpClient.DefaultRequestHeaders.Add("x-ms-documentdb-partitionkey", $"[\"{item.pk}\"]");

    var requestUri = new Uri($"{baseUrl}/{resourceLink}/docs");
    var requestContent = new StringContent(JsonSerializer.Serialize(item), System.Text.Encoding.UTF8, "application/json");

    var httpRequest = new HttpRequestMessage { Method = method, Content = requestContent, RequestUri = requestUri };

    var httpResponse = await httpClient.SendAsync(httpRequest);
    await ReportOutput("Create Document", httpResponse);
}

async Task ListDocuments(string partitionKey)
{
    var method = HttpMethod.Get;
    var resourceType = ResourceType.docs;
    var resourceLink = $"dbs/{databaseName}/colls/{containerName}";
    var requestDateString = DateTime.UtcNow.ToString("r");
    var auth = GenerateMasterKeyAuthorizationSignature(method, resourceType, resourceLink, requestDateString, cosmosKey);

    httpClient.DefaultRequestHeaders.Clear();
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    httpClient.DefaultRequestHeaders.Add("authorization", auth);
    httpClient.DefaultRequestHeaders.Add("x-ms-date", requestDateString);
    httpClient.DefaultRequestHeaders.Add("x-ms-version", "2018-12-31");
    httpClient.DefaultRequestHeaders.Add("x-ms-documentdb-partitionkey", $"[\"{partitionKey}\"]");

    var requestUri = new Uri($"{baseUrl}/{resourceLink}/docs");
    var httpRequest = new HttpRequestMessage { Method = method, RequestUri = requestUri };

    var httpResponse = await httpClient.SendAsync(httpRequest);
    await ReportOutput($"List Documents for partitionKey {partitionKey}", httpResponse);
}

async Task GetDocument(string id, string partitionKey)
{
    var method = HttpMethod.Get;
    var resourceType = ResourceType.docs;
    var resourceLink = $"dbs/{databaseName}/colls/{containerName}/docs/{id}";
    var requestDateString = DateTime.UtcNow.ToString("r");
    var auth = GenerateMasterKeyAuthorizationSignature(method, resourceType, resourceLink, requestDateString, cosmosKey);

    httpClient.DefaultRequestHeaders.Clear();
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    httpClient.DefaultRequestHeaders.Add("authorization", auth);
    httpClient.DefaultRequestHeaders.Add("x-ms-date", requestDateString);
    httpClient.DefaultRequestHeaders.Add("x-ms-version", "2018-12-31");
    httpClient.DefaultRequestHeaders.Add("x-ms-documentdb-partitionkey", $"[\"{partitionKey}\"]");

    var requestUri = new Uri($"{baseUrl}/{resourceLink}");
    var httpRequest = new HttpRequestMessage { Method = method, RequestUri = requestUri };

    var httpResponse = await httpClient.SendAsync(httpRequest);
    await ReportOutput($"Get Document by id: '{id}'", httpResponse);
}

async Task ReplaceDocument(string id, ItemDto newItem)
{
    var method = HttpMethod.Put;

    var resourceType = ResourceType.docs;
    var resourceLink = $"dbs/{databaseName}/colls/{containerName}/docs/{id}";
    var requestDateString = DateTime.UtcNow.ToString("r");
    var auth = GenerateMasterKeyAuthorizationSignature(method, resourceType, resourceLink, requestDateString, cosmosKey);

    httpClient.DefaultRequestHeaders.Clear();
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    httpClient.DefaultRequestHeaders.Add("authorization", auth);
    httpClient.DefaultRequestHeaders.Add("x-ms-date", requestDateString);
    httpClient.DefaultRequestHeaders.Add("x-ms-version", "2018-12-31");
    httpClient.DefaultRequestHeaders.Add("x-ms-documentdb-partitionkey", $"[\"{newItem.pk}\"]");

    var requestUri = new Uri($"{baseUrl}/{resourceLink}");
    var requestContent = new StringContent(JsonSerializer.Serialize(newItem), System.Text.Encoding.UTF8, "application/json");

    var httpRequest = new HttpRequestMessage { Method = method, Content = requestContent, RequestUri = requestUri };

    var httpResponse = await httpClient.SendAsync(httpRequest);
    await ReportOutput($"Replace Document with id '{id}'", httpResponse);

}

async Task PatchDocument(string id, string partitionKey)
{
    var method = HttpMethod.Patch;

    var resourceType = ResourceType.docs;
    var resourceLink = $"dbs/{databaseName}/colls/{containerName}/docs/{id}";
    var requestDateString = DateTime.UtcNow.ToString("r");
    var auth = GenerateMasterKeyAuthorizationSignature(method, resourceType, resourceLink, requestDateString, cosmosKey);

    httpClient.DefaultRequestHeaders.Clear();
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    httpClient.DefaultRequestHeaders.Add("authorization", auth);
    httpClient.DefaultRequestHeaders.Add("x-ms-date", requestDateString);
    httpClient.DefaultRequestHeaders.Add("x-ms-version", "2018-12-31");
    httpClient.DefaultRequestHeaders.Add("x-ms-documentdb-partitionkey", $"[\"{partitionKey}\"]");

    var requestUri = new Uri($"{baseUrl}/{resourceLink}");
    var requestBody = @"
{
  ""operations"": [
    {
      ""op"": ""set"",
      ""path"": ""/someProperty"",
      ""value"": ""value-patched""
    }
  ]
}  ";

    var requestContent = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

    var httpRequest = new HttpRequestMessage { Method = method, Content = requestContent, RequestUri = requestUri };

    var httpResponse = await httpClient.SendAsync(httpRequest);
    await ReportOutput($"Patch Document with id '{id}'", httpResponse);
}


async Task DeleteDocument(string id, string partitionKey)
{
    var method = HttpMethod.Delete;
    var resourceType = ResourceType.docs;
    var resourceLink = $"dbs/{databaseName}/colls/{containerName}/docs/{id}";
    var requestDateString = DateTime.UtcNow.ToString("r");
    var auth = GenerateMasterKeyAuthorizationSignature(method, resourceType, resourceLink, requestDateString, cosmosKey);

    httpClient.DefaultRequestHeaders.Clear();
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    httpClient.DefaultRequestHeaders.Add("authorization", auth);
    httpClient.DefaultRequestHeaders.Add("x-ms-date", requestDateString);
    httpClient.DefaultRequestHeaders.Add("x-ms-version", "2018-12-31");
    httpClient.DefaultRequestHeaders.Add("x-ms-documentdb-partitionkey", $"[\"{partitionKey}\"]");

    var requestUri = new Uri($"{baseUrl}/{resourceLink}");
    var httpRequest = new HttpRequestMessage { Method = method, RequestUri = requestUri };

    var httpResponse = await httpClient.SendAsync(httpRequest);

    Console.WriteLine($"Deleted item with id '{id}': {httpResponse.IsSuccessStatusCode}");
}


async Task QueryDocuments(string partitionKey)
{
    var method = HttpMethod.Post;
    var resourceType = ResourceType.docs;
    var resourceLink = $"dbs/{databaseName}/colls/{containerName}";
    var requestDateString = DateTime.UtcNow.ToString("r");
    var auth = GenerateMasterKeyAuthorizationSignature(method, resourceType, resourceLink, requestDateString, cosmosKey);

    httpClient.DefaultRequestHeaders.Clear();
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    httpClient.DefaultRequestHeaders.Add("authorization", auth);
    httpClient.DefaultRequestHeaders.Add("x-ms-date", requestDateString);
    httpClient.DefaultRequestHeaders.Add("x-ms-version", "2018-12-31");
    httpClient.DefaultRequestHeaders.Add("x-ms-documentdb-isquery", "True");

    var requestUri = new Uri($"{baseUrl}/{resourceLink}/docs");
    var requestBody = @$"
{{  
  ""query"": ""SELECT * FROM c WHERE c.pk = @pk"",  
  ""parameters"": [
    {{  
      ""name"": ""@pk"",
      ""value"": ""{partitionKey}""  
    }}
  ]  
}}";
    var requestContent = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/query+json");
    //NOTE -> this is important. CosmosDB expects a specific Content-Type with no CharSet on a query request.
    requestContent.Headers.ContentType.CharSet = ""; 
    var httpRequest = new HttpRequestMessage { Method = method, Content=requestContent, RequestUri = requestUri };

    var httpResponse = await httpClient.SendAsync(httpRequest);

    await ReportOutput("Query: ", httpResponse);
}

async Task QueryDocumentsCrossPartition()
{
    var method = HttpMethod.Post;
    var resourceType = ResourceType.docs;
    var resourceLink = $"dbs/{databaseName}/colls/{containerName}";
    var requestDateString = DateTime.UtcNow.ToString("r");
    var auth = GenerateMasterKeyAuthorizationSignature(method, resourceType, resourceLink, requestDateString, cosmosKey);

    httpClient.DefaultRequestHeaders.Clear();
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    httpClient.DefaultRequestHeaders.Add("authorization", auth);
    httpClient.DefaultRequestHeaders.Add("x-ms-date", requestDateString);
    httpClient.DefaultRequestHeaders.Add("x-ms-version", "2018-12-31");
    httpClient.DefaultRequestHeaders.Add("x-ms-max-item-count", "2");
    httpClient.DefaultRequestHeaders.Add("x-ms-documentdb-query-enablecrosspartition", "True");
    httpClient.DefaultRequestHeaders.Add("x-ms-documentdb-isquery", "True");

    var requestUri = new Uri($"{baseUrl}/{resourceLink}/docs");
    var requestBody = @$"
{{  
  ""query"": ""SELECT * FROM c"",  
  ""parameters"": []  
}}";

    var requestContent = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/query+json");
    //NOTE -> this is important. CosmosDB expects a specific Content-Type with no CharSet on a query request.
    requestContent.Headers.ContentType.CharSet = "";
    var httpRequest = new HttpRequestMessage { Method = method, Content = requestContent, RequestUri = requestUri };

    var httpResponse = await httpClient.SendAsync(httpRequest);
    //var continuation = httpResponse.Headers.GetValues("x-ms-continuation");

    await ReportOutput("Query: ", httpResponse);
}




string GenerateMasterKeyAuthorizationSignature(HttpMethod verb, ResourceType resourceType, string resourceLink, string date, string key)
{
    var keyType = "master";
    var tokenVersion = "1.0";
    var payload = $"{verb.ToString().ToLowerInvariant()}\n{resourceType.ToString().ToLowerInvariant()}\n{resourceLink}\n{date.ToLowerInvariant()}\n\n";

    var hmacSha256 = new System.Security.Cryptography.HMACSHA256 { Key = Convert.FromBase64String(key) };
    var hashPayload = hmacSha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
    var signature = Convert.ToBase64String(hashPayload);
    var authSet = WebUtility.UrlEncode($"type={keyType}&ver={tokenVersion}&sig={signature}");

    return authSet;
}

async Task ReportOutput(string methodName,HttpResponseMessage httpResponse)
{
    var responseContent = await httpResponse.Content.ReadAsStringAsync();
    if (httpResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"{methodName}: SUCCESS\n    {responseContent}\n\n");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"{methodName}: FAILED -> {(int)httpResponse.StatusCode}: {httpResponse.ReasonPhrase}.\n    {responseContent}\n\n");
        Console.ForegroundColor = ConsoleColor.White;
    }
}

record ItemDto (string id, string pk, string someProperty);

enum ResourceType
{
    dbs,
    colls,
    docs,
}

    
