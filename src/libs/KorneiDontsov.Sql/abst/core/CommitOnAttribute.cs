namespace KorneiDontsov.Sql {
	using System;

	public sealed class CommitOnAttribute: Attribute, ICommitOnEndpointMetadata {
		/// <inheritdoc />
		public Int32 statusCode { get; }

		public CommitOnAttribute (Int32 statusCode) => this.statusCode = statusCode;
	}
}
