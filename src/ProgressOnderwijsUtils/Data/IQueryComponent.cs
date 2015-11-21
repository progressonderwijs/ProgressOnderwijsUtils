using System;
using System.Data.SqlClient;
using ProgressOnderwijsUtils.Data;

namespace ProgressOnderwijsUtils
{
    interface IQueryComponent : IEquatable<IQueryComponent>, IBuildableQuery
    {
        string ToSqlString(CommandFactory qnum);
        string ToDebugText(Taal? taalOrNull);
    }

    interface IQueryParameter : IQueryComponent
    {
        SqlParameter ToSqlParameter(string paramName);
    }
}
