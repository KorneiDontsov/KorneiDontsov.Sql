namespace KorneiDontsov.Sql {
	using System.Data;
	using System.Threading;
	using System.Threading.Tasks;

	public static class DbProviderFunctions {
		public static ValueTask<IManagedSqlTransaction> BeginReadUncommitted
			(this IDbProvider dbProvider,
			 SqlAccess? access = null,
			 CancellationToken cancellationToken = default) =>
			dbProvider.Begin(IsolationLevel.ReadUncommitted, cancellationToken, access);

		public static ValueTask<IManagedSqlTransaction> BeginReadCommitted
			(this IDbProvider dbProvider,
			 SqlAccess? access = null,
			 CancellationToken cancellationToken = default) =>
			dbProvider.Begin(IsolationLevel.ReadCommitted, cancellationToken, access);

		public static ValueTask<IManagedSqlTransaction> BeginRepeatableRead
			(this IDbProvider dbProvider,
			 SqlAccess? access = null,
			 CancellationToken cancellationToken = default) =>
			dbProvider.Begin(IsolationLevel.RepeatableRead, cancellationToken, access);

		public static ValueTask<IManagedSqlTransaction> BeginSerializable
			(this IDbProvider dbProvider,
			 SqlAccess? access = null,
			 CancellationToken cancellationToken = default) =>
			dbProvider.Begin(IsolationLevel.Serializable, cancellationToken, access);

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
			(this IDbProvider dbProvider, IsolationLevel isolationLevel, SqlAccess? access = null) =>
			new(dbProvider, isolationLevel, access);

		public static SqlProvider UsingRwReadUncommitted (this IDbProvider dbProvider) =>
			new(dbProvider, IsolationLevel.ReadUncommitted, SqlAccess.Rw);

		public static SqlProvider UsingRoReadUncommitted (this IDbProvider dbProvider) =>
			new(dbProvider, IsolationLevel.ReadUncommitted, SqlAccess.Ro);

		public static SqlProvider UsingRwReadCommitted (this IDbProvider dbProvider) =>
			new(dbProvider, IsolationLevel.ReadCommitted, SqlAccess.Rw);

		public static SqlProvider UsingRoReadCommitted (this IDbProvider dbProvider) =>
			new(dbProvider, IsolationLevel.ReadCommitted, SqlAccess.Ro);

		public static SqlProvider UsingRwRepeatableRead (this IDbProvider dbProvider) =>
			new(dbProvider, IsolationLevel.RepeatableRead, SqlAccess.Rw);

		public static SqlProvider UsingRoRepeatableRead (this IDbProvider dbProvider) =>
			new(dbProvider, IsolationLevel.RepeatableRead, SqlAccess.Ro);

		public static SqlProvider UsingRwSerializable (this IDbProvider dbProvider) =>
			new(dbProvider, IsolationLevel.Serializable, SqlAccess.Rw);

		public static SqlProvider UsingRoSerializable (this IDbProvider dbProvider) =>
			new(dbProvider, IsolationLevel.Serializable, SqlAccess.Ro);
	}
}
