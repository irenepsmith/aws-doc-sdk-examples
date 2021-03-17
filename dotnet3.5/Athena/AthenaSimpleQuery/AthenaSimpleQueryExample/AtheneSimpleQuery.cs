using Amazon;
using Amazon.Athena;
using Amazon.Athena.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AthenaSimpleQueryExample
{
    class AtheneSimpleQuery
    {
        private const String ATHENA_TEMP_PATH = "s3://doc-example-bucket/athena-temp/";
        private const String ATHENA_DB = "default";

        private static readonly RegionEndpoint _region = RegionEndpoint.USEast1;
        private static AmazonAthenaClient _client;

        static async Task Main()
        {
            _client = new AmazonAthenaClient();
            using (_client = new AmazonAthenaClient(Amazon.RegionEndpoint.USEast1))
            {
                QueryExecutionContext queryContext = new QueryExecutionContext();
                queryContext.Database = ATHENA_DB;
                ResultConfiguration resultConfig = new ResultConfiguration();
                resultConfig.OutputLocation = ATHENA_TEMP_PATH;

                Console.WriteLine("Created Athena Client");
                await RunQuery(_client, queryContext, resultConfig);
            }
        }

        async static Task RunQuery(IAmazonAthena client, QueryExecutionContext qContext, ResultConfiguration resConf)
        {
            /* Execute a simple query on a table */
            StartQueryExecutionRequest qReq = new StartQueryExecutionRequest()
            {
                QueryString = "SELECT * FROM cloudtrail_logs limit 10;",
                QueryExecutionContext = qContext,
                ResultConfiguration = resConf
            };

            try
            {
                // Executes the query.
                StartQueryExecutionResponse executionResponse = await client.StartQueryExecutionAsync(qReq);

                // Parse the results and return a list of key/value pairs.
                List<Dictionary<String, String>> items = await GetQueryExecution(client, executionResponse.QueryExecutionId);
                foreach (var item in items)
                {
                    foreach (KeyValuePair<String, String> pair in item)
                    {
                        Console.WriteLine($"Col: {pair.Key}");
                        Console.WriteLine($"Val: {pair.Value}");
                    }
                }
            }
            catch (InvalidRequestException e)
            {
                Console.WriteLine($"Run Error: {e.Message}");
            }
        }
        async static Task<List<Dictionary<String, String>>> GetQueryExecution(IAmazonAthena client, String id)
        {
            List<Dictionary<String, String>> items = new List<Dictionary<String, String>>();
            GetQueryExecutionResponse results = null;
            QueryExecution q = null;
            /* Declare query execution request object */
            GetQueryExecutionRequest qReq = new GetQueryExecutionRequest()
            {
                QueryExecutionId = id
            };

            // Poll API to determine when the query completed.
            do
            {
                try
                {
                    results = await client.GetQueryExecutionAsync(qReq);
                    q = results.QueryExecution;
                    Console.WriteLine($"Status: {q.Status.State}... {q.Status.StateChangeReason}");

                    //Wait for 5sec before polling again
                    await Task.Delay(5000);
                }
                catch (InvalidRequestException e)
                {
                    Console.WriteLine($"GetQueryExec Error: {e.Message}");
                }
            } while (q.Status.State == "RUNNING" || q.Status.State == "QUEUED");

            Console.WriteLine($"Data Scanned for {id}: {q.Statistics.DataScannedInBytes} Bytes");

            /* Declare query results request object */
            GetQueryResultsRequest resReq = new GetQueryResultsRequest()
            {
                QueryExecutionId = id,
                MaxResults = 10
            };

            GetQueryResultsResponse resResp = null;
            /* Page through results and request additional pages if available */
            do
            {
                resResp = await client.GetQueryResultsAsync(resReq);
                /* Loop over result set and create a dictionary with column name for key and data for value */
                foreach (Row row in resResp.ResultSet.Rows)
                {
                    Dictionary<String, String> dict = new Dictionary<String, String>();
                    for (var i = 0; i < resResp.ResultSet.ResultSetMetadata.ColumnInfo.Count; i++)
                    {
                        dict.Add(resResp.ResultSet.ResultSetMetadata.ColumnInfo[i].Name, row.Data[i].VarCharValue);
                    }
                    items.Add(dict);
                }

                if (resResp.NextToken != null)
                {
                    resReq.NextToken = resResp.NextToken;
                }
            } while (resResp.NextToken != null);

            // Return List of dictionary per row containing column name and value
            return items;
        }
    }
}
