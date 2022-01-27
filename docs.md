
## Constructing the hashed token signature for a master token

The hash signature for the master key token can be constructed from the following parameters: **Verb**, **ResourceType**, **ResourceLink**, and **Date**.

1. The Verb represents the HTTP verb of your request. Possible values are: get, post, put, patch, delete

    **Note:**: The values must be lowercase.

1. The ResourceType portion of the string identifies the type of resource that the request is for. Possible values are: 
    - Database operations: `dbs`
    - Container operations: `colls`
    - Stored Procedures: `sprocs`
    - User Defined Functions: `udfs`
    - Triggers: `triggers`
    - Users: `users`
    - Permissions: `permissions`
    - Item level operations: `docs`
    
    **Note:** The values are case sensitive and must be lowercase.
   
1. The ResourceLink portion of the string is the identity property of the resource that the request is directed at.
 The ResourceLink value is dependent on the operation you are trying to execute. 
 Each operation will have its own corresponding ResourceLink following this convention: 
    - If the operation is performed against a specific resource then the value is the link to that resource. 
    Examples:
        - For Get Database use: `dbs/{databaseId}`
        - For Get Document use: `dbs/{databaseId}/colls/{containerId}/docs/{docId}`
        
    - If the operation is performed against a set of resources (List, Create, Query) then the value is the link of the parent resrouce. Examples: 
        - For Create Document use: `dbs/{databaseId}/colls/{containerId}`
        - For Create Stored Procedure use: `dbs/{databaseId}/colls/{containerId}`
        - For Create a Container use: `dbs/{databaseId}`
        - For Create Database use: "" -> an empty string since Databases do not have a parent resource
             
     **Note:** The resource names that are being referenced as part of the ResourceLink value are case sensitive and must match the casing of how they were declared in the database. The other components must be lowercase.

1. The Date portion of the string is the UTC date and time the message was sent (in "HTTP-date" format as defined by [RFC 7231 Date/Time Formats](http://tools.ietf.org/html/rfc7231#section-7.1.1.1)), for example, "Tue, 01 Nov 1994 08:12:31 GMT". 
    
    In C#, it can be obtained by using the "R" format specifier on the `DateTime.UtcNow` value.
    
    This same date(in same format) also needs to be passed as `x-ms-date` header in the request.
    
    **Note:** The value is case sensitive and must be all lowercase.

To compute the signature we use the SHA256 based Hash-based Message Authentication Code (HMAC) function with the CosmosDB Key as the secret.

The payload for the hashing function is based on the 4 components presented above using the following format: `"{verb}\n{resourceType}\n{resourceLink}\n{date}\n\n"` (note the extra new line at the end of the payload).

The Base64 encoded result of the function will be used as the signature when constructing the Authorization header for the call.



## Other updates to docs

#### Replace a document
    - in the Request section there is a mention of "{doc-name}" and "document name" it should be {doc-id}

#### Query Documents 
    - Update docs to mention that cosmos expects a specific Content-Type in the POST request for a query, that does not include a value for CharSet (which is added by default by HttpClient in C#)

