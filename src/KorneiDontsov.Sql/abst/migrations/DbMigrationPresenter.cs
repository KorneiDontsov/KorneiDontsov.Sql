namespace KorneiDontsov.Sql.Migrations {
	using Arcanum.Routes;
	using System;

	public abstract record DbMigrationPresenter {
		DbMigrationPresenter () { }

		/// <inheritdoc cref = "DbMigrationPresenter" />
		/// <summary>
		///     Migration is presented by class that implements <see cref = "IDbMigration" />.
		/// </summary>
		public sealed record Class: DbMigrationPresenter {
			/// <summary> Migration class type. Must implement <see cref = "IDbMigration" />. </summary>
			public Type type { get; }

			/// <summary>
			///     The additional parameters that are injected to the constructor of migration class.
			///     By default it's an empty array.
			/// </summary>
			public Object[] parameters { get; }

			/// <param name = "type"> Migration class type. Must implement <see cref = "IDbMigration" />. </param>
			public Class (Type type, Object[]? parameters = null) {
				if(! typeof(IDbMigration).IsAssignableFrom(type))
					throw new ArgumentException($"{type} doesn't implement {typeof(IDbMigration)}.");
				else {
					this.type = type;
					this.parameters = parameters ?? Array.Empty<Object>();
				}
			}
		}

		/// <inheritdoc cref = "DbMigrationPresenter" />
		/// <summary>
		///     Migration is presented by sql script that is included in the assembly as an embedded resource.
		/// </summary>
		public sealed record EmbeddedScript: DbMigrationPresenter {
			/// <summary> Location of the script in the project. Must be in format '*.sql'. </summary>
			public Route location { get; }

			public String? locationNamespace { get; }

			/// <summary> Migration id. By default it's the script name. </summary>
			public String migrationId { get; }

			/// <param name = "location"> Location of the script in the project. Must be in format '*.sql'. </param>
			/// <param name = "migrationId"> Migration id. By default it's the script name. </param>
			public EmbeddedScript (Route location, String? locationNamespace = null, String? migrationId = null) {
				if(location.isDefaultOrEmpty)
					throw new ArgumentException("location is empty.", nameof(location));
				else if(location.nodes[^1] is var scriptName && ! scriptName.EndsWith(".sql", StringComparison.Ordinal))
					throw new ArgumentException($"File '{location}' is not '*.sql'.");
				else {
					this.location = location;
					this.locationNamespace = locationNamespace;
					this.migrationId = migrationId ?? scriptName[..^4];
				}
			}
		}
	}
}
