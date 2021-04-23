namespace KorneiDontsov.Sql {
	public interface IRwSqlTransaction: ISqlTransaction {
		SqlAccess? ISqlProvider.initialAccess => SqlAccess.Rw;
	}
}
