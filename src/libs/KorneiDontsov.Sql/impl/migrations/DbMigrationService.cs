namespace KorneiDontsov.Sql.Migrations {
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;
	using static SqlConflict;
	using static System.String;

	sealed class DbMigrationService: BackgroundService {
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

		class MigrationDescriptorCollection: IDbMigrationCollection {
			readonly List<MigrationDescriptor> descriptors =
				new List<MigrationDescriptor>();

			readonly Dictionary<String, MigrationDescriptor> descriptorsByIds =
				new Dictionary<String, MigrationDescriptor>();

			readonly HashSet<Type> migrationTypeSet =
				new HashSet<Type>();

			public Int32 count => descriptors.Count;

			public MigrationDescriptor this [Int32 index] => descriptors[index];

			public MigrationDescriptor? MayGetById (String id) => descriptorsByIds.GetValueOrDefault(id);

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
				else if(! descriptorsByIds.TryAdd(descriptor.id, descriptor)) {
					var msg =
						$"Migration id {descriptor.id} of type {migrationType} is already used "
						+ $"by type {descriptorsByIds[descriptor.id].type}.";
					throw new ArgumentException(msg, nameof(migrationType));
				}
				else
					descriptors.Add(descriptor);
			}
		}

		static (String schema, MigrationDescriptorCollection descriptors) Descript (IDbMigrationPlan migrationPlan) {
			var schema =
				migrationPlan.migrationSchema switch {
					null => throw new Exception("Migration schema is null."),
					"" => throw new Exception("Migration schema is empty string."),
					{} value when IsNullOrWhiteSpace(value) => throw new Exception("Migration schema is white space."),
					{} value => value
				};

			var migrationDescriptors = new MigrationDescriptorCollection();
			migrationPlan.Configure(migrationDescriptors);

			return (schema, migrationDescriptors);
		}

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		static async ValueTask<MigrationDescriptor?> MbNextDescriptor
			(IDbMigrationProvider dbMigrationProvider,
			 IRwSqlTransaction transaction,
			 String migrationSchema,
			 MigrationDescriptorCollection descriptors,
			 CancellationToken cancellationToken) {
			var mbLastMigration = await
				dbMigrationProvider.MaybeLastMigrationInfo(transaction, migrationSchema, cancellationToken);
			if(! (mbLastMigration is {} lastMigration))
				return descriptors[0];
			else if(descriptors.MayGetById(lastMigration.id) is {index: var expectedIndex}
			        && lastMigration.index != expectedIndex) {
				var msg =
					$"Expected migration '{lastMigration.id}' to be at '{expectedIndex}', but found at "
					+ $"{lastMigration.index} as last registered migration in database.";
				throw new SqlException.MigrationFailure(msg);
			}
			else if(lastMigration.index < 0 || lastMigration.index >= descriptors.count) {
				var msg =
					$"Last migration that is registered in database as '{lastMigration.id}' at '{lastMigration.index}' "
					+ "is not known.";
				throw new SqlException.MigrationFailure(msg);
			}
			else if(descriptors[lastMigration.index].id != lastMigration.id) {
				var msg =
					$"Last migration that is registered in database as '{lastMigration.id}' at '{lastMigration.index}' "
					+ $"is not known. Expected migration '{descriptors[lastMigration.index]}' here.";
				throw new SqlException.MigrationFailure(msg);
			}
			else if(lastMigration.index + 1 is var nextMigrationIndex
			        && nextMigrationIndex < descriptors.count)
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

		/// <inheritdoc />
		protected async override Task ExecuteAsync (CancellationToken stoppingToken) {
			try {
				var (schema, descriptors) = Descript(migrationPlan);
				await using(await dbMigrationProvider.Lock(schema, stoppingToken))
					while(true) {
						await using var transaction = await dbProvider.BeginRwSerializable(stoppingToken);
						var descriptor = await
							MbNextDescriptor(dbMigrationProvider, transaction, schema, descriptors, stoppingToken);
						if(descriptor is {})
							try {
								var startLog = "Run migration '{migrationId}' ({current}/{total}).";
								logger.LogInformation(startLog, descriptor.id, descriptor.index + 1, descriptors.count);

								await MigrateAsync(transaction, descriptor, stoppingToken);
								await dbMigrationProvider.SetLastMigrationInfo(
									transaction,
									schema,
									descriptor.index,
									descriptor.id,
									stoppingToken);
								await transaction.CommitAsync(stoppingToken);

								logger.LogInformation("Migration '{migrationId}' completed.", descriptor.id);
							}
							catch(SqlException.ConflictFailure ex) when(ex.conflict is SerializationFailure) {
								var log = "Migration '{migrationId}' had serialization failure. Trying again.";
								logger.LogInformation(ex, log, descriptor.id);
							}
							catch(SqlException ex) {
								var log = "Migration '{migrationId}' failed. Service will be stopped.";
								logger.LogCritical(ex, log, descriptor.id);

								var failureInfo = $"Migration '{descriptor.id}' failed.\n{ex}";
								state.Complete(new DbMigrationResult.Failed(failureInfo));

								Environment.ExitCode = 1;
								appLifetime.StopApplication();
								break;
							}
						else {
							logger.LogInformation("Database migration completed.");
							state.Complete(DbMigrationResult.ok);
							break;
						}
					}
			}
			catch(OperationCanceledException) {
				state.Complete(DbMigrationResult.canceled);
			}
			catch(Exception ex) {
				logger.LogCritical(ex, "Unhandled exception occurred in database migration. Service will be stopped.");
				Environment.ExitCode = 1;
				appLifetime.StopApplication();
			}
		}
	}
}
