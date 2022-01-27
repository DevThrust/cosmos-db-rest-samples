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

Console.WriteLine(accountName);

var baseUrl = $"https://{accountName}.documents.azure.com";
var httpClient = new HttpClient();


await CreateItem(new ItemDto("id1", "pk1", "value1"));
await CreateItem(new ItemDto("id2", "pk1", "value1"));

await GetItem(id: "id1", partitionKey: "pk1");
await ListItems(partitionKey:"pk1");

await DeleteItem(id: "id1", partitionKey: "pk1");
await DeleteItem(id: "id2", partitionKey: "pk1");



async Task CreateItem(ItemDto item)
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

    Console.WriteLine($"Upsert: {httpResponse.IsSuccessStatusCode}");
}

async Task DeleteItem(string id, string partitionKey)
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

    Console.WriteLine($"Delete: {httpResponse.IsSuccessStatusCode}");
}


async Task ListItems(string partitionKey)
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

    Console.WriteLine($"Query: {httpResponse.IsSuccessStatusCode}");
}

async Task GetItem(string id, string partitionKey)
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

    Console.WriteLine($"Get: {httpResponse.IsSuccessStatusCode}");
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



record ItemDto (string id, string pk, string someProperty);

enum ResourceType
{
    dbs,
    colls,
    docs,
}

    
