namespace EC2CRUDExampleTest
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.EC2;
    using Amazon.EC2.Model;
    using Moq;
    using Xunit;

    public class EC2CRUDExampleTest
    {
        private AmazonEC2Client CreateMockEC2Client()
        {
            /*
            AmazonEC2Client ec2Client,
            string amiId,
            string securityGroupName,
            InstanceType instanceType,
            string spotPrice,
            int instanceCount
            */
            var mockEC2Client = new Mock<AmazonEC2Client>();
            mockEC2Client.Setup(client => client.RequestSpotInstancesAsync(
                It.IsAny<RequestSpotInstancesRequest>(),
                It.IsAny<CancellationToken>()
            )).Callback<RequestSpotInstancesRequest, CancellationToken>((request, token) =>
            {
                if (!string.IsNullOrEmpty(request.SourceBucket))
                {
                    Assert.Equal(request.SourceBucket, _SourceBucket);
                }
            }).Returns((RequestSpotInstancesResponse r, CancellationToken token) =>
            {
                return Task.FromResult(new RequestSpotInstancesResponse()
                {
                    HttpStatusCode = System.Net.HttpStatusCode.OK,
                });
            });

            return mockEC2Client.Object;
        }

        [Fact]
        public async Task CheckCreateSpotInstanceAsync()
        {
            // Default values for the optional command line arguments.
            string amiId = "ami-039243a8469cc5bce";
            string securityGroupName = "default";
            string spotPrice = "0.003";
            int instanceCount = 1;

            AmazonEC2Client client = CreateMockEC2Client();

            /*
            AmazonEC2Client ec2Client,
            string amiId,
            string securityGroupName,
            InstanceType instanceType,
            string spotPrice,
            int instanceCount
            */
            RequestSpotInstancesRequest request = new(
                "ami-039243a8469cc5bce",
                securityGroupName,
                InstanceType.T1Micro,
                spotPrice,
                instanceCount
            );

            var result = await client.RequestSpotInstancesAsync(
                client,
            );

            bool gotResult = result != null;
            Assert.True(gotResult, "Copy operation failed.");

            bool ok = result.HttpStatusCode == HttpStatusCode.OK;
            Assert.True(ok, $"Could NOT copy item {_SourceObjectKey} from {_SourceBucket}.");

        }
    }
}
