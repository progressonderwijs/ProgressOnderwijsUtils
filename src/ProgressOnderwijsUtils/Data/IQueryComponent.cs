using System;
using System.Data.SqlClient;

namespace ProgressOnderwijsUtils
{
    interface IQueryComponent : IEquatable<IQueryComponent>, IBuildableQuery
    {
        string ToSqlString(ref CommandFactory qnum);
    }

    interface IQueryParameter : IQueryComponent
    {
        SqlParameter ToSqlParameter(string paramName);
        object EquatableValue { get; }
    }
}
