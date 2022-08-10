using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

// C:\Users\cblake\.awscredentials
//
//[default]
//aws_access_key_id = AKIARQYOLJLXR3A3H2NS
//aws_secret_access_key = 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
//
//NOTE: These are for user curtis, principal is arn:aws:iam::104719928047:user/curtis

namespace AWSS3BucketTest
{
    class Program
    {
        private static IConfiguration configuration;
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json");
            configuration = builder.Build();
            var regionName = configuration.GetValue<string>("AWS_REGION");
            var region = RegionEndpoint.GetBySystemName(regionName);
            var s3Client = new AmazonS3Client(region);
            await ListBuckets(s3Client);
            await UploadFile(region);
            await UploadText(s3Client);
        }
        private static async Task ListBuckets(AmazonS3Client s3Client)
        {
            var listResponse = await s3Client.ListBucketsAsync();
            Console.WriteLine($"Number of buckets: {listResponse.Buckets.Count}");
            foreach (var bucket in listResponse.Buckets)
            {
                Console.WriteLine(bucket.BucketName);
            }
        }
        private static async Task UploadText(AmazonS3Client s3Client)
        {
            var putObjectRequest = new PutObjectRequest
            {
                BucketName = "gsidestinationbucket",
                Key = "package.json",
                ContentBody = @"
{
  ""type"": ""module"",
  ""dependencies"": {
     ""@aws-sdk/client-s3"": ""^3.121.0""
  }
}
",
                ContentType = "application/json"
            };
            await s3Client.PutObjectAsync(putObjectRequest);
        }
        private static async Task UploadFile(RegionEndpoint region)
        {
            AmazonS3Uploader amazonS3 = new AmazonS3Uploader(region);
            await amazonS3.UploadFile("gsidestinationbucket", "Marshall.jpg", @"C:\Users\cblake\source\repos\Arcfield\AwsS3BucketTest\Marshall.jpg");
        }
    }
}

// Node.js version of bucket copy
//import
//{ S3Client }
//from "@aws-sdk/client-s3";
//import
//{ ListObjectsCommand }
//from "@aws-sdk/client-s3";
//var sourceBucketParams = { Bucket: "gsisourcebucket" };
//var destinationBucketParams = { Bucket: "gsidestinationbucket" };

//const s3Client = new S3Client({ region: 'us-west-1' });

//async function enumerateBucketContentsInternal(bucketParams)
//{
//    var ret = [];
//    try
//    {
//        ret.push(bucketParams.Bucket, "=".repeat(bucketParams.Bucket.length));
//        const data = await s3Client.send(new ListObjectsCommand(bucketParams));
//        data.Contents.forEach((arrayEntry) => {
//            ret.push(arrayEntry.Key);
//        }
//      );
//    }
//    catch (err)
//    {
//        ret.push(`Error: ${ err}`);
//  };
//return ret;
//};

//function run(bucketParams, callback)
//{
//    return callback(bucketParams);
//};

//var sourceResult = await run(sourceBucketParams, enumerateBucketContentsInternal);
//await sourceResult.forEach((arrayEntry) => {
//    console.log(arrayEntry);
//});
//var destinationResult = await run(destinationBucketParams, enumerateBucketContentsInternal);
//await destinationResult.forEach((arrayEntry) => {
//    console.log(arrayEntry);
//});
