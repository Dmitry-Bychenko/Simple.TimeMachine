namespace Simple.TimeMachine.Tests;

public sealed class TimeMachineTimeProviderExtensionsTest {
  [Fact]
  public void Adjust_Spans_TimersAreCalled() {
    // Arrange
    var provider = new TimeMachineTimeProvider(new DateTime(2000, 1, 1));

    var calls = new List<string?>();

    provider.CreateTimer(s => calls.Add(s?.ToString()), "A", TimeSpan.Zero, TimeSpan.FromSeconds(1));
    provider.CreateTimer(s => calls.Add(s?.ToString()), "B", TimeSpan.Zero, TimeSpan.FromSeconds(2));
    provider.CreateTimer(s => calls.Add(s?.ToString()), "C", TimeSpan.Zero, TimeSpan.FromSeconds(3));

    // Act
    provider.Adjust(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(7) });

    // Assert
    string sequence = string.Concat(calls);

    Assert.Equal("ABCAABACABAABCAABACABA", sequence);
  }

  [Fact]
  public void Adjust_TimeOffsets_TimersAreCalled() {
    // Arrange
    var at = new DateTime(2000, 1, 1);

    var provider = new TimeMachineTimeProvider(at);

    var calls = new List<string?>();

    provider.CreateTimer(s => calls.Add(s?.ToString()), "A", TimeSpan.Zero, TimeSpan.FromSeconds(1));
    provider.CreateTimer(s => calls.Add(s?.ToString()), "B", TimeSpan.Zero, TimeSpan.FromSeconds(2));
    provider.CreateTimer(s => calls.Add(s?.ToString()), "C", TimeSpan.Zero, TimeSpan.FromSeconds(3));

    // Act
    provider.Adjust(new[] { at.AddSeconds(1), at.AddSeconds(4), at.AddSeconds(11) });

    // Assert
    string sequence = string.Concat(calls);

    Assert.Equal("ABCAABACABAABCAABACABA", sequence);
  }

  [Fact]
  public void Adjust_Times_TimersAreCalled() {
    // Arrange
    var at = new DateTime(2000, 1, 1);

    var provider = new TimeMachineTimeProvider(at);

    var calls = new List<string?>();

    provider.CreateTimer(s => calls.Add(s?.ToString()), "A", TimeSpan.Zero, TimeSpan.FromSeconds(1));
    provider.CreateTimer(s => calls.Add(s?.ToString()), "B", TimeSpan.Zero, TimeSpan.FromSeconds(2));
    provider.CreateTimer(s => calls.Add(s?.ToString()), "C", TimeSpan.Zero, TimeSpan.FromSeconds(3));

    // Act
    provider.Adjust(new[] { at.AddSeconds(1), at.AddSeconds(4), at.AddSeconds(11) });

    // Assert
    string sequence = string.Concat(calls);

    Assert.Equal("ABCAABACABAABCAABACABA", sequence);
  }
}