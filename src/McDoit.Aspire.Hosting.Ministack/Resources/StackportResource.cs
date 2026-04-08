using Aspire.Hosting.ApplicationModel;

namespace McDoit.Aspire.Hosting.Ministack.Resources
{
	public sealed class StackportResource([ResourceName] string name, MinistackResource ministackResource)
	: ContainerResource(name), IResourceWithParent<MinistackResource>
	{
		public MinistackResource Parent { get; } = ministackResource;
	}
}

