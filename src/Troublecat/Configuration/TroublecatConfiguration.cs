using Troublecat.IO;

namespace Troublecat.Configuration;

[Serializable]
public class TroublecatConfiguration {
    public string AtlasFolderName { get; set; } = "atlases";
    public string ResourcesBuildDirectory { get; set; } = "resources";
    public string ResourcesPackDirectory { get; set; } = "packed";
    public string ResourcesBuildDirectoryAbsolute => Paths.GetPath(ResourcesBuildDirectory);
    public string ResourcesPackDirectoryAbsolute => Paths.GetPath(ResourcesPackDirectory);
}
