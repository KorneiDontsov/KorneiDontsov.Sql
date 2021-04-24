namespace KorneiDontsov.Sql.Postgres.Tests {
	using System;

	class DockerComposeUpScope: IDisposable {
		readonly DockerService docker;
		public DockerComposeUpScope (DockerService docker) => this.docker = docker;

		public void Dispose () {
			docker.Compose("stop");
			docker.Compose("logs --no-color", DockerOutput.Stdout);
			docker.Compose("down -v");
		}
	}
}
