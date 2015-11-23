using System;
using System.Data.SqlClient;

namespace ProgressOnderwijsUtils
{
    interface IQueryComponent : IEquatable<IQueryComponent>, IBuildableQuery
    {
    }

    interface IQueryParameter : IQueryComponent
    {
        SqlParameter ToSqlParameter(string paramName);
        object EquatableValue { get; }
    }
}
