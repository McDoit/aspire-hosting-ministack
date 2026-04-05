// Put extensions in the Aspire.Hosting namespace to ease discovery as referencing
// the Aspire hosting package automatically adds this namespace.
using Aspire.Hosting.ApplicationModel;

namespace McDoit.Aspire.Hosting.Ministack;

public class MinistackContainerOptions
{
    public string? Registry { get; set; }

	public string? Image { get; set; }

	public string? Tag { get; set; }

	//Setting port number, if set will also disable proxying
	public int? Port { get; set; }

	public ContainerLifetime Lifetime { get; set; } = ContainerLifetime.Session;

	/// <summary>
	/// Sets the Redis host for state persistence via <c>REDIS_HOST</c>.
	/// When set, MiniStack uses Redis to persist state across container restarts
	/// instead of the local filesystem.
	/// </summary>
	public string? RedisHost { get; set; }

	/// <summary>
	/// Sets a custom AWS account ID via <c>MINISTACK_ACCOUNT_ID</c>.
	/// Defaults to <c>000000000000</c> when not specified.
	/// </summary>
	public string? AccountId { get; set; }
}
