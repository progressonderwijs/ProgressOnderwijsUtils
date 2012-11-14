using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ProgressOnderwijsUtils.Data
{
	interface IQueryComponent : IEquatable<IQueryComponent>
	{
		string ToSqlString(CommandFactory qnum);

		string ToDebugText(Taal? taalOrNull);
	}

	interface IQueryParameter : IQueryComponent
	{
		SqlParameter ToSqlParameter(int paramNum);
	}
}
