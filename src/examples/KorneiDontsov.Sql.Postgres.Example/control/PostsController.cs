namespace KorneiDontsov.Sql.Postgres.Example {
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using static Affect;

	[ApiController, Route("/api/posts")]
	public class PostsController: ControllerBase {
		[HttpGet]
		[BeginRoReadCommitted]
		public async Task<IEnumerable<PostView>> GetPosts
			([FromServices] IRoSqlTransaction transaction,
			 CancellationToken cancellationToken) {
			var sql = "select * from posts order by timestamp desc limit 100";
			var posts = await transaction.QueryRows<Post>(sql, cancellationToken);
			return posts.Select(post => new PostView { author = post.author, content = post.content });
		}

		[HttpPost]
		[BeginRwSerializable, CommitOn(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> Post
			([FromBody] PostView postView,
			 [FromServices] IRwSqlTransaction transaction,
			 CancellationToken cancellationToken) {
			var sql = "insert into posts (author, content) values (@author, @content)";
			await transaction.ExecuteAsync(sql, cancellationToken, args: postView, affect: SingleRow);
			return NoContent();
		}
	}
}
