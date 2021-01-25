// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX - License - Identifier: Apache - 2.0

namespace EC2CRUDExample
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.EC2;
    using Amazon.EC2.Model;

    /// <summary>
    /// This application creates, cancels, and terminates an Amazon Elastic
    /// Compute Cloud (Amazon EC2) spot instance.
    /// </summary>
    class EC2CRUDExample
    {
        // Specify your AWS Region (an example Region is shown).
        private static readonly RegionEndpoint InstanceRegion = RegionEndpoint.USWest2;
        private static readonly InstanceType InstanceType = InstanceType.T1Micro;
        private static AmazonEC2Client ec2Client;

        // Default values for the optional command line arguments.
        private static string _securityGroupName = "default";
        private static string _spotPrice = "0.003";
        private static int _instanceCount = 1;

        static async Task Main(string[] args)
        {
            // Placeholder for the only required command-line arg
            string amiId = string.Empty;

            // Parse command-line args
            int i = 0;
            while (i < args.Length)
            {
                switch (args[i])
                {
                    case "-s":
                        i++;
                        _securityGroupName = args[i];
                        if (_securityGroupName == string.Empty)
                        {
                            Console.WriteLine("The security group name cannot be blank");
                            Usage();
                            return;
                        }

                        break;
                    case "-p":
                        i++;
                        _spotPrice = args[i];
                        double price;
                        double.TryParse(_spotPrice, out price);
                        if (price < 0.001)
                        {
                            Console.WriteLine("The spot price must be > 0.001");
                            Usage();
                            return;
                        }

                        break;
                    case "-c":
                        i++;
                        int.TryParse(args[i], out _instanceCount);
                        if (_instanceCount < 1)
                        {
                            Console.WriteLine("The instance count must be > 0");
                            Usage();
                            return;
                        }

                        break;
                    case "-h":
                        Usage();
                        return;
                    default:
                        amiId = args[i];
                        break;
                }

                i++;
            }

            // Make sure we have an AMI.
            if (amiId == string.Empty)
            {
                Console.WriteLine("You must supply an AMI");
                Usage();
                return;
            }

            AmazonEC2Client ec2Client = new AmazonEC2Client(region: InstanceRegion);

            Console.WriteLine("Creating spot instance request");

            SpotInstanceRequest req = await RequestSpotInstanceAsync(ec2Client, amiId, _securityGroupName, InstanceType, _spotPrice, _instanceCount);

            string id = req.SpotInstanceRequestId;

            // Wait for it to become active
            Console.WriteLine($"Waiting for spot instance request with ID {id} to become active");

            int wait = 1;
            int totalTime = 0;

            while (true)
            {
                totalTime += wait;
                Console.Write(".");

                SpotInstanceState state = await GetSpotRequestStateAsync(ec2Client, id);

                if (state == SpotInstanceState.Active)
                {
                    Console.WriteLine(string.Empty);
                    break;
                }

                // wait a bit and try again
                Thread.Sleep(wait);

                // wait longer next time
                // 1, 2, 4, ...
                wait *= 2;
            }

            // Should be around 1000 (one second)
            Console.WriteLine($"That took {totalTime} milliseconds.");

            // Cancel the request
            Console.WriteLine("Canceling spot instance request.");

            await CancelSpotRequestAsync(ec2Client, id);

            // Clean everything up
            Console.WriteLine("Terminating spot instance request.");

            await TerminateSpotInstanceAsync(ec2Client, id);

            Console.WriteLine("Done. Press enter to quit.");

            Console.ReadLine();
        }

        /// <summary>
        /// Creates an Amazon Elastic Compute Cloud (Amazon EC2) spot instance.
        /// </summary>
        /// <param name="ec2Client">The Amazon EC2 client object through which
        /// the EC2 spot instances will be created.</param>
        /// <param name="amiId">A string indicating the AMI of the Ec2 instances
        /// to request.</param>
        /// <param name="securityGroupName">A string containing the name of the
        /// security group of the requested EC2 instance.</param>
        /// <param name="instanceType">A string containing the type of EC2
        /// insance to create.</param>
        /// <param name="spotPrice">The price of the EC2 instance to requrest.</param>
        /// <param name="instanceCount">An EC2 integer representing the number
        /// of Ec2 instances to create.</param>
        /// <returns>A TaskSpotInstanceRequest object containing the value
        /// returned from the call to RequestSpotInstanceAsync.</returns>
        private static async Task<SpotInstanceRequest> RequestSpotInstanceAsync(
            AmazonEC2Client ec2Client,
            string amiId,
            string securityGroupName,
            InstanceType instanceType,
            string spotPrice,
            int instanceCount)
        {
            RequestSpotInstancesRequest request = new RequestSpotInstancesRequest
            {
                SpotPrice = spotPrice,
                InstanceCount = instanceCount,
            };

            LaunchSpecification launchSpecification = new LaunchSpecification
            {
                ImageId = amiId,
                InstanceType = instanceType,
            };

            launchSpecification.SecurityGroups.Add(securityGroupName);

            request.LaunchSpecification = launchSpecification;

            var result = await ec2Client.RequestSpotInstancesAsync(request);

            return result.SpotInstanceRequests[0];
        }

        /// <summary>
        /// Gets the state of an Amazon Elastic Compute Cloud (Amazon EC2) spot
        /// instance request.
        /// </summary>
        /// <param name="ec2Client">The Amazon EC2 client object used to
        /// request the status of a spot instance request.
        /// </param>
        /// <param name="spotRequestId">A string representing the ID of the
        /// EC2 spot instance for which you are requesting status.</param>
        /// <returns>Returns a SpotInstanceState objec that represents the
        /// current state of the sport instance.</returns>
        private static async Task<SpotInstanceState> GetSpotRequestStateAsync(
            AmazonEC2Client ec2Client,
            string spotRequestId)
        {
            // Create the describeRequest object with all of the request ids
            // to monitor (e.g. that we started).
            var request = new DescribeSpotInstanceRequestsRequest();
            request.SpotInstanceRequestIds.Add(spotRequestId);

            // Retrieve the request we want to monitor.
            var describeResponse = await ec2Client.DescribeSpotInstanceRequestsAsync(request);

            SpotInstanceRequest req = describeResponse.SpotInstanceRequests[0];

            return req.State;
        }

        /// <summary>
        /// Cancels an Amazon Elastic Compute Cloud (Amazon EC2) spot instance
        /// request.
        /// </summary>
        /// <param name="ec2Client">The Amazon EC2 client object used to
        /// request the status of a spot instance request.</param>
        /// <param name="spotRequestId">A string containing the ID of the
        /// EC2 spot instance.</param>
        private static async Task CancelSpotRequestAsync(
            AmazonEC2Client ec2Client,
            string spotRequestId)
        {
            var cancelRequest = new CancelSpotInstanceRequestsRequest();

            cancelRequest.SpotInstanceRequestIds.Add(spotRequestId);

            await ec2Client.CancelSpotInstanceRequestsAsync(cancelRequest);
        }

        /// <summary>
        /// This method terminates an existing Amazon Elastic Compute Cloud
        /// (Amazon EC2) spot sintance.
        /// </summary>
        /// <param name="ec2Client">The Amazon EC2 client object used to
        /// request the status of a spot instance request.</param>
        /// <param name="spotRequestId">A string containing the ID of the EC2
        /// spot request.</param>
        private static async Task TerminateSpotInstanceAsync(
            AmazonEC2Client ec2Client,
            string spotRequestId)
        {
            var describeRequest = new DescribeSpotInstanceRequestsRequest();
            describeRequest.SpotInstanceRequestIds.Add(spotRequestId);

            // Retrieve the request we want to monitor.
            var describeResponse = await ec2Client.DescribeSpotInstanceRequestsAsync(describeRequest);

            if (describeResponse.SpotInstanceRequests[0].State == SpotInstanceState.Active)
            {
                string instanceId = describeResponse.SpotInstanceRequests[0].InstanceId;

                var terminateRequest = new TerminateInstancesRequest();
                terminateRequest.InstanceIds = new List<string>() { instanceId };

                try
                {
                    await ec2Client.TerminateInstancesAsync(terminateRequest);
                }
                catch (AmazonEC2Exception ex)
                {
                    // Check the ErrorCode to see if the instance does not exist.
                    if (ex.ErrorCode == "InvalidInstanceID.NotFound")
                    {
                        Console.WriteLine($"Instance {instanceId} does not exist.");
                    }
                    else
                    {
                        // The exception was thrown for another reason, so re-throw the exception.
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Displays usage information on the screen if the user-supplied.
        /// parameters are either wrong or missing.
        /// </summary>
        private static void Usage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("Ec2SpotCrud.exe AMI [-s SECURITY_GROUP] [-p SPOT_PRICE] [-c INSTANCE_COUNT] [-h]");
            Console.WriteLine("  where:");
            Console.WriteLine("  AMI is the AMI to use. No default value. Cannot be an empty string.");
            Console.WriteLine("  SECURITY_GROUP is the name of a security group. Default is default. Cannot be an empty string.");
            Console.WriteLine("  SPOT_PRICE is the spot price. Default is 0.003. Must be > 0.001.");
            Console.WriteLine("  INSTANCE_COUNT is the number of instances. Default is 1. Must be > 0.");
            Console.WriteLine("  -h displays this message and quits");
            Console.WriteLine();
        }
    }
}
