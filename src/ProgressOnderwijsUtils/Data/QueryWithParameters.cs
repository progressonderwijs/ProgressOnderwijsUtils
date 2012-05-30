//using System.Data.SqlClient;
//using System.Linq;
//using System.Collections.Generic;
//using System;
//using ProgressOnderwijsUtils;
//using MoreLinq;

//namespace ProgressOnderwijsUtils.Data
//{
//    public struct QueryWithParameters : IEquatable<QueryWithParameters>
//    {
//        public readonly string CommandText;
//        readonly SqlParameter[] Params;

//        public QueryWithParameters(string text, SqlParameter[] parms)
//        {
//            CommandText = text;
//            Params = parms;
//        }

//        public bool Equals(QueryWithParameters other) { return CommandText == other.CommandText && Params.Select(p => p.Value).SequenceEqual(other.Params.Select(p => p.Value)); }
//        public override bool Equals(object obj) { return obj is QueryWithParameters && Equals((QueryWithParameters)obj); }

//        public override int GetHashCode()
//        {
//            return 11 + CommandText.GetHashCode() - Params.Length +
//                   Params.Select((p, i) => p.Value.GetHashCode() * (i * 2 + 1)).Aggregate(0, (a, b) => a + b); //don't use Sum because sum does overflow checking.
//        }

//        public static bool operator ==(QueryWithParameters a, QueryWithParameters b) { return ReferenceEquals(a, b) || null != a && null != b && a.Equals(b); } //watch out: a!=null would create infinite recursion.
//        public static bool operator !=(QueryWithParameters a, QueryWithParameters b) { return !(a == b); }
//        public override string ToString() { return "{ CommandText = \"" + CommandText + "\", Params = {" + Params.Select(p => p.Value == DBNull.Value ? "NULL" : p.Value.ToString()).JoinStrings(", ") + "} }"; }

//        public SqlCommand CreateCommand(SqlConnection sqlconn, int commandTimeout)
//        {
//            bool finishedOk = false;
//            var command = new SqlCommand();
//            try
//            {
//                command.Connection = sqlconn;
//#if USE_RAW_TRANSACTIONS
//                command.Transaction = conn.SqlTransaction;
//#endif
//                command.CommandTimeout = commandTimeout;//60 by default
//                command.CommandText = CommandText;
//                command.Parameters.AddRange(Params);
//                finishedOk = true;
//                return command;
//            }
//            finally
//            {
//                if (!finishedOk)
//                    command.Dispose();
//            }
//        }

//    }
//}