﻿using System.Collections.Generic;
using NUnit.Framework;
using Progress.Business;
using Progress.Business.Test;
using ProgressOnderwijsUtils;
using ProgressOnderwijsUtils.Test;

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

            const string q = @"
                select
                    'Geen index op kolom ['  + c.name + '] in tabel [' + s.name + '.' + pt.name + '], gebruikt in [' + k.name + ']'
                from sys.foreign_keys k
                join sys.foreign_key_columns kc on kc.constraint_object_id = k.object_id
                join sys.tables pt on pt.object_id = kc.parent_object_id
                join sys.columns c on c.object_id = kc.parent_object_id and c.column_id = kc.parent_column_id
                join sys.schemas s on s.schema_id = pt.schema_id
                left join sys.index_columns ic on ic.object_id = kc.parent_object_id and ic.column_id = kc.parent_column_id
                where ic.object_id is null
                    and k.name not in (select val from {0})
                ";

            Assert.That(QueryBuilder.Create(q, exceptions).ReadPlain<string>(conn), Is.Empty, "Geen index op FK-kolom.");
        }
    }
}
