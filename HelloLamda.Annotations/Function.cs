using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HelloLamda.Annotations;

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
    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Get, "users/{userId}")]
    public async Task<User> FunctionHandler(string userId, ILambdaContext context)
    {
        Guid.TryParse(userId, out var id);

        return await _dynamoDbContext.LoadAsync<User>(id);
    }

    [LambdaFunction]
    [HttpApi(LambdaHttpMethod.Post, "users")]
    public async Task PostFunctionHandler([FromBody]User user, ILambdaContext context)
    {
        await _dynamoDbContext.SaveAsync<User>(user);
    }

    //[LambdaFunction]
    //[HttpApi(LambdaHttpMethod.Delete, "users/{userId}")]
}
