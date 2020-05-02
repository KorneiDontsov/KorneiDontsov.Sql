namespace KorneiDontsov.Sql {
	using Microsoft.AspNetCore.Builder;

	public static class EndpointFunctions {
		public static IEndpointConventionBuilder WithoutDbMigration (this IEndpointConventionBuilder epConv) =>
			epConv.WithMetadata(WithoutDbMigrationAttribute.shared);
	}
}
