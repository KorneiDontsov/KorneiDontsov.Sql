namespace KorneiDontsov.Sql {
	using System;

	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public sealed class CommitOnAttribute: Attribute, ICommitOnEndpointMetadata {
		/// <inheritdoc />
		public Int32 statusCode { get; }

		public CommitOnAttribute (Int32 statusCode) => this.statusCode = statusCode;
	}
}
