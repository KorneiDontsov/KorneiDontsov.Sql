namespace KorneiDontsov.Sql.Migrations {
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	public interface ISqlTest {
		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<String?> Test (ISqlTransaction transaction, CancellationToken cancellationToken);
	}
}
