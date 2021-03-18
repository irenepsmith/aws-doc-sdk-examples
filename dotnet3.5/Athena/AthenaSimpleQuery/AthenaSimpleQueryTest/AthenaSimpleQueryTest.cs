using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Moq;
using Xunit;

namespace AthenaSimpleQueryTest
{
    public class AthenaSimpleQueryTest
    {
        private const String ATHENA_TEMP_PATH = "s3://doc-example-bucket/athena-temp/";
        private const String ATHENA_DB = "default";

        private static readonly RegionEndpoint _region = RegionEndpoint.USEast1;

        [Fact]
        public async Task RunQueryTest()
        {
            // Create the mock client

            // Run the query


        }

        public async Task<List<Dictionary<String, String>>> GetQueryExecutionTest()
        {
            // Create the mock client

            // get execution results
        }
    }
}
