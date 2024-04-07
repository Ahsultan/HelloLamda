using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HelloLamda;

public class Function
{
    private readonly DynamoDBContext _dynamoDbContext;

    public Function()
    {
        _dynamoDbContext = new DynamoDBContext(new AmazonDynamoDBClient());
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        //return await HandleGetRequest(request);

        return request.RequestContext.Http.Method.ToUpper() switch
        {
            "GET" => await HandleGetRequest(request),
            "POST" => await HandlePostRequest(request),
            "DELETE" => await HandleDeleteRequest(request),
        };

    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandlePostRequest(APIGatewayHttpApiV2ProxyRequest request)
    { 
        var user = JsonSerializer.Deserialize<User>(request.Body);
        if(user == null)
        {
            return HandleErrors("Invalid User Details");
        }

        await _dynamoDbContext.SaveAsync(user);
        return OkRespone();
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandleDeleteRequest(APIGatewayHttpApiV2ProxyRequest request)
    {
        request.PathParameters.TryGetValue("userId", out var userIdString);
        if(Guid.TryParse(userIdString, out var userId))
        {
            await _dynamoDbContext.DeleteAsync<User>(userId);
            return OkRespone();
        }

        return HandleErrors("User Not Found");
    }

        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetRequest(APIGatewayHttpApiV2ProxyRequest request)
    {
        request.PathParameters.TryGetValue("userId", out var userIdString);
        if (Guid.TryParse(userIdString, out var userId))
        {
            var user = await _dynamoDbContext.LoadAsync<User>(userId);

            if (user != null)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = JsonSerializer.Serialize(user),
                    StatusCode = 200
                };
            }
        }

        return HandleErrors("Invalid User Id");
    }

    private static APIGatewayHttpApiV2ProxyResponse OkRespone()
    {
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            StatusCode = 200
        };
    }

    private static APIGatewayHttpApiV2ProxyResponse HandleErrors(string message)
    {
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            Body = message,
            StatusCode = 404
        };
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
