namespace KorneiDontsov.Sql.Postgres.Tests {
	using FluentAssertions.Http;
	using KorneiDontsov.Sql.Postgres.Example;
	using Newtonsoft.Json;
	using NUnit.Framework;
	using Polly;
	using Polly.Retry;
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Net.Mime;
	using System.Text;
	using System.Threading.Tasks;

	public class TestExampleProject {
		static DirectoryInfo GetRepositoryDir () {
			static DirectoryInfo? Maybe () {
				for(var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir is not null; dir = dir.Parent)
					if(dir.Name is "tests")
						return dir.Parent;

				return null;
			}

			return Maybe() ?? throw new("Failed to find path to repository.");
		}

		static DockerService docker { get; } =
			new(Path.Combine(GetRepositoryDir().FullName, "examples", "KorneiDontsov.Sql.Postgres.Example"));

		[SetUp]
		public void Setup () {
			docker.Compose("build");
			docker.Compose("down -v");
		}

		static HttpClient CreateHttpClient () =>
			new() {
				BaseAddress = new("http://127.0.0.1:23456/"),
				Timeout = TimeSpan.FromSeconds(5)
			};

		static readonly AsyncRetryPolicy<HttpResponseMessage> httpPolicy =
			Policy.Handle<HttpRequestException>()
				.OrResult<HttpResponseMessage>(
					response =>
						response.StatusCode switch {
							HttpStatusCode.ServiceUnavailable => true,
							_ => false
						})
				.WaitAndRetryAsync(retryCount: 10, _ => TimeSpan.FromSeconds(0.5));

		static Task<HttpResponseMessage> GetAsync (HttpClient httpClient, String requestUri) =>
			httpPolicy.ExecuteAsync(() => httpClient.GetAsync(requestUri));

		static Task<HttpResponseMessage> PostAsync<T> (HttpClient httpClient, String requestUri, T content) {
			var contentJson = JsonConvert.SerializeObject(content);
			var stringContent = new StringContent(contentJson, Encoding.UTF8, MediaTypeNames.Application.Json);
			return httpPolicy.ExecuteAsync(() => httpClient.PostAsync(requestUri, stringContent));
		}

		[Test]
		public async Task GetPosts () {
			using(docker.ComposeUp()) {
				using var httpClient = CreateHttpClient();
				using var response = await GetAsync(httpClient, "api/posts");
				response.Should().HaveStatusCode(HttpStatusCode.OK)
					.And.HaveContent(new PostDto[] { new() { author = "unknown", content = "Hello, world!" } });
			}
		}

		[Test]
		public async Task Post () {
			using(docker.ComposeUp()) {
				using var httpClient = CreateHttpClient();

				var newPost = new PostDto { author = "Tester", content = "This is a test message." };
				using var response1 = await PostAsync(httpClient, "api/posts", newPost);
				response1.Should().HaveStatusCode(HttpStatusCode.NoContent);

				using var response2 = await GetAsync(httpClient, "api/posts");
				response2.Should().HaveStatusCode(HttpStatusCode.OK)
					.And.HaveContent(
						new[] {
							new() { author = "unknown", content = "Hello, world!" },
							newPost
						});
			}
		}
	}
}
