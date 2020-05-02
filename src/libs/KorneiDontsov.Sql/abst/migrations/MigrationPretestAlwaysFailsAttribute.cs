namespace KorneiDontsov.Sql.Migrations {
	using System;

	/// <inheritdoc />
	/// <summary>
	///     If specified then migration doesn't pass where preliminary <see cref = "ISqlTest.Test" />
	///     (before invocation of <see cref = "IDbMigration.Exec" />) returns no error. Otherwise,
	///     migration is pass and <see cref = "IDbMigration.Exec" /> is not invoked.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
	public sealed class MigrationPretestAlwaysFailsAttribute: Attribute { }
}
