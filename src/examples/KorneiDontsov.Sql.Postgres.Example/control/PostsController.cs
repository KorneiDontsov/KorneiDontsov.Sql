namespace KorneiDontsov.Sql.Postgres.Example {
	using Microsoft.AspNetCore.Mvc;
	using System.Threading;
	using System.Threading.Tasks;

	[ApiController, Route("/api/posts")]
	public class PostsController: ControllerBase {
		[HttpGet("last"), BeginRoReadCommitted]
		public async Task<Post> GetLastPost ([FromServices] IRoSqlTransaction transaction, CancellationToken ct) =>
			await transaction.QueryFirstRow<Post>(
				"select * from posts order by timestamp desc limit 1",
				ct);
	}
}
