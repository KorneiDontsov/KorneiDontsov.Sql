namespace KorneiDontsov.Sql.Migrations {
	using Arcanum.Routes;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Hosting;
	using Microsoft.Extensions.Logging;
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Runtime.CompilerServices;
	using System.Threading;
	using System.Threading.Tasks;
	using static SqlConflict;
	using static System.String;

	sealed class DbMigrationService: BackgroundService {
		DbMigrationRuntime runtime { get; }
		DbMigrationState state { get; }
		IDbProvider dbProvider { get; }
		IDbMigrationProvider dbMigrationProvider { get; }
		IDbMigrationPlan migrationPlan { get; }
		IHostApplicationLifetime appLifetime { get; }
		IServiceProvider serviceProvider { get; }
		ILogger logger { get; }

		public DbMigrationService
			(DbMigrationRuntime runtime,
			 DbMigrationState state,
			 IDbProvider dbProvider,
			 IDbMigrationProvider dbMigrationProvider,
			 IDbMigrationPlan migrationPlan,
			 IHostApplicationLifetime appLifetime,
			 IServiceProvider serviceProvider,
			 ILogger<DbMigrationService> logger) {
			this.runtime = runtime;
			this.state = state;
			this.dbProvider = dbProvider;
			this.dbMigrationProvider = dbMigrationProvider;
			this.migrationPlan = migrationPlan;
			this.appLifetime = appLifetime;
			this.serviceProvider = serviceProvider;
			this.logger = logger;
		}

		class MigrationDescriptor {
			public DbMigrationPresenter presenter { get; }
			public Int32 index { get; }
			public String id { get; }
			public Boolean hasTest { get; }
			public Boolean pretestAlwaysFails { get; }

			public MigrationDescriptor (DbMigrationPresenter presenter, Int32 index) {
				this.presenter = presenter;
				this.presenter = presenter;
				this.index = index;
				switch(presenter) {
					case DbMigrationPresenter.Class c:
						id = c.type.GetCustomAttribute<DbMigrationIdAttribute>()?.value ?? c.type.Name;
						hasTest = typeof(ISqlTest).IsAssignableFrom(c.type);
						pretestAlwaysFails = Attribute.IsDefined(c.type, typeof(MigrationPretestAlwaysFailsAttribute));
						break;
					case DbMigrationPresenter.EmbeddedScript s:
						id = s.migrationId;
						break;
					default:
						throw new($"{presenter.GetType()} is not matched.");
				}
			}
		}

		class MigrationDescriptorCollection: IDbMigrationCollection {
			readonly List<MigrationDescriptor> descriptors = new();

			readonly Dictionary<String, MigrationDescriptor> descriptorsByIds = new();

			readonly HashSet<Type> migrationTypeSet = new();

			public Int32 count => descriptors.Count;

			public MigrationDescriptor this [Int32 index] => descriptors[index];

			public MigrationDescriptor? MayGetById (String id) => descriptorsByIds.GetValueOrDefault(id);

			/// <inheritdoc />
			public void Add (DbMigrationPresenter dbMigrationPresenter) {
				[MethodImpl(MethodImplOptions.NoInlining)]
				static void Throw (String msg) =>
					throw new ArgumentException(msg, nameof(dbMigrationPresenter));

				if(dbMigrationPresenter is DbMigrationPresenter.Class c && ! migrationTypeSet.Add(c.type))
					Throw($"Migration type {c.type} is already registered.");

				var descriptor = new MigrationDescriptor(dbMigrationPresenter, index: descriptors.Count);

				if(! descriptorsByIds.TryAdd(descriptor.id, descriptor)) {
					static String ToStr (DbMigrationPresenter p) =>
						p switch {
							DbMigrationPresenter.Class c => $"class '{c.type.FullName}'",
							DbMigrationPresenter.EmbeddedScript s => $"script '{s.location}'"
						};

					Throw(
						$"Migration id '{descriptor.id}' of {ToStr(dbMigrationPresenter)} is already used by "
						+ $"{ToStr(descriptorsByIds[descriptor.id].presenter)}.");
				}

				descriptors.Add(descriptor);
			}
		}

		static (String schema, MigrationDescriptorCollection descriptors) Descript (IDbMigrationPlan migrationPlan) {
			var schema =
				migrationPlan.migrationSchema switch {
					null => throw new("Migration schema is null."),
					"" => throw new("Migration schema is empty string."),
					{ } value when IsNullOrWhiteSpace(value) => throw new("Migration schema is white space."),
					{ } value => value
				};

			var migrationDescriptors = new MigrationDescriptorCollection();
			migrationPlan.Configure(migrationDescriptors);

			return (schema, migrationDescriptors);
		}

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		static async ValueTask<MigrationDescriptor?> MayGetNextDescriptor
			(IDbMigrationProvider dbMigrationProvider,
			 IRwSqlTransaction transaction,
			 String migrationSchema,
			 MigrationDescriptorCollection descriptors,
			 CancellationToken cancellationToken) {
			var maybeLastMigration =
				await dbMigrationProvider.MaybeLastMigrationInfo(transaction, migrationSchema, cancellationToken);
			if(maybeLastMigration is not var (lastMigrationIndex, lastMigrationId))
				return descriptors[0];
			else if(descriptors.MayGetById(lastMigrationId) is { index: var expectedIndex }
			        && lastMigrationIndex != expectedIndex) {
				var msg =
					$"Expected migration '{lastMigrationId}' to be at '{expectedIndex}', but found at "
					+ $"{lastMigrationIndex} as last registered migration in database.";
				throw new SqlException.MigrationFailure(msg);
			}
			else if(lastMigrationIndex < 0 || lastMigrationIndex >= descriptors.count) {
				var msg =
					$"Last migration that is registered in database as '{lastMigrationId}' at '{lastMigrationIndex}' "
					+ "is not known.";
				throw new SqlException.MigrationFailure(msg);
			}
			else if(descriptors[lastMigrationIndex].id != lastMigrationId) {
				var msg =
					$"Last migration that is registered in database as '{lastMigrationId}' at '{lastMigrationIndex}' "
					+ $"is not known. Expected migration '{descriptors[lastMigrationIndex]}' here.";
				throw new SqlException.MigrationFailure(msg);
			}
			else if(lastMigrationIndex + 1 < descriptors.count)
				return descriptors[lastMigrationIndex + 1];
			else
				return null;
		}

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		async ValueTask MigrateByClassAsync
			(IRwSqlTransaction transaction,
			 MigrationDescriptor descriptor,
			 DbMigrationPresenter.Class presenter,
			 CancellationToken ct) {
			var migration =
				(IDbMigration) ActivatorUtilities.CreateInstance(serviceProvider, presenter.type, presenter.parameters);

			ISqlTest? test;
			if(! descriptor.hasTest)
				test = null;
			else {
				test = (ISqlTest) migration;

				var error = await test.Test(transaction, ct);
				switch(error) {
					case null when descriptor.pretestAlwaysFails: {
						var msg = $"Pretest of migration '{descriptor.id}' ended without error.";
						throw new SqlException.MigrationFailure(msg);
					}
					case not null: {
						var log = "Pretest of migration '{migrationId}' ended with error.\n{error}";
						logger.LogInformation(log, descriptor.id, error);
						break;
					}
				}
			}

			await migration.Exec(transaction, ct);

			if(test is not null) {
				var error = await test.Test(transaction, ct);
				if(error is not null) {
					var msg = $"Pretest of migration '{descriptor.id}' ended with error.\n{error}";
					throw new SqlException.MigrationFailure(msg);
				}
			}
		}

		/// <exception cref = "SqlException" />
		/// <exception cref = "OperationCanceledException" />
		async ValueTask MigrateByScriptAsync
			(IRwSqlTransaction transaction,
			 MigrationDescriptor descriptor,
			 DbMigrationPresenter.EmbeddedScript presenter,
			 CancellationToken ct) {
			var scriptText =
				await runtime.startupType.Assembly.ReadResourceText(
					presenter.location,
					presenter.locationNamespace ?? runtime.startupType.Namespace);
			await transaction.ExecuteAsync(scriptText, ct);
		}

		/// <inheritdoc />
		protected async override Task ExecuteAsync (CancellationToken stoppingToken) {
			try {
				var (schema, descriptors) = Descript(migrationPlan);
				await using(await dbMigrationProvider.Lock(schema, stoppingToken))
					while(true) {
						await using var transaction = await dbProvider.BeginRwSerializable(stoppingToken);
						var descriptor =
							await MayGetNextDescriptor(
								dbMigrationProvider,
								transaction,
								schema,
								descriptors,
								stoppingToken);
						if(descriptor is not null)
							try {
								var startLog = "Run migration '{migrationId}' ({current}/{total}).";
								logger.LogInformation(startLog, descriptor.id, descriptor.index + 1, descriptors.count);

								await (descriptor.presenter switch {
									DbMigrationPresenter.Class classPresenter =>
										MigrateByClassAsync(transaction, descriptor, classPresenter, stoppingToken),
									DbMigrationPresenter.EmbeddedScript scriptPresenter =>
										MigrateByScriptAsync(transaction, descriptor, scriptPresenter, stoppingToken)
								});
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
