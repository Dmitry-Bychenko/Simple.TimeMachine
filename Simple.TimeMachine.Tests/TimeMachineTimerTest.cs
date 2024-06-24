namespace Simple.TimeMachine.Tests;

public sealed class TimeMachineTimerTest {

  [Fact]
  public void Dispose_Disposed() {
    var provider = new TimeMachineTimeProvider(new DateTime(2000, 1, 1));

    var calls = new List<string?>();

    var timer = provider.CreateTimer(s => calls.Add(s?.ToString()), "A", TimeSpan.Zero, TimeSpan.FromSeconds(1));
    provider.CreateTimer(s => calls.Add(s?.ToString()), "B", TimeSpan.Zero, TimeSpan.FromSeconds(2));
    provider.CreateTimer(s => calls.Add(s?.ToString()), "C", TimeSpan.Zero, TimeSpan.FromSeconds(3));

    timer.Changed += (sender, _) => {
      timer.Provider?.Timers[2]?.Dispose();

      timer.Dispose();
    };

    // Act
    provider.Adjust(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(7) });

    // Assert
    string sequence = string.Concat(calls);

    Assert.Equal("ABCABBBBB", sequence);
  }
}

