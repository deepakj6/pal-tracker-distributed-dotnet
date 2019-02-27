using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Allocations
{
    public class ProjectClient : IProjectClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<ProjectClient> _logger;
        private readonly IDictionary<long, ProjectInfo> _projectInfoCache;

        public ProjectClient(HttpClient client, ILogger<ProjectClient> logger)
        {
            _client = client;
            _logger = logger;
            _projectInfoCache = new Dictionary<long, ProjectInfo>();
        }

        public async Task<ProjectInfo> DoGet(long projectId) {
            _client.DefaultRequestHeaders.Accept.Clear();
            var streamTask = _client.GetStreamAsync($"project?projectId={projectId}");
            _logger.LogInformation($"Attempting to fetch projectId: {projectId}");

            var serializer = new DataContractJsonSerializer(typeof(ProjectInfo));
            var projectInfo = serializer.ReadObject(await streamTask) as ProjectInfo;

            _projectInfoCache.Add(projectId, projectInfo);
            _logger.LogInformation($"Caching projectId: {projectId}");

            return projectInfo;
        }

        public Task<ProjectInfo> DoGetFromCache(long projectId) {
            _logger.LogInformation($"Retrieving from cache projectId: {projectId}");
            return Task.FromResult(_projectInfoCache[projectId]);
        }

        public async Task<ProjectInfo> Get(long projectId)
        {         
            var command = new GetProjectCommand(DoGet,DoGetFromCache,projectId);
            return await command.ExecuteAsync();

        }
    }
}