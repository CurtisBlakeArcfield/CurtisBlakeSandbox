using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSS3BucketTest
{
    public class AmazonS3Uploader
    {
        readonly RegionEndpoint region = RegionEndpoint.USEast1;
        public AmazonS3Uploader(RegionEndpoint region)
        {
            this.region = region;
        }
        public async Task UploadFile(string bucketName, string keyName, string filePath)
        {
            // bucketName = "your-amazon-s3-bucket";
            // keyName = "the-name-of-your-file";
            // filePath = "C:\\Users\\yourUserName\\Desktop\\myImageToUpload.jpg";
            var client = new AmazonS3Client(region);
            try
            {
                PutObjectRequest putRequest = new()
                {
                    BucketName = bucketName,
                    Key = keyName,
                    FilePath = filePath,
                    ContentType = "text/plain"
                };

                PutObjectResponse response = await client.PutObjectAsync(putRequest);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("Error occurred: " + amazonS3Exception.Message);
                }
            }
        }
    }
}
