namespace KorneiDontsov.Sql.Postgres {
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using System;

	sealed class PostgresDbProviderSettings {
		public String database { get; }
		public String host { get; }
		public Int32 port { get; }
		public String username { get; }
		public PostgresPasswordSource passwordSource { get; }
		public Int32 defaultQueryTimeout { get; }
		public String? searchPath { get; }
		public Boolean awaitUntilDbIsReady { get; }
		public Int32 connectionTimeout { get; }
		public Int32 minPoolSize { get; }
		public Int32 maxPoolSize { get; }
		public Int32 connectionIdleLifetime { get; }
		public Int32 connectionPruningInterval { get; }

		void Validate () {
			if(port <= 0)
				throw new Exception($"Port is not positive: {port}.");
			else if(defaultQueryTimeout < 0)
				throw new Exception($"Default query timeout is negative: {defaultQueryTimeout}.");
			else if(connectionTimeout < 0)
				throw new Exception($"Connection timeout is negative: {connectionTimeout}.");
			else if(minPoolSize < 0)
				throw new Exception($"Min pool size is negative: {minPoolSize}");
			else if(maxPoolSize < 0)
				throw new Exception($"Max pool size is negative: {maxPoolSize}");
			else if(maxPoolSize < minPoolSize)
				throw new Exception($"Max pool size '{maxPoolSize}' is less that min pool size '{minPoolSize};");
			else if(connectionIdleLifetime < 0)
				throw new Exception($"Connection idle lifetime is negative: {connectionIdleLifetime}");
			else if(connectionPruningInterval < 0)
				throw new Exception($"Connection pruning interval is negative: {connectionPruningInterval}");
		}

		public PostgresDbProviderSettings
			(String database,
			 String host,
			 Int32 port,
			 String username,
			 PostgresPasswordSource passwordSource,
			 Int32 defaultQueryTimeout = 30,
			 String? searchPath = null,
			 Boolean awaitUntilDbIsReady = false,
			 Int32 connectionTimeout = 15,
			 Int32 minPoolSize = 0,
			 Int32 maxPoolSize = 100,
			 Int32 connectionIdleLifetime = 300,
			 Int32 connectionPruningInterval = 10) {
			this.database = database;
			this.host = host;
			this.port = port;
			this.username = username;
			this.passwordSource = passwordSource;
			this.defaultQueryTimeout = defaultQueryTimeout;
			this.searchPath = searchPath;
			this.awaitUntilDbIsReady = awaitUntilDbIsReady;
			this.connectionTimeout = connectionTimeout;
			this.minPoolSize = minPoolSize;
			this.maxPoolSize = maxPoolSize;
			this.connectionIdleLifetime = connectionIdleLifetime;
			this.connectionPruningInterval = connectionPruningInterval;

			Validate();
		}

		[ActivatorUtilitiesConstructor]
		public PostgresDbProviderSettings (IConfiguration configuration) {
			static Exception NotFound (String propName, IConfigurationSection conf) =>
				throw new Exception($"Property '{propName}' is not found in '{conf.Path}'");

			static String GetString (IConfigurationSection conf, String propName) =>
				conf[propName] ?? throw NotFound(propName, conf);

			static Exception NotInteger (String propName, String propValue, IConfigurationSection conf) =>
				throw new Exception($"Property '{propName}' = '{propValue}' in '{conf.Path}' is not an integer.");

			static Int32 GetInt32 (IConfigurationSection conf, String propName) =>
				conf[propName] switch {
					{} propValue when Int32.TryParse(propValue, out var intValue) => intValue,
					{} propValue => throw NotInteger(propName, propValue, conf),
					null => throw NotFound(propName, conf)
				};

			static Int32 GetInt32OrDefault (IConfigurationSection conf, String propName, Int32 defaultValue) =>
				conf[propName] switch {
					{} propValue when Int32.TryParse(propValue, out var intValue) => intValue,
					{} propValue => throw NotInteger(propName, propValue, conf),
					null => defaultValue
				};

			static Exception NotBoolean (String propName, String propValue, IConfigurationSection conf) =>
				throw new Exception($"Property '{propName}' = '{propValue}' in '{conf.Path}' is not a boolean.");

			static Boolean GetBooleanOrDefault (IConfigurationSection conf, String propName, Boolean defaultValue) =>
				conf[propName]?.ToLower() switch {
					"true" => true,
					"false" => false,
					null => false,
					{} propValue => throw NotBoolean(propName, propValue, conf)
				};

			var conf = configuration.GetSection("postgres");

			database = GetString(conf, "database");
			host = GetString(conf, "host");
			port = GetInt32(conf, "port");
			username = GetString(conf, "username");
			passwordSource =
				(conf["password"], conf["passfile"]) switch {
					({} password, _) => new PostgresPasswordSource.Text(password),
					(_, {} pgPassFilePath) => new PostgresPasswordSource.PgPassFile(pgPassFilePath),
					_ => throw NotFound("password", conf)
				};
			defaultQueryTimeout = GetInt32OrDefault(conf, "defaultQueryTimeout", 30);
			searchPath = conf["searchPath"];
			awaitUntilDbIsReady = GetBooleanOrDefault(conf, "awaitUntilDbIsReady", false);
			connectionTimeout = GetInt32OrDefault(conf, "connectionTimeout", 15);
			minPoolSize = GetInt32OrDefault(conf, "minPoolSize", 0);
			maxPoolSize = GetInt32OrDefault(conf, "maxPoolSize", 100);
			connectionIdleLifetime = GetInt32OrDefault(conf, "connectionIdleLifetime", 300);
			connectionPruningInterval = GetInt32OrDefault(conf, "connectionPruningInterval", 10);

			Validate();
		}
	}
}
