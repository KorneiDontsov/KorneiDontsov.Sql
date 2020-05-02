namespace KorneiDontsov.Sql {
	using System.Data;
	using System.Threading;
	using System.Threading.Tasks;

	public static class DbProviderFunctions {
		public static ValueTask<IManagedSqlTransaction> BeginReadUncommitted
			(this IDbProvider dbProvider,
			 SqlAccess access,
			 CancellationToken cancellationToken = default) =>
			dbProvider.Begin(access, IsolationLevel.ReadUncommitted, cancellationToken);

		public static ValueTask<IManagedSqlTransaction> BeginReadCommitted
			(this IDbProvider dbProvider,
			 SqlAccess access,
			 CancellationToken cancellationToken = default) =>
			dbProvider.Begin(access, IsolationLevel.ReadCommitted, cancellationToken);

		public static ValueTask<IManagedSqlTransaction> BeginRepeatableRead
			(this IDbProvider dbProvider,
			 SqlAccess access,
			 CancellationToken cancellationToken = default) =>
			dbProvider.Begin(access, IsolationLevel.RepeatableRead, cancellationToken);

		public static ValueTask<IManagedSqlTransaction> BeginSerializable
			(this IDbProvider dbProvider,
			 SqlAccess access,
			 CancellationToken cancellationToken = default) =>
			dbProvider.Begin(access, IsolationLevel.Serializable, cancellationToken);

		public static ValueTask<IManagedRwSqlTransaction> BeginRwReadUncommitted
			(this IDbProvider dbProvider, CancellationToken cancellationToken = default) =>
			dbProvider.BeginRw(IsolationLevel.ReadUncommitted, cancellationToken);

		public static ValueTask<IManagedRwSqlTransaction> BeginRwReadCommitted
			(this IDbProvider dbProvider, CancellationToken cancellationToken = default) =>
			dbProvider.BeginRw(IsolationLevel.ReadCommitted, cancellationToken);

		public static ValueTask<IManagedRwSqlTransaction> BeginRwRepeatableRead
			(this IDbProvider dbProvider, CancellationToken cancellationToken = default) =>
			dbProvider.BeginRw(IsolationLevel.RepeatableRead, cancellationToken);

		public static ValueTask<IManagedRwSqlTransaction> BeginRwSerializable
			(this IDbProvider dbProvider, CancellationToken cancellationToken = default) =>
			dbProvider.BeginRw(IsolationLevel.Serializable, cancellationToken);

		public static ValueTask<IManagedRoSqlTransaction> BeginRoReadUncommitted
			(this IDbProvider dbProvider, CancellationToken cancellationToken = default) =>
			dbProvider.BeginRo(IsolationLevel.ReadUncommitted, cancellationToken);

		public static ValueTask<IManagedRoSqlTransaction> BeginRoReadCommitted
			(this IDbProvider dbProvider, CancellationToken cancellationToken = default) =>
			dbProvider.BeginRo(IsolationLevel.ReadCommitted, cancellationToken);

		public static ValueTask<IManagedRoSqlTransaction> BeginRoRepeatableRead
			(this IDbProvider dbProvider, CancellationToken cancellationToken = default) =>
			dbProvider.BeginRo(IsolationLevel.RepeatableRead, cancellationToken);

		public static ValueTask<IManagedRoSqlTransaction> BeginRoSerializable
			(this IDbProvider dbProvider, CancellationToken cancellationToken = default) =>
			dbProvider.BeginRo(IsolationLevel.Serializable, cancellationToken);

		public static SqlProvider Using
			(this IDbProvider dbProvider, IsolationLevel isolationLevel, SqlAccess access) =>
			new SqlProvider(dbProvider, access, isolationLevel);

		public static SqlProvider UsingRwReadUncommitted (this IDbProvider dbProvider) =>
			new SqlProvider(dbProvider, SqlAccess.Rw, IsolationLevel.ReadUncommitted);

		public static SqlProvider UsingRoReadUncommitted (this IDbProvider dbProvider) =>
			new SqlProvider(dbProvider, SqlAccess.Ro, IsolationLevel.ReadUncommitted);

		public static SqlProvider UsingRwReadCommitted (this IDbProvider dbProvider) =>
			new SqlProvider(dbProvider, SqlAccess.Rw, IsolationLevel.ReadCommitted);

		public static SqlProvider UsingRoReadCommitted (this IDbProvider dbProvider) =>
			new SqlProvider(dbProvider, SqlAccess.Ro, IsolationLevel.ReadCommitted);

		public static SqlProvider UsingRwRepeatableRead (this IDbProvider dbProvider) =>
			new SqlProvider(dbProvider, SqlAccess.Rw, IsolationLevel.RepeatableRead);

		public static SqlProvider UsingRoRepeatableRead (this IDbProvider dbProvider) =>
			new SqlProvider(dbProvider, SqlAccess.Ro, IsolationLevel.RepeatableRead);

		public static SqlProvider UsingRwSerializable (this IDbProvider dbProvider) =>
			new SqlProvider(dbProvider, SqlAccess.Rw, IsolationLevel.Serializable);

		public static SqlProvider UsingRoSerializable (this IDbProvider dbProvider) =>
			new SqlProvider(dbProvider, SqlAccess.Ro, IsolationLevel.Serializable);
	}
}
