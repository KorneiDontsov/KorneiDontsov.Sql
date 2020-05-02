namespace KorneiDontsov.Sql {
	using System;
	using System.Data;
	using System.Threading;
	using System.Threading.Tasks;

	public interface IDbProvider {
		String databaseName { get; }

		String username { get; }

		Int32 defaultQueryTimeout { get; }

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		ValueTask<IManagedSqlTransaction> Begin
			(SqlAccess access,
			 IsolationLevel isolationLevel,
			 CancellationToken cancellationToken = default,
			 Int32? defaultQueryTimeout = null);

		/// <inheritdoc cref = "Begin" />
		ValueTask<IManagedRwSqlTransaction> BeginRw
			(IsolationLevel isolationLevel,
			 CancellationToken cancellationToken = default,
			 Int32? defaultQueryTimeout = null);

		/// <inheritdoc cref = "Begin" />
		ValueTask<IManagedRoSqlTransaction> BeginRo
			(IsolationLevel isolationLevel,
			 CancellationToken cancellationToken = default,
			 Int32? defaultQueryTimeout = null);
	}
}
