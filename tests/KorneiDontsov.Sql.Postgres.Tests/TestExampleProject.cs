namespace KorneiDontsov.Sql.Postgres.Tests {
	using FluentAssertions.Http;
	using KorneiDontsov.Sql.Postgres.Example;
	using Newtonsoft.Json;
	using NUnit.Framework;
	using Polly;
	using Polly.Retry;
	using System;
	using System.Buffers;
	using System.Diagnostics;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Net.Mime;
	using System.Text;
	using System.Threading.Tasks;

	public class TestExampleProject {
		static DirectoryInfo? MayFindRepositoryDir () {
			for(var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir is not null; dir = dir.Parent)
				if(dir.Name is "tests")
					return dir.Parent;

			return null;
		}

		static readonly DirectoryInfo repositoryDir =
			MayFindRepositoryDir() ?? throw new("Failed to find path to repository.");

		enum DockerComposeOutput { Stdout, Stderr }

		static void DockerCompose (String args, DockerComposeOutput output = DockerComposeOutput.Stderr) {
			var console = Console.Out;

			var command = $"docker-compose {args}";
			var workDir = Path.Combine("examples", "KorneiDontsov.Sql.Postgres.Example");
			using var process =
				new Process {
					StartInfo = new() {
						FileName = "C:/Program Files/Docker/Docker/resources/bin/docker-compose.exe",
						Arguments = args,
						WorkingDirectory = Path.Combine(repositoryDir.FullName, workDir),
						RedirectStandardOutput = output is DockerComposeOutput.Stdout,
						RedirectStandardError = output is DockerComposeOutput.Stderr
					}
				};
			if(process.Start()) {
				console.WriteLine();
				console.WriteLine($"{workDir}> {command}");
				console.WriteLine();
			}
			else {
				var failureMsg = $"{workDir}> Failed to start '{command}'";
				console.WriteLine();
				console.WriteLine(failureMsg);
				console.WriteLine();
				throw new(failureMsg);
			}

			var dockerComposeOutput =
				output switch {
					DockerComposeOutput.Stdout => process.StandardOutput,
					DockerComposeOutput.Stderr => process.StandardError
				};
			var bufferPool = ArrayPool<Char>.Shared;
			var buffer = bufferPool.Rent(4096);
			try {
				while(dockerComposeOutput.ReadBlock(buffer, 0, 4096) is > 0 and var charsRead)
					console.Write(buffer, 0, charsRead);
			}
			finally {
				bufferPool.Return(buffer);
			}

			process.WaitForExit();

			var exitCode = process.ExitCode;
			var exitMsg = $"{workDir}> '{command}' exited with code {exitCode}.";
			console.WriteLine();
			console.WriteLine(exitMsg);
			console.WriteLine();
			if(exitCode is not 0) throw new(exitMsg);
		}

		[SetUp]
		public void Setup () {
			DockerCompose("build");
			DockerCompose("down -v");
		}

		class DockerComposeUpScope: IDisposable {
			public void Dispose () {
				DockerCompose("logs --no-color", DockerComposeOutput.Stdout);
				DockerCompose("down -v");
			}
		}

		static DockerComposeUpScope DockerComposeUp () {
			DockerCompose("up -d");
			return new();
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
			using(DockerComposeUp()) {
				using var httpClient = CreateHttpClient();
				using var response = await GetAsync(httpClient, "api/posts");
				response.Should().HaveStatusCode(HttpStatusCode.OK)
					.And.HaveContent(new PostDto[] { new() { author = "unknown", content = "Hello, world!" } });
			}
		}

		[Test]
		public async Task Post () {
			using(DockerComposeUp()) {
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
