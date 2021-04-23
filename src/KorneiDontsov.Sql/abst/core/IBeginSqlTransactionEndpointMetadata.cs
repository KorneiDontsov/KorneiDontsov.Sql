namespace KorneiDontsov.Sql {
	using System.Data;

	public interface IBeginSqlTransactionEndpointMetadata {
		IsolationLevel isolationLevel { get; }
		SqlAccess? access => null;
	}
}
