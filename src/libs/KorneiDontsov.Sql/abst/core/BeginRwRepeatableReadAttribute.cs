﻿namespace KorneiDontsov.Sql {
	using System;
	using System.Data;

	public sealed class BeginRwRepeatableReadAttribute: Attribute, IBeginAccessEndpointMetadata, IBeginIsolationLevelEndpointMetadata {
		/// <inheritdoc />
		public SqlAccess access => SqlAccess.Rw;

		/// <inheritdoc />
		public IsolationLevel isolationLevel => IsolationLevel.RepeatableRead;
	}
}
