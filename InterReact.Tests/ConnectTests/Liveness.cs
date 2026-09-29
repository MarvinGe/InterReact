namespace ConnectTests;

public class Liveness(ITestOutputHelper output) : OutputHelperTestBase(output, LogLevel.Debug)
{
    /// <summary>
    /// Login arms a three second cancellation on the connection-wide token source. If it is not disarmed
    /// again, the connection cancels itself shortly after startup and every later request fails.
    /// </summary>
    [Fact]
    public async Task SurvivesLoginTimerTestAsync()
    {
        IInterReactClient client = await InterReactClient.CreateAsync(options =>
            options.LogFactory = LogFactory, TestContext.Current.CancellationToken);
        try
        {
            Assert.True(client.IsConnected);

            await Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.True(client.IsConnected);
            await client.Request.RequestCurrentTimeAsync();
        }
        finally
        {
            await client.DisposeAsync();
        }

        Assert.False(client.IsConnected);
    }
}
