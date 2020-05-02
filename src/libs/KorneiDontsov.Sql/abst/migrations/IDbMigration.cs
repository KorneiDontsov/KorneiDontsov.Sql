namespace KorneiDontsov.Sql.Migrations {
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	public interface IDbMigration {
		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask Exec (IRwSqlTransaction transaction, CancellationToken cancellationToken);
	}
}
