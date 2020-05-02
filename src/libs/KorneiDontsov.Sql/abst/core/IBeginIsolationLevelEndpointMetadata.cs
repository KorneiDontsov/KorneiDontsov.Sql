namespace KorneiDontsov.Sql {
	using System.Data;

	public interface IBeginIsolationLevelEndpointMetadata {
		IsolationLevel isolationLevel { get; }
	}
}
