namespace Troublecat.Data;
public interface IImporterRegistry {
    IResourceImporter[] GatherImporters();
}
