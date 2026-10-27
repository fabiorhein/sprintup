namespace SprintUp.Shared.Infrastructure;

/// <summary>
/// Registration seam for module-owned background workers. The host owns their lifetime but the
/// modules keep their handlers and schedules; workers land with their owning tasks.
/// </summary>
public static class WorkerRegistration
{
    public static IServiceCollection AddSprintUpWorkers(this IServiceCollection services)
    {
        return services;
    }
}
