namespace KorneiDontsov.Sql.Migrations {
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Routing;
	using Microsoft.Extensions.DependencyInjection;
	using System;
	using System.Net.Mime;
	using System.Threading.Tasks;

	public static class EndpointFunctions {
		static async Task HandleDbMigrationResultLongPolls (HttpContext context) {
			var dbMigrationState = context.RequestServices.GetService<IDbMigrationState>();
			if(dbMigrationState is null) {
				context.Response.StatusCode = StatusCodes.Status204NoContent;
				context.Response.ContentType = MediaTypeNames.Text.Plain;
				await context.Response.WriteAsync("NoMigration");
			}
			else {
				var dbMigrationResult = await dbMigrationState.WhenCompleted(context.RequestAborted);
				switch(dbMigrationResult) {
					case DbMigrationResult.Succeeded _:
						context.Response.StatusCode = StatusCodes.Status200OK;
						context.Response.ContentType = MediaTypeNames.Text.Plain;
						await context.Response.WriteAsync("Migrated");
						break;

					case DbMigrationResult.Failed failedResult:
						context.Response.StatusCode = StatusCodes.Status500InternalServerError;
						context.Response.ContentType = MediaTypeNames.Text.Plain;
						await context.Response.WriteAsync(failedResult.info);
						break;

					case DbMigrationResult.Canceled _:
						context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
						context.Response.ContentType = MediaTypeNames.Text.Plain;
						await context.Response.WriteAsync("Canceled");
						break;

					default:
						throw new Exception($"{dbMigrationResult.GetType()} is not known.");
				}
			}
		}

		public static IEndpointConventionBuilder MapDbMigrationResultLongPolls
			(this IEndpointRouteBuilder endpoints, String pattern) =>
			endpoints.Map(pattern, HandleDbMigrationResultLongPolls).WithoutDbMigration();
	}
}
