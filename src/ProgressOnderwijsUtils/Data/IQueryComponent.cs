using System;
using System.Data.SqlClient;

namespace ProgressOnderwijsUtils
{
    interface IQueryComponent : IEquatable<IQueryComponent>
    {
        string ToSqlString(CommandFactory qnum);
        string ToDebugText(Taal? taalOrNull);
    }

    interface IQueryParameter : IQueryComponent
    {
        SqlParameter ToSqlParameter(string paramName);
    }
}
