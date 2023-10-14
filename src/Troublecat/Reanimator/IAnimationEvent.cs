namespace Troublecat.Reanimator;
public interface IAnimationEvent {
    void Execute(EventExecutionContext context, object data);
}
