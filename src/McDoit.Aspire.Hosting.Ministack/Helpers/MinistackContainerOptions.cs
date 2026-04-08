using Aspire.Hosting.ApplicationModel;

namespace McDoit.Aspire.Hosting.Ministack.Helpers;

/// <summary>
/// Put extensions in the Aspire.Hosting namespace to ease discovery as referencing
/// the Aspire hosting package automatically adds this namespace.
/// </summary>
public class MinistackContainerOptions
{
    public string? Registry { get; set; }

	public string? Image { get; set; }

	public string? Tag { get; set; }

	/// <summary>Setting port number, if set will also disable proxying.</summary>
	public int? Port { get; set; }

	public ContainerLifetime Lifetime { get; set; } = ContainerLifetime.Session;
}
