namespace McDoit.Aspire.Hosting.Ministack.Tests.Fixture;

[CollectionDefinition("Sample CDK App collection")]
public class SampleCDKAppCollection : ICollectionFixture<CdkBootstrapLiveFixture>
{
	// This class has no code, and is never created. Its purpose is simply
	// to be the place to apply [CollectionDefinition] and all the
	// ICollectionFixture<> interfaces.
}
