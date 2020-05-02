namespace KorneiDontsov.Sql.Migrations {
	using System;

	public interface IDbMigrationCollection {
		/// <param name = "migrationType"> Must implement <see cref = "IDbMigration" />. </param>
		void Add (Type migrationType, params Object[] parameters);
	}
}
