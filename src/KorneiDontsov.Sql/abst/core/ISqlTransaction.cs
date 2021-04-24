namespace KorneiDontsov.Sql {
	using System;
	using System.Threading.Tasks;

	public interface ISqlTransaction: ISqlProvider {
		void OnCommitted (Action action);

		void OnCommitted (Func<ValueTask> action);
	}
}
