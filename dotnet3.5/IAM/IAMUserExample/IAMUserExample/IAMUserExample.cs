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
            // Create an Amazon Identity Managements Service Group.

            // Create a policy and add it to the group.

            // Create a new user.
            User readOnlyUser;
            var IAMClient = new AmazonIdentityManagementServiceClient();
            var userRequest = new CreateUserRequest
            {
                UserName = USER_NAME
            };

            readOnlyUser = await CreateNewUserAsync(IAMClient, userRequest);

            // Add the new user to the group.

            // Show that the user can now access Amazon Simple Storage Service
            // (Amazon S3) by listing the buckets on the account.
            await ListS3BucketsAsync(readOnlyUser);

            // Delete the user.
            await DeleteS3ReadOnlyUserAsync(IAMClient, readOnlyUser);
        }

        static async Task<CreateGroupResponse> CreateNewGroupAsync(
            AmazonIdentityManagementServiceClient client,
            string groupName)
        {
            var createGroupRequest = new CreateGroupRequest
            {
                GroupName = groupName
            };

            var response = await client.CreateGroupAsync(createGroupRequest);
            
        }

        static async Task<bool> AddGroupPermissionsAsync(AmazonIdentityManagementServiceClient client, Group group)
        {
            // Add appropriate permissions so the new user can access S3 on
            // a readonly basis.
            var groupPolicyRequest = new PutGroupPolicyRequest
            {
                GroupName = group.GroupName,
                PolicyName = "S3ReadOnlyAccess",
                PolicyDocument = S3_READONLY_POLICY
            };

            var response = await client.PutGroupPolicyAsync(groupPolicyRequest);
            return (response.HttpStatusCode == System.Net.HttpStatusCode.OK);
        }

        static async Task<User> CreateNewUserAsync(AmazonIdentityManagementServiceClient client, CreateUserRequest request)
        {
            try
            {
                var response = await client.CreateUserAsync(request);

                // Show the information about the user from the response.
                Console.WriteLine($"New user: {response.User.UserName} ARN = {response.User.Arn}.");
                Console.WriteLine($"{response.User.UserName} has {response.User.PermissionsBoundary}.");
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
