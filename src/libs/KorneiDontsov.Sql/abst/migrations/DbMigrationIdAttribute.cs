namespace KorneiDontsov.Sql.Migrations {
	using System;

	/// <inheritdoc />
	/// <summary>
	///     Specifies id of the migration that stores is database to identify migration. if not specified
	///     then migration type name is used.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
	public sealed class DbMigrationIdAttribute: Attribute {
		public String value { get; }

		public DbMigrationIdAttribute (String value) =>
			this.value = value;
	}
}
