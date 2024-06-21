using System.Globalization;

namespace Simple.TimeMachine;

/// <summary>
/// Time Machine Timer for the Timer Provider
/// </summary>
public sealed class TimeMachineTimer : ITimer {
  private TimerCallback? _callback;

  private object? _state;

  /// <summary>
  /// Time Machine Time Provider
  /// </summary>
  public TimeMachineTimeProvider? Provider { get; private set; }

  /// <summary>
  /// Timer Offset
  /// </summary>
  public TimeSpan Offset { get; private set; } = TimeSpan.FromSeconds(-1);

  /// <summary>
  /// Timer Period
  /// </summary>
  public TimeSpan Period { get; private set; } = TimeSpan.FromSeconds(-1);

  /// <summary>
  /// If instance is disposed
  /// </summary>
  public bool IsDisposed => Provider is null;

  /// <summary>
  /// If timer is enabled
  /// </summary>
  public bool Enabled {
    get {
      if (IsDisposed) 
        return false;
      
      if (Offset.Ticks < 0) 
        return false;
      
      if (Period.Ticks > 0) 
        return true;
      
      return Offset >= Provider!.GetUtcNow() - Provider!.UtcStartTime;
    }
  }

  internal TimeMachineTimer(TimeMachineTimeProvider provider, TimerCallback callback, object? state) {
    ArgumentNullException.ThrowIfNull(provider);
    ArgumentNullException.ThrowIfNull(callback);

    Provider = provider;
    _callback = callback;
    _state = state;
  }

  /// <summary>
  /// Change Timer Settings
  /// </summary>
  /// <param name="offset">Offset</param>
  /// <param name="period">Period</param>
  /// <returns>If changed</returns>
  public bool Change(TimeSpan offset, TimeSpan period) {
    if (Provider is null) 
      return false;
    
    Offset = offset;
    Period = period;

    if (Offset.Ticks >= 0 && Provider.UtcStartTime + Offset == Provider.GetUtcNow()) 
      Fire();
    
    return true;
  }

  /// <summary>
  /// To String (debug) 
  /// </summary>
  public override string ToString() {
    if (Provider is null)
      return "Disposed timer";

    if (Offset.Ticks < 0)
      return "Disabled timer";

    if (Period.Ticks <= 0)
      return $"Timer runs once at {Provider.UtcStartTime.Add(Offset).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture)}";

    return $"Timer runs periodically, starts at {Provider.UtcStartTime.Add(Offset).ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture)} with {Period.ToString("G", CultureInfo.InvariantCulture)} period";
  }

  internal void Fire() {
    if (_callback is not null) 
      _callback(_state);
  }

  internal bool NextFireTime(out DateTimeOffset time) {
    time = default;

    if (Provider is null) 
      return false;
    
    if (!Enabled) 
      return false;
    
    if (Period.Ticks <= 0) {
      time = Provider.UtcStartTime + Offset;

      if (time <= Provider.UtcCurrentTime) {
        time = default;

        return false;
      }

      return true;
    }

    long step = (Provider.GetUtcNow() - Provider.UtcStartTime - Offset).Ticks / Period.Ticks;

    if (step < 0) 
      step += 1;
    
    time = Provider.UtcStartTime + Offset + step * Period;

    if (time <= Provider.GetUtcNow()) 
      time += Period;
    
    return true;
  }

  /// <summary>
  /// Standard Dispose
  /// </summary>
  public void Dispose() {
    if (Provider is not null) {
      var oldProvider = Provider;

      Provider = null;

      oldProvider.Compress();
    }

    _state = null;
    _callback = null;
    Provider = null;
  }

  /// <summary>
  /// Standard Dispose Async
  /// </summary>
  /// <returns></returns>
  public ValueTask DisposeAsync() {
    Dispose();

    return ValueTask.CompletedTask;
  }
}