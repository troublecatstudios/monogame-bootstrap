namespace Troublecat.Reanimator;

 public class EventExecutionContext {
    public AnimationFrame CurrentFrame { get; set; }
    public object Owner { get; set; }
    public IRenderer Renderer { get; set; }

    public T ResolveComponent<T>() {
        return default;
    }
}
