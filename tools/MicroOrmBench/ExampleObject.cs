using System;
using System.Runtime.CompilerServices;
using ProgressOnderwijsUtils;

namespace MicroOrmBench
{
    public class ExampleObject : IMetaObject
    {
        public int? Arg { get; set; }
        public int? A { get; set; }
        public int B { get; set; }
        public string C { get; set; }
        public DateTime D { get; set; }
        public int E { get; set; }

        static readonly FormattableString formattableQueryString = $@"
	        select top ({0}) 
		        a=a.x+{2}
		        , b=b.x
		        , c=c.x+{"hehe"}
		        , d=d.x
		        , e=e.x
		        , Arg = {default(int?)}
	        from       (select x=0 union all select 1 union all select null) a
	        cross join (select x=0 union all select 1 union all select 2) b
	        cross join (select x=N'abracadabra fee fi fo fum' union all select N'abcdef' union all select N'quick brown fox') c
	        cross join (select x=getdate() union all select getdate() union all select getdate()) d
	        cross join (select x=0 union all select 1 union all select 2) e
	        cross join (select x=0 union all select 1 union all select 2 union all select 3) f
	        cross join (select x=0 union all select 1 union all select 2 union all select 3) g
	        cross join (select x=0 union all select 1 union all select 2 union all select 3) h
	        cross join (select x=0 union all select 1 union all select 2 union all select 3) i
        ";
        static readonly string formatString = formattableQueryString.Format;
        public static readonly string RawQueryString = string.Format(formatString, "@Top", "@Num2", "@Hehe", "@Arg");
        public static ParameterizedSql ParameterizedSqlForRows(int rows) => SafeSql.SQL(FormattableStringFactory.Create(formatString, rows, 2, "hehe", default(int?)));
    }
}