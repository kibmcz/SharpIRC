using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web;

namespace Cognize.GoogleSearchAPIIntegration {
    // Google search rest API developer documentation: http://code.google.com/apis/ajaxsearch/documentation/
    // Info on JSON deserialization in .NET at http://msdn.microsoft.com/en-us/library/bb412179.aspx
    // Info on JSON object notation at http://www.json.org/

    public class GoogleSearch {
        /// <summary>
        /// Get google search results from the whole web. Google documentation suggests that the maximum number
        /// of results that can be requested without throwing an exception are 32. In testing, using large chunks,
        /// upto 64 results have been acheived. Any number above 64 is amended to 64.
        /// </summary>
        /// <param name="searchString">The raw search string.</param>
        /// <param name="requestedResults">The number of results requested. </param>
        /// <returns>The number of search results to return.</returns>
        public static SortedList<int, Results> GetSearchResults(string searchString, int maxRequestedResults) { return GetSearchResults(null, searchString, maxRequestedResults); }

        /// <summary>
        /// Get google search results from a specific domain. Google documentation suggests that the maximum number
        /// of results that can be requested without throwing an exception are 32. In testing, using large chunks,
        /// upto 64 results have been acheived. Any number above 64 is amended to 64.
        /// </summary>
        /// <param name="siteName">The fully qualified site name url.</param>
        /// <param name="searchString">The raw search string.</param>
        /// <param name="requestedResults">The number of results requested. </param>
        /// <returns>The number of search results to return.</returns>
        public static SortedList<int, Results> GetSearchResults(string siteName, string searchString, int requestedResults) {
            var searchResultsLst = new SortedList<int, Results>();

            // API will error if we request more than 64 results,
            // so modifiy if over that figure
            if (requestedResults > 64) requestedResults = 64;

            int maxChunksRequired = CalculateChunksRequired(requestedResults);

            for (int chunkIndx = 0; chunkIndx < maxChunksRequired; chunkIndx++) {
                // Results are returned in sets of 8
                // so we request results from the start of
                // the next group of 8.
                int chunkResultCountStart = (chunkIndx*8);

                // Return the response data including chunk of search results
                ResponseData responseData = GetSearchResultsChunk(siteName, searchString, chunkResultCountStart);

                // For some search terms, the max no of requested results will be higher
                // than the actual number of results google has. This is determined
                // by esitmatedResultCount value returned by the API. In this case we need
                // to reduce the number of API calls, since superflous calls will result
                // in the results being duplicated where google returns repeats the results despite
                // the value passed in chunkResultCountStart

                if (responseData == null) continue;
                if (chunkIndx == 0) {
                    int realChunksRequired = CalculateChunksRequired(responseData.cursor.estimatedResultCount);

                    if (maxChunksRequired > realChunksRequired) maxChunksRequired = realChunksRequired;
                }

                // Put the results in a more manageable sorted list
                foreach (Results t in responseData.results) searchResultsLst.Add(searchResultsLst.Count, t);
            }

            return searchResultsLst;
        }

        /// <summary>
        /// Given a requested number of results, this method works
        /// out how many result requests will be required to the 
        /// Google search API.
        /// </summary>
        /// <param name="requestedResults"></param>
        /// <returns></returns>
        private static int CalculateChunksRequired(int requestedResults) {
            int chunksRequired = (requestedResults/8);

            // If remainder exists, add a final chunk
            if ((requestedResults%8) > 0) chunksRequired = chunksRequired + 1;

            return chunksRequired;
        }

        /// <summary>
        /// Get google search results for a particular site
        /// </summary>
        /// <param name="siteName">The fully qualified site root path for the site that results are to be limited to. E.g. http://www.cognize.co.uk</param>
        /// <param name="searchString">The raw (non encoded) search string</param>
        /// <returns></returns>
        public static ResponseData GetSearchResultsChunk(string siteName, string searchString, int resultCountStartPos) {
            ResponseData responseData = null;

            try {
                searchString = HttpUtility.UrlEncode(searchString.Trim());

                using (var client = new WebClient()) {
                    // Manipulate request headers - Google rest API requires valid result header
                    // hence the use of Web client as opposed to WebRequest
                    client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                    string siteSearchString = String.Empty;

                    if (!String.IsNullOrEmpty(siteName)) {
                        // Param name and value must include no spaces
                        siteSearchString = "site:" + siteName + " ";
                    }

                    // Result size is rsz in query string. This parameter causes the
                    // api to return results in sets of 8 where 'large' is used
                    // rather than 4 if 'small' is used, so less total api requests are required.

                    string resultSize = "large";

                    string searchRequestURL = "http://ajax.googleapis.com/ajax/services/search/web?v=1.0&start=" + resultCountStartPos + "&rsz=" + resultSize + "&q=" + siteSearchString + searchString;

                    var jsonSerializer = new DataContractJsonSerializer(typeof (GoogleAPISearchResults));

                    // Read our search results into a .net object
                    var searchResultsObj = (GoogleAPISearchResults) jsonSerializer.ReadObject(client.OpenRead(searchRequestURL));

                    responseData = searchResultsObj.responseData;
                }
            } catch (Exception ex) {
                // Log error here

                // Allow exception to bubble up

                throw ex;
            }

            // Return response data including search results

            return responseData;
        }
    }
}
