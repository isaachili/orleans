using GrainTimerDeadlock.Grains.Contracts;
using Orleans;
using Orleans.Runtime;
using System.Reflection;

namespace GrainTimerDeadlock.Grains.Implementations;

public class TestGrain : Grain, ITestGrain
{
	public static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
	private IDisposable _timer = null!;

	private IGrainContext GrainContext => (IGrainContext)typeof(Grain).GetProperty("GrainContext",
		BindingFlags.Instance | BindingFlags.NonPublic)!
		.GetValue(this)!;

	public override Task OnActivateAsync(CancellationToken cancellationToken)
	{
		// Register timer
		var interval = Interval;
		_timer = RegisterTimer(TimerCallback, null, interval, interval);

		_ = Synchronizer.Synchronizers.TryAdd(GrainContext, new Synchronizer
		{
			GrainTimer = _timer
		});

		return base.OnActivateAsync(cancellationToken);
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
		while (Synchronizer.Synchronizers.TryGetValue(GrainContext, out var synchronizer)
			&& !synchronizer.State.HasFlag(Synchronizer.States.TimerDispose))
		{

		}

		var grain = this.AsReference<ITestGrain>();
		await grain.DoSomethingAsync();
	}
}
