using Orleans;

namespace GrainTimerDeadlock.Grains.Contracts;

public interface ITestGrain : IGrainWithIntegerKey
{
	Task DeactivateOnIdleAsync();
	Task DoSomethingAsync();
	Task<Guid> DoSomethingElseAsync();
}
