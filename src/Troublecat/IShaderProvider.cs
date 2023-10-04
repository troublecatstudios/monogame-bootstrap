namespace Troublecat;
public interface IShaderProvider {
    /// <summary>
    /// Names of custom shaders that will be provided.
    /// This is expected to be placed in ./<game_directory/>/../resources
    /// </summary>
    public string[] Shaders { get; }
}
