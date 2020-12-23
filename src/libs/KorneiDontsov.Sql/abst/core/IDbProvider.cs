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

		/// <summary>
		///     Creates implementation-level database connection for unusual cases.
		/// </summary>
		/// <typeparam name = "TConnection"> Type of implementation-level database connection. </typeparam>
		/// <exception cref = "NotSupportedException">
		///     Creation of <typeparamref name = "TConnection" /> is not supported by implementation.
		/// </exception>
		TConnection CreateConnection<TConnection> () where TConnection: class =>
			throw new NotSupportedException();
	}
}
