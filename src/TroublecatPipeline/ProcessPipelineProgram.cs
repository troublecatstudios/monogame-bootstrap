using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Troublecat.Data;
using Troublecat.IO;

namespace Troublecat;

internal class ProcessPipelineProgram {
    private readonly ILogger<ProcessPipelineProgram> _logger;
    private readonly IEnumerable<IResourceImporter> _importers;
    private readonly ImportConfiguration _configuration;

    public ProcessPipelineProgram(ILogger<ProcessPipelineProgram> logger, IOptions<ImportConfiguration> configuration, IEnumerable<IResourceImporter> importers) {
        _logger = logger;
        _importers = importers;
        _configuration = configuration.Value;
    }

    public async Task RunAsync() {
        _logger.LogInformation("Gathering importers.");
        var work = new List<Task>();
        foreach(var importer in _importers) {
            var attr = importer.GetType().GetCustomAttribute<ResourceImporterAttribute>();
            _logger.LogInformation($"{importer.GetType()} {{filters}}", String.Join(',', attr!.Filters));
            var absolutePath = _configuration.ResourcesBuildDirectoryAbsolute;
            foreach(var file in Paths.GetAllFilesInFolder(absolutePath, true, attr!.Filters)) {
                _logger.LogInformation($"Found: {{asset}}", file.Name);
                work.Add(importer.ImportAsync(file.FullName));
            }
        }
        await Task.WhenAll(work);
        _logger.LogInformation("Done.");
    }
}
