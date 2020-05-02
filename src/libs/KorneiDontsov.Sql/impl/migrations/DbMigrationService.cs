namespace KorneiDontsov.Sql.Migrations {
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;
	using static System.String;

	class DbMigrationService: IHostedService, IDisposable {
		DbMigrationState state { get; }
		IDbProvider dbProvider { get; }
		IDbMigrationProvider dbMigrationProvider { get; }
		IDbMigrationPlan migrationPlan { get; }
		IHostApplicationLifetime appLifetime { get; }
		IServiceProvider serviceProvider { get; }
		ILogger logger { get; }

		public DbMigrationService
			(DbMigrationState state,
			 IDbProvider dbProvider,
			 IDbMigrationProvider dbMigrationProvider,
			 IDbMigrationPlan migrationPlan,
			 IHostApplicationLifetime appLifetime,
			 IServiceProvider serviceProvider,
			 ILogger<DbMigrationService> logger) {
			this.state = state;
			this.dbProvider = dbProvider;
			this.dbMigrationProvider = dbMigrationProvider;
			this.migrationPlan = migrationPlan;
			this.appLifetime = appLifetime;
			this.serviceProvider = serviceProvider;
			this.logger = logger;
		}

		class MigrationDescriptor {
			public Type type { get; }
			public Object[] parameters { get; }
			public Int32 index { get; }
			public String id { get; }
			public Boolean hasTest { get; }
			public Boolean pretestAlwaysFails { get; }

			/// <param name = "type"> Must implement <see cref = "IDbMigration" />. </param>
			public MigrationDescriptor (Type type, Object[] parameters, Int32 index) {
				this.type = type;
				this.parameters = parameters;
				this.index = index;
				id = type.GetCustomAttribute<DbMigrationIdAttribute>()?.value ?? type.Name;
				hasTest = typeof(ISqlTest).IsAssignableFrom(type);
				pretestAlwaysFails = Attribute.IsDefined(type, typeof(MigrationPretestAlwaysFailsAttribute));
			}
		}

		class DbMigrationCollection: IDbMigrationCollection {
			public List<MigrationDescriptor> descriptors { get; } =
				new List<MigrationDescriptor>();

			HashSet<Type> migrationTypeSet { get; } =
				new HashSet<Type>();

			Dictionary<String, Type> migrationTypesByIds { get; } =
				new Dictionary<String, Type>();

			/// <inheritdoc />
			public void Add (Type migrationType, params Object[] parameters) {
				var descriptor =
					typeof(IDbMigration).IsAssignableFrom(migrationType)
						? new MigrationDescriptor(migrationType, parameters, index: descriptors.Count)
						: throw new ArgumentException(
							$"Migration {migrationType} does not implement {typeof(IDbMigration)}.",
							nameof(migrationType));

				if(! migrationTypeSet.Add(descriptor.type)) {
					var msg = $"Migration type {migrationType} is already registered.";
					throw new ArgumentException(msg, nameof(migrationType));
				}
				else if(! migrationTypesByIds.TryAdd(descriptor.id, migrationType)) {
					var msg =
						$"Migration id {descriptor.id} of type {migrationType} is already used "
						+ $"by type {migrationTypesByIds[descriptor.id]}.";
					throw new ArgumentException(msg, nameof(migrationType));
				}
				else
					descriptors.Add(descriptor);
			}
		}

		static (String planId, List<MigrationDescriptor> descriptors) PlanMigrations (IDbMigrationPlan migrationPlan) {
			var migrations = new DbMigrationCollection();
			migrationPlan.Configure(migrations);

			var planId =
				migrationPlan.migrationPlanId switch {
					null => throw new Exception("Migration plan id is null."),
					"" => throw new Exception("Migration plan id is empty string."),
					{} value when IsNullOrWhiteSpace(value) => throw new Exception("Migration plan id is white space."),
					{} value => value
				};

			return (planId, migrations.descriptors);
		}

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		static async ValueTask<MigrationDescriptor?> MbNextDescriptor
			(IDbMigrationProvider dbMigrationProvider,
			 IRwSqlTransaction transaction,
			 String planId,
			 List<MigrationDescriptor> descriptors,
			 CancellationToken cancellationToken) {
			var mbLastMigration = await
				dbMigrationProvider.MaybeLastMigrationInfo(transaction, planId, cancellationToken);
			if(! (mbLastMigration is {} lastMigration))
				return descriptors[0];
			else if(lastMigration.index < 0
			        || lastMigration.index >= descriptors.Count
			        || descriptors[lastMigration.index].id != lastMigration.id) {
				var msg = "Failed to migrate database because last migration "
				          + $"'{lastMigration.id}' at '{lastMigration.index}' is not known.";
				throw new SqlException.MigrationFailure(msg);
			}
			else if(lastMigration.index + 1 is var nextMigrationIndex
			        && nextMigrationIndex < descriptors.Count)
				return descriptors[nextMigrationIndex];
			else
				return null;
		}

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		async ValueTask MigrateAsync
			(IRwSqlTransaction transaction, MigrationDescriptor descriptor, CancellationToken cancellationToken) {
			var migration =
				(IDbMigration)
				ActivatorUtilities.CreateInstance(serviceProvider, descriptor.type, descriptor.parameters);

			ISqlTest? test;
			if(! descriptor.hasTest)
				test = null;
			else {
				test = (ISqlTest) migration;

				var error = await test.Test(transaction, cancellationToken);
				switch(error) {
					case null when descriptor.pretestAlwaysFails: {
						var msg = $"Pretest of migration '{descriptor.id}' ended without error.";
						throw new SqlException.MigrationFailure(msg);
					}
					case {}: {
						var log = "Pretest of migration '{migrationId}' ended with error.\n{error}";
						logger.LogInformation(log, descriptor.id, error);
						break;
					}
				}
			}

			await migration.Exec(transaction, cancellationToken);

			if(test is {}) {
				var error = await test.Test(transaction, cancellationToken);
				if(error is {}) {
					var msg = $"Pretest of migration '{descriptor.id}' ended with error.\n{error}";
					throw new SqlException.MigrationFailure(msg);
				}
			}
		}

		/// <exception cref = "OperationCanceledException" />
		async void BeginExecute (TaskCompletionSource<Object?> whenCompleted, CancellationToken cancellationToken) {
			try {
				var (planId, descriptors) = PlanMigrations(migrationPlan);
				await using(await dbMigrationProvider.Lock(planId, cancellationToken))
					while(true) {
						await using var transaction = await dbProvider.BeginRwSerializable(cancellationToken);
						var descriptor = await
							MbNextDescriptor(dbMigrationProvider, transaction, planId, descriptors, cancellationToken);
						if(descriptor is {})
							try {
								var startLog = "Run migration '{migrationId}' ({current}/{total}).";
								logger.LogInformation(startLog, descriptor.id, descriptor.index + 1, descriptors.Count);

								await MigrateAsync(transaction, descriptor, cancellationToken);
								await dbMigrationProvider.SetLastMigrationInfo(
									transaction,
									planId,
									descriptor.index,
									descriptor.id,
									cancellationToken);
								await transaction.CommitAsync(cancellationToken);

								logger.LogInformation("Migration '{migrationId}' completed.", descriptor.id);
							}
							catch(SqlException.SerializationFailure ex) {
								var log = "Migration '{migrationId}' had serialization failure. Trying again.";
								logger.LogInformation(ex, log, descriptor.id);
							}
							catch(SqlException ex) {
								var log = "Migration '{migrationId}' failed. Application is going to be stopped.";
								logger.LogCritical(ex, log, descriptor.id);

								var failureInfo = $"Migration '{descriptor.id}' failed.\n{ex}";
								state.whenCompleted.SetResult(new DbMigrationResult.Failed(failureInfo));

								Environment.ExitCode = 1;
								appLifetime.StopApplication();
								break;
							}
						else {
							logger.LogInformation("Database migration completed.");

							state.whenCompleted.SetResult(DbMigrationResult.succeeded);
							break;
						}
					}
			}
			catch(OperationCanceledException) {
				state.whenCompleted.SetResult(DbMigrationResult.canceled);
			}
			finally {
				whenCompleted.SetResult(null);
			}
		}

		CancellationTokenSource cancellation { get; } =
			new CancellationTokenSource();

		TaskCompletionSource<Object?>? whenExecuted;

		/// <inheritdoc />
		public Task StartAsync (CancellationToken cancellationToken) {
			whenExecuted = new TaskCompletionSource<Object?>(TaskCreationOptions.RunContinuationsAsynchronously);
			BeginExecute(whenExecuted, cancellationToken);
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public async Task StopAsync (CancellationToken cancellationToken) {
			if(whenExecuted is {})
				try {
					cancellation.Cancel();
				}
				finally {
					await Task.WhenAny(whenExecuted.Task, Task.Delay(Timeout.Infinite, cancellationToken));
				}
		}

		public void Dispose () {
			cancellation.Cancel();
			cancellation.Dispose();
			state.whenCompleted.TrySetCanceled();
		}
	}
}
