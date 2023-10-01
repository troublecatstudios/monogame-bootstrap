namespace Troublecat.Data;

public interface IResourceImporter {
    Task ImportAsync(string resourcePath);
}
