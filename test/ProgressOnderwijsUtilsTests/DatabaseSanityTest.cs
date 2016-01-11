using System.Collections.Generic;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;
using static ProgressOnderwijsUtils.SafeSql;

namespace ProgressOnderwijsUtilsTests
{
    [Continuous]
    public sealed class DatabaseSanityTest : TestSuiteBase
    {
        [Test]
        public void OntbrekendeForeignKeyIndexes()
        {
            IEnumerable<string> exceptions = new[] {
                "FK_log_logactietype", // dit was een niet gebruikte index die verwijderd is
            };

            Assert.That(SQL($@"
                select
                    'Geen index op kolom ['  + c.name + '] in tabel [' + s.name + '.' + pt.name + '], gebruikt in [' + k.name + ']'
                from sys.foreign_keys k
                join sys.foreign_key_columns kc on kc.constraint_object_id = k.object_id
                join sys.tables pt on pt.object_id = kc.parent_object_id
                join sys.columns c on c.object_id = kc.parent_object_id and c.column_id = kc.parent_column_id
                join sys.schemas s on s.schema_id = pt.schema_id
                left join sys.index_columns ic on ic.object_id = kc.parent_object_id and ic.column_id = kc.parent_column_id
                where 1=1
                    and ic.object_id is null
                    and k.name not in {exceptions}
                    and k.name not like 'fk_pnetlog_%%_Log_ActieTypeId' -- exclude al die actietype indexes in de log tabellen; deze zitten meer in de weg dan dat ze iets opleveren
                ").ReadPlain<string>(conn), Is.Empty, "Geen index op FK-kolom.");
        }
    }
}
