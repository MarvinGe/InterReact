using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
namespace ConnectTests;

public class Liveness(ITestOutputHelper output) : OutputHelperTestBase(output, LogLevel.Debug)
{
    /// <summary>
    /// Login arms a three second cancellation on the connection-wide token source. If it is not
    /// disarmed again, the connection cancels itself shortly after login and is unusable from then on.
    /// </summary>
    [Fact]
    public async Task SurvivesLoginTimerTestAsync()
    {
        IInterReactClient client = await InterReactClient.CreateAsync(options =>
            options.LogFactory = LogFactory, TestContext.Current.CancellationToken);
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            // Sending covers the outgoing channel, receiving covers the pipeline: both run on the
            // token the timer would have cancelled.
            Task<CurrentTime> currentTime = client
                .Response
                .OfType<CurrentTime>()
                .Timeout(TimeSpan.FromSeconds(5))
                .FirstAsync()
                .ToTask(TestContext.Current.CancellationToken);

            await client.Request.RequestCurrentTimeAsync();

            Assert.True((await currentTime).Seconds > 0);
        }
        finally
        {
            await client.DisposeAsync();
        }
    }
}
