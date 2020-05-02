namespace KorneiDontsov.Sql {
	using System;

	public interface ISqlRetryEvent {
		void AddHandler (Action handler);

		void RemoveHandler (Action handler);
	}
}
