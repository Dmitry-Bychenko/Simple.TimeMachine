using System.Globalization;

namespace Simple.TimeMachine;

/// <summary>
/// Time Machine Time Provider
/// </summary>
public sealed class TimeMachineTimeProvider : TimeProvider {
  private DateTimeOffset _utcCurrentTime;

  private readonly List<TimeMachineTimer> _timers = [];

  private bool _changingTime;

  /// <summary>
  /// Start Time
  /// </summary>
  public DateTimeOffset UtcStartTime { get; }

  /// <summary>
  /// Current Time
  /// </summary>
  public DateTimeOffset UtcCurrentTime {
    get => GetUtcNow();
    set => ChangeTime(value.ToUniversalTime());
  }

  /// <summary>
  /// Duration
  /// </summary>
  public TimeSpan Duration {
    get => _utcCurrentTime - UtcStartTime;
    set {
      if (UtcStartTime + value < _utcCurrentTime)
        throw new ArgumentOutOfRangeException(nameof(value), "Backward movement is not allowed");

      ChangeTime(UtcStartTime + value);
    }
  }

  /// <summary>
  /// Local Time Zone
  /// </summary>
  public override TimeZoneInfo LocalTimeZone { get; }

  /// <summary>
  /// Frequency (1 nanosecond)
  /// </summary>
  public override long TimestampFrequency => 1_000_000_000L;

  /// <summary>
  /// All timers
  /// </summary>
  public IReadOnlyList<TimeMachineTimer> Timers => _timers;

  /// <summary>
  /// Standard Constructor
  /// </summary>
  /// <param name="startTime">Start Up Time</param>
  /// <param name="localTimeZone">Time Zone (UTC by default)</param>
  public TimeMachineTimeProvider(DateTimeOffset startTime, TimeZoneInfo? localTimeZone = default) {
    UtcStartTime = startTime.ToUniversalTime();
    LocalTimeZone = localTimeZone ?? TimeZoneInfo.Utc;

    _utcCurrentTime = UtcStartTime;
  }

  /// <summary>
  /// Standard Constructor
  /// </summary>
  /// <param name="localTimeZone">Time Zone (UTC by default)</param>
  public TimeMachineTimeProvider(TimeZoneInfo? localTimeZone = default) : this(DateTimeOffset.UtcNow, localTimeZone) {
  }

  /// <summary>
  /// Current time stamp in nanoseconds
  /// </summary>
  /// <returns>Nanoseconds from Start Time</returns>
  public override long GetTimestamp() => (_utcCurrentTime.Ticks - UtcStartTime.Ticks) * 100L;

  /// <summary>
  /// Current Time (UTC)
  /// </summary>
  /// <returns>Current Time in UTC</returns>
  public override DateTimeOffset GetUtcNow() => _utcCurrentTime;

  /// <summary>
  /// Create Fake Timer
  /// </summary>
  /// <param name="callback">Timer callback</param>
  /// <param name="state">State to pass into the callback</param>
  /// <param name="offset">Offset</param>
  /// <param name="period">Period</param>
  /// <returns>Faked Timer</returns>
  public override TimeMachineTimer CreateTimer(TimerCallback callback, object? state, TimeSpan offset, TimeSpan period) {
    ArgumentNullException.ThrowIfNull(callback);

    var result = new TimeMachineTimer(this, callback, state);

    _timers.Add(result);

    result.Change(offset, period);

    return result;
  }

  /// <summary>
  /// Move
  /// </summary>
  /// <param name="period">Period to Add</param>
  /// <exception cref="ArgumentOutOfRangeException">When trying to move backward</exception>
  public void Adjust(TimeSpan period) {
    if (period.Ticks < 0)
      throw new ArgumentOutOfRangeException(nameof(period), "Time can't be moved backward");

    if (period.Ticks == 0)
      return;

    ChangeTime(GetUtcNow() + period);
  }

  /// <summary>
  /// Move
  /// </summary>
  /// <param name="exactTime">Exact Time to Move</param>
  /// <exception cref="ArgumentOutOfRangeException">When trying to move backward</exception>
  public void Adjust(DateTimeOffset exactTime) {
    exactTime = exactTime.ToUniversalTime();

    if (exactTime < GetUtcNow())
      throw new ArgumentOutOfRangeException(nameof(exactTime), "Time can't be moved backward");

    ChangeTime(exactTime);
  }

  /// <summary>
  /// To String representation
  /// </summary>
  public override string ToString() => GetUtcNow().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);

  private void ChangeTime(DateTimeOffset time) {
    time = time.ToUniversalTime();

    if (time == _utcCurrentTime)
      return;

    if (time < _utcCurrentTime)
      throw new ArgumentOutOfRangeException(nameof(time), "Time Provider can't move to the past");

    // Scan and fire all timers
    List<TimeMachineTimer> timersToFire = [];

    while (true) {
      timersToFire.Clear();

      DateTimeOffset nearest = default;

      foreach (TimeMachineTimer timer in _timers)
        if (timer.NextFireTime(out var fireTime) && fireTime <= time) {
          if (timersToFire.Count == 0 || nearest == fireTime) {
            nearest = fireTime;
            timersToFire.Add(timer);
          }
          else if (timersToFire.Count > 0 && fireTime < nearest) {
            timersToFire.Clear();

            nearest = fireTime;
            timersToFire.Add(timer);
          }
        }

      if (timersToFire.Count <= 0)
        break;

      _utcCurrentTime = nearest;

      var savedChangingTime = _changingTime;

      try {
        _changingTime = true;

        foreach (var timer in timersToFire)
          timer.Fire();
      }
      finally {
        _changingTime = savedChangingTime;
      }

      Compress();
    }

    _utcCurrentTime = time;
  }

  internal void Compress() {
    if (_changingTime)
      return;

    int left = 0;

    for (int i = 0; i < _timers.Count; ++i)
      if (!_timers[i].IsDisposed)
        _timers[left++] = _timers[i];

    _timers.RemoveRange(left, _timers.Count - left);
  }
}