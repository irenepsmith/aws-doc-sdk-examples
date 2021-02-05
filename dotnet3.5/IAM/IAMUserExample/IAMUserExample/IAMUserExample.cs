// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved. 
// SPDX-License-Identifier:  Apache-2.0

using Amazon;
using Amazon.Auth.AccessControlPolicy;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.S3;
using System;
using System.Threading.Tasks;

namespace IAMUserExample
{
    class IAMUserExample
    {
        // Indicates the regsion where the S3 bucket is located. Remember to replace
        // the value with the endpoit for your own region.
        private static readonly RegionEndpoint BUCKET_REGION = RegionEndpoint.USWest1;
        private static readonly string USER_NAME = "S3ReadOnlyUser";

        // Represents json code for AWS read-only policy for Amazon Simple
        // Storage Service (Amazon S3).
        private const string S3_READONLY_POLICY = "{" +
            "	\"Statement\" : [{" +
                "	\"Action\" : [\"s3:Get*\"]," +
                "	\"Effect\" : \"Allow\"," +
                "	\"Resource\" : \"*\"" +
            "}]" +
        "}";

        static async Task Main()
        {
            // Create a new user.
            User readOnlyUser;
            var IAMClient = new AmazonIdentityManagementServiceClient();
            var userRequest = new CreateUserRequest
            {
                UserName = USER_NAME
            };

            readOnlyUser = await CreateS3ReadOnlyUserAsync(IAMClient, userRequest);

            // Show that the user can now access an S3 bucket.
            await ListS3BucketsAsync(readOnlyUser);

            // Delete the user.
            await DeleteS3ReadOnlyUserAsync(IAMClient, readOnlyUser);
        }

        static async Task<User> CreateS3ReadOnlyUserAsync(AmazonIdentityManagementServiceClient client, CreateUserRequest request)
        {
            try
            {
                var response = await client.CreateUserAsync(request);

                // Show the information about the user from the response.
                Console.WriteLine($"New user: {response.User.UserName} ARN = {response.User.Arn}.");
                Console.WriteLine($"{response.User.UserName} has {response.User.PermissionsBoundary}.");

                // Add appropriate permissions so the new user can access S3 on
                // a readonly basis.
                var userPolicyRequest = new PutUserPolicyRequest
                {
                    UserName = response.User.UserName,
                    PolicyName = "S3ReadOnlyAccess",
                    PolicyDocument = S3_READONLY_POLICY
                };

                await client.PutUserPolicyAsync(userPolicyRequest);
                return response.User;
            }
            catch (EntityAlreadyExistsException ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        static async Task ListS3BucketsAsync(User user)
        {
            var client = new AmazonS3Client();

            // Get the list of buckets accessible by the new user.
            var response = await client.ListBucketsAsync();

            // Loop through the list and print each bucket's name
            // and creation date.
            response.Buckets
                .ForEach(b => Console.WriteLine($"Bucket name: {b.BucketName}, created on: {b.CreationDate}"));
        }

        static async Task DeleteS3ReadOnlyUserAsync(AmazonIdentityManagementServiceClient client, User user)
        {
            var request = new DeleteUserRequest()
            {
                UserName = user.UserName
            };

            await client.DeleteUserAsync(request);
        }
    }
}
