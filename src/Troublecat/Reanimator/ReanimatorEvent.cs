namespace Troublecat.Reanimator;

public abstract class ReanimatorEvent<T> : ReanimatorEvent {
    public override void Execute(EventExecutionContext context, object data) {
        try {
            Execute(context, (T)data);
        } catch (Exception e) {

        }
    }

    public abstract void Execute(EventExecutionContext context, T data);

    private Type _cachedType = null;
    public override Type GetEventDataType() {
        if (_cachedType == null) _cachedType = typeof(T);
        return _cachedType;
    }
}

public abstract class ReanimatorEvent : IAnimationEvent {
    public virtual void Execute(EventExecutionContext context, object data) {

    }

    public abstract Type GetEventDataType();
}
