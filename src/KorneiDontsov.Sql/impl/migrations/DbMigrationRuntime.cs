namespace KorneiDontsov.Sql.Migrations {
	using System;
	using System.Diagnostics;

	record DbMigrationRuntime {
		public Type startupType { get; }

		public DbMigrationRuntime () {
			static Type FindStartupType () {
				var stackTrace = new StackTrace(skipFrames: 1);
				for(var i = 0; i < stackTrace.FrameCount; ++ i) {
					var type = stackTrace.GetFrame(i)?.GetMethod()?.DeclaringType;
					if(type is not null && type.Name.EndsWith("Startup", StringComparison.OrdinalIgnoreCase))
						return type;
				}
				throw new InvalidOperationException(
					"Failed to find Startup-like type. Probably the method was invoked in a method declared outside of "
					+ "a type. Please, invoke method in Startup-like type, so migraton module could analyse application"
					+ " layer and use embedded resources, etc.");
			}

			startupType = FindStartupType();
		}
	}
}
