namespace KorneiDontsov.Sql {
	public interface IRoSqlTransaction: ISqlTransaction {
		SqlAccess? ISqlProvider.initialAccess => SqlAccess.Ro;
	}
}
