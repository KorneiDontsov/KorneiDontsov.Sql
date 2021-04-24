namespace KorneiDontsov.Sql.Postgres.Tests {
	using System;
	using System.Buffers;
	using System.Diagnostics;

	class DockerService {
		String composeBinFilePath { get; }

		String workingDirectory { get; }

		public DockerService (String workingDirectory) {
			composeBinFilePath =
				App.configuration["docker:bin:compose"]
				?? throw new("docker:bin:compose is not specified in appsettings.json");
			this.workingDirectory = workingDirectory;
		}

		public void Compose (String args, DockerOutput output = DockerOutput.Stderr) {
			var console = Console.Out;

			var command = $"docker-compose {args}";
			using var process =
				new Process {
					StartInfo = new() {
						WorkingDirectory = workingDirectory,
						FileName = composeBinFilePath,
						Arguments = args,
						RedirectStandardOutput = output is DockerOutput.Stdout,
						RedirectStandardError = output is DockerOutput.Stderr
					}
				};
			if(process.Start()) {
				console.WriteLine();
				console.WriteLine($"> {command}");
			}
			else {
				console.WriteLine();
				var failureMsg = $"> Failed to start process: {command}";
				console.WriteLine(failureMsg);
				throw new(failureMsg);
			}

			var dockerComposeOutput =
				output switch {
					DockerOutput.Stdout => process.StandardOutput,
					DockerOutput.Stderr => process.StandardError
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
			var exitMsg = $"> Process exited with code {exitCode}: {command}";
			console.WriteLine(exitMsg);
			console.WriteLine();
			if(exitCode is not 0) throw new(exitMsg);
		}

		public DockerComposeUpScope ComposeUp () {
			Compose("up -d");
			return new(this);
		}
	}
}
