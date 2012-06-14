using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace ProgressOnderwijsUtils.Data
{
	interface IQueryComponent : IEquatable<IQueryComponent>
	{
		string ToSqlString(CommandFactory qnum);

		string ToDebugText();
	}

	interface IQueryParameter : IQueryComponent
	{
		SqlParameter ToSqlParameter(int paramNum);
		bool CanShareParamNumberWith(IQueryParameter other);
		int ParamNumberSharingHashCode();
	}
}
