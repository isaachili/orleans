using GrainTimerDeadlock.Grains.Contracts;
using Orleans;
using Orleans.Runtime;

namespace GrainTimerDeadlock.Grains.Implementations;

public class TestGrain : Grain, ITestGrain
{
	public static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
	private IDisposable _timer = null!;

	private long TestGrainId { get; set; }

	public override Task OnActivateAsync()
	{
		// Register timer
		var interval = Interval;
		_timer = RegisterTimer(TimerCallback, null, interval, interval);

		TestGrainId = this.GetPrimaryKeyLong();
		_ = Synchronizer.Synchronizers.TryAdd((typeof(TestGrain), TestGrainId), new Synchronizer
		{
			GrainTimer = _timer
		});

		return base.OnActivateAsync();
	}

	public Task DeactivateOnIdleAsync()
	{
		DeactivateOnIdle();
		return Task.CompletedTask;
	}

	public async Task DoSomethingAsync()
	{
		await Task.Delay(TimeSpan.FromSeconds(5));
	}

	public Task<Guid> DoSomethingElseAsync()
	{
		return Task.FromResult(Guid.NewGuid());
	}

	private async Task TimerCallback(object? state = null)
	{
		while (Synchronizer.Synchronizers.TryGetValue((typeof(TestGrain), TestGrainId), out var synchronizer)
			&& !synchronizer.State.HasFlag(Synchronizer.States.TimerDispose))
		{

		}

		var grain = this.AsReference<ITestGrain>();
		await grain.DoSomethingAsync();
	}
}
