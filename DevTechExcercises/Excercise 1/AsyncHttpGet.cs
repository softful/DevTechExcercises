using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevTechExcercises.Excercise_1
{
    public class AsyncHttpGet
    {
        public async Task<int> GetContentLengths(List<string> urls, HttpClient client, CancellationToken cancellationToken)
        {
            var tasks = new List<Task<int>>();

            tasks.AddRange(urls.Select(url => GetContentLength(url, client, cancellationToken)));

            var allContents = await Task.WhenAll(tasks);

            return allContents.Sum();
        }

        public async Task<int> GetContentLength(string url, HttpClient client, CancellationToken cancellationToken)
        {            
            var response = await client.GetAsync(url, cancellationToken);
                        
            var content = await response.Content.ReadAsByteArrayAsync();

            return content.Length;
        }

    }
}
