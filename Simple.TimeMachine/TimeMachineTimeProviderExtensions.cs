namespace Simple.TimeMachine;

/// <summary>
/// TimeMachine Time Provider extensions
/// </summary>
public static class TimeMachineTimeProviderExtensions
{
    /// <summary>
    /// Move Several Steps
    /// </summary>
    /// <param name="provider">Time Provider</param>
    /// <param name="steps">Steps</param>
    public static void Adjust(this TimeMachineTimeProvider provider, IEnumerable<TimeSpan> steps)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(steps);

        foreach (var step in steps)
            provider.Move(step);
    }

    /// <summary>
    /// Move Several Steps
    /// </summary>
    /// <param name="provider">Time Provider</param>
    /// <param name="exactTimes">Exact Time Moments to moves</param>
    public static void Adjust(this TimeMachineTimeProvider provider, IEnumerable<DateTimeOffset> exactTimes)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(exactTimes);

        foreach (var step in exactTimes.OrderBy(item => item))
            provider.Move(step);
    }

    /// <summary>
    /// Move Several Steps
    /// </summary>
    /// <param name="provider">Time Provider</param>
    /// <param name="exactTimes">Exact Time Moments to moves</param>
    public static void Adjust(this TimeMachineTimeProvider provider, IEnumerable<DateTime> exactTimes)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(exactTimes);

        foreach (var step in exactTimes.OrderBy(item => item))
            provider.Move(step);
    }
}