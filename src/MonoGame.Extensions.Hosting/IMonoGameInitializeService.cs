namespace MonoGame.Extensions.Hosting;

public interface IMonoGameInitializeService {
    void Initialize(IServiceProvider services);
}

public interface IMonoGameInitializeScopedService {
    void Initialize(IServiceProvider services);
}
