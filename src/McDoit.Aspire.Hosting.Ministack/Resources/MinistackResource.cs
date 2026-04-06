using Amazon;
using Aspire.Hosting.ApplicationModel;

namespace McDoit.Aspire.Hosting.Ministack.Resources
{
	public sealed class MinistackResource([ResourceName] string name)
	: ContainerResource(name), IResourceWithConnectionString, IResourceWithEnvironment, IResourceWithWaitSupport
	{
		internal const string HttpEndpointName = "http";

		// An EndpointReference is a core Aspire type used for keeping
		// track of endpoint details in expressions. Simple literal values cannot
		// be used because endpoints are not known until containers are launched.
		private EndpointReference? _httpReference;

		public EndpointReference HttpEndpoint =>
			_httpReference ??= new(this, HttpEndpointName);

		// Required property on IResourceWithConnectionString. Represents a connection
		// string that applications can use to access the MailDev server. In this case
		// the connection string is composed of the SmtpEndpoint endpoint reference.
		public ReferenceExpression ConnectionStringExpression =>
			ReferenceExpression.Create(
				$"http://{HttpEndpoint.Property(EndpointProperty.HostAndPort)}"
			);

		/// <summary>
		/// The AWS region configured for this Ministack instance.
		/// </summary>
		public required RegionEndpoint Region { get; set; }
	}
}

