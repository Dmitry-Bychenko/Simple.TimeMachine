namespace Simple.TimeMachine.Tests;

public sealed class TimeMachineTimeProviderTest {
  [Fact]
  public void Create_CreatedWithLocalization() {
    // Arrange 
    var time = new DateTime(2024, 6, 20);

    // Act
    var provider = new TimeMachineTimeProvider(time, TimeZoneInfo.Local);

    // Assert
    Assert.Equal(time.ToUniversalTime(), provider.UtcCurrentTime);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(15)]
  [InlineData(1_000_000_000)]
  public void Move_NoTimers_Moved(int seconds) {
    // Arrange 
    var time = new DateTime(2024, 6, 20, 0, 0, 0, DateTimeKind.Utc);

    var provider = new TimeMachineTimeProvider(time);

    // Act
    provider.Adjust(TimeSpan.FromSeconds(seconds));

    // Assert
    Assert.Equal(time.AddSeconds(seconds), provider.GetUtcNow());
  }

  [Fact]
  public void CreateTimer_ZeroOffset_TimerCreatedAndFired() {
    // Arrange
    var time = new DateTime(2024, 6, 20, 0, 0, 0, DateTimeKind.Utc);

    var provider = new TimeMachineTimeProvider(time);

    List<string?> calls = [];

    // Act
    provider.CreateTimer(item => { calls.Add(item as string); }, "A", TimeSpan.Zero, TimeSpan.Zero);
    provider.CreateTimer(item => { calls.Add(item as string); }, "B", TimeSpan.Zero, TimeSpan.FromSeconds(2));
    provider.CreateTimer(item => { calls.Add(item as string); }, "C", TimeSpan.FromSeconds(1), TimeSpan.Zero);
    provider.CreateTimer(item => { calls.Add(item as string); }, "D", TimeSpan.FromSeconds(1),
        TimeSpan.FromHours(2));

    // Assert
    Assert.Equal("A.B", string.Join('.', calls));
  }

  [Fact]
  public void CreateTimer_Moved_TimerCreatedAndFired() {
    // Arrange
    var time = new DateTime(2024, 6, 20, 0, 0, 0, DateTimeKind.Utc);

    var provider = new TimeMachineTimeProvider(time);

    List<string?> calls = [];

    // Act
    provider.CreateTimer(item => { calls.Add(item as string); }, "A", TimeSpan.FromSeconds(1), TimeSpan.Zero);
    provider.CreateTimer(item => { calls.Add(item as string); }, "B", TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(3));

    provider.Adjust(TimeSpan.FromSeconds(9));

    // Assert
    Assert.Equal("A.B.B.B", string.Join('.', calls));
  }

}

