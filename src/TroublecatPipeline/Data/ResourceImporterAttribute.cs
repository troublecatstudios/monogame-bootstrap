namespace Troublecat.Data;

[AttributeUsage(AttributeTargets.Class)]
public class ResourceImporterAttribute : Attribute {
    public string[] Filters { get; set; } = Array.Empty<string>();

    public ResourceImporterAttribute(params string[] filters) {
        Filters = filters;
    }
}
