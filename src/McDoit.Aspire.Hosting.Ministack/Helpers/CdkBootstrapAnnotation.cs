using Aspire.Hosting.ApplicationModel;

namespace McDoit.Aspire.Hosting.Ministack.Helpers;

/// <summary>
/// Annotation that marks a Ministack resource as having CDK bootstrap configured.
/// </summary>
public sealed class CdkBootstrapAnnotation : IResourceAnnotation
{
	/// <summary>
	/// Gets the optional bootstrap qualifier passed to <c>cdk bootstrap --qualifier</c>.
	/// </summary>
	public string? Qualifier { get; }

	/// <summary>
	/// Initializes a new instance of <see cref="CdkBootstrapAnnotation"/>.
	/// </summary>
	/// <param name="qualifier">The optional CDK bootstrap qualifier.</param>
	public CdkBootstrapAnnotation(string? qualifier = null)
	{
		Qualifier = qualifier;
	}
}
