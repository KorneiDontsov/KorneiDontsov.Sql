namespace KorneiDontsov.Sql.Postgres.Example {
	using Microsoft.AspNetCore.Mvc;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using static Affect;

	[ApiController, Route("/api/posts")]
	public class PostsController: ControllerBase {
		[HttpGet, BeginRoRepeatableRead]
		public async Task<IEnumerable<PostDto>> GetPosts
			([FromServices] IRoSqlTransaction sqlTransaction, CancellationToken ct) {
			var sql = "select * from posts order by timestamp desc limit 100";
			var posts = await sqlTransaction.QueryRows<Post>(sql, ct);
			return posts.Select(post => new PostDto { author = post.author, content = post.content });
		}

		[HttpPost, BeginRwSerializable]
		public async Task<IActionResult> Post
			([FromBody] PostDto post, [FromServices] IRwSqlTransaction sqlTransaction, CancellationToken ct) {
			var sql = "insert into posts (author, content) values (@author, @content)";
			await sqlTransaction.ExecuteAsync(sql, ct, args: post, affect: SingleRow);
			return NoContent();
		}
	}
}
