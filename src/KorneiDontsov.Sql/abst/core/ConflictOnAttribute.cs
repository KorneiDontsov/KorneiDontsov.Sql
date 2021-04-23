namespace KorneiDontsov.Sql {
	using System;

	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public sealed class ConflictOnAttribute: Attribute, IConflictOnEndpointMetadata {
		/// <inheritdoc />
		public SqlConflict conflict { get; }

		public ConflictOnAttribute (SqlConflict conflict) => this.conflict = conflict;
	}
}
