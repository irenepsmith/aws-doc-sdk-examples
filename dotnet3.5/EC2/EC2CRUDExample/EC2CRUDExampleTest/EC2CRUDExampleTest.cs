using System.Threading.Tasks;
using Moq;
using Xunit;

namespace EC2CRUDExampleTest
{
    public class EC2CRUDExampleTest
    {
        [Fact]
        public async Task TestCreateSpotInstanceAsync()
        {
            // Default values for the optional command line arguments.
            string amiId = "ami-039243a8469cc5bce";
            string securityGroupName = "default";
            string spotPrice = "0.003";
            int instanceCount = 1;

            //AmazonEC2Client client = CreateMockEC2Client();

            /*
            AmazonEC2Client ec2Client,
            string amiId,
            string securityGroupName,
            InstanceType instanceType,
            string spotPrice,
            int instanceCount
            */
            //RequestSpotInstancesRequest request = new(
            //    "ami-039243a8469cc5bce",
            //    securityGroupName,
            //    InstanceType.T1Micro,
            //    spotPrice,
            //    instanceCount
            //);

            //var result = await client.RequestSpotInstancesAsync(
            //    client
            //);

            //bool gotResult = result != null;
            //Assert.True(gotResult, "Copy operation failed.");

            //bool ok = result.HttpStatusCode == HttpStatusCode.OK;
            //Assert.True(ok, $"Could NOT copy item {_SourceObjectKey} from {_SourceBucket}.");
        }

        [Fact]
        public async Task TestGetSpotRequestStateAsync()
        {
            // Create the mock client object.

            // Call GetSpotRequestStateAsync

            // Handle the return value.
        }

        [Fact]
        public async Task TestCancelSpotRequestAsync()
        {
            // Create the mock client object.

            // Call GetSpotRequestStateAsync

            // Handle the return value.
        }

        [Fact]
        public async Task TestTerminateInstancesAsync()
        {
            // Create the mock client object.

            // Call GetSpotRequestStateAsync

            // Handle the return value.
        }

/*        private AmazonEC2Client CreateMockEC2Client()
        {
            *//*
            AmazonEC2Client ec2Client,
            string amiId,
            string securityGroupName,
            InstanceType instanceType,
            string spotPrice,
            int instanceCount
            *//*
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
*/    }
}
