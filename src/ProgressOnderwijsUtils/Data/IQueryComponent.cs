﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ProgressOnderwijsUtils.Data
{
	interface IQueryComponent : IEquatable<IQueryComponent>
	{
		string ToSqlString(QueryFactory qnum);

		string ToDebugText();
	}

	interface IQueryParameter : IQueryComponent
	{
		SqlParameter ToSqlParameter(int paramNum);
		bool CanShareParamNumberWith(IQueryParameter other);
		int ParamNumberSharingHashCode();
	}

	public struct IntValues_DbTableType : IMetaObject
	{
		public int val { get; set; }
	}
}
