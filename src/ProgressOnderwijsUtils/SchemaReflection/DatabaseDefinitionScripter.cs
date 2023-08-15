namespace ProgressOnderwijsUtils.SchemaReflection;

public sealed record DatabaseDefinitionScripter(DatabaseDescription db)
{
    public void TableDefinitionScript(StringBuilder sb, DatabaseDescription.Table table, bool includeNondeterminisiticObjectIds)
    {
        _ = sb.Append(TableScript(table, includeNondeterminisiticObjectIds));
        _ = sb.Append(IndexesScript(table));
        _ = sb.Append(ForeignKeyConstraintsScript(table));
        _ = sb.Append(CheckConstraintsScript(table));
        _ = sb.Append(DefaultConstraintsScript(table));
        _ = sb.Append(TriggerScripts(table));
        _ = sb.Append($"--end of {table.QualifiedName} definition\n");
        _ = sb.Append('\n');
    }

    static string TableScript(DatabaseDescription.Table table, bool includeNondeterminisiticObjectIds)
    {
        var sb = new StringBuilder();
        var objectIdLineComment = includeNondeterminisiticObjectIds ? " --objectid:" + table.ObjectId : "";
        _ = sb.Append($"create table {table.QualifiedName} ({objectIdLineComment}\n");
        var separatorFromPreviousCol = "";
        foreach (var colMetaData in table.Columns) {
            if (colMetaData.ComputedAs is { } computedColumn) {
                var definition = computedColumn;
                var collationClause = colMetaData.CollationName == null
                    ? ""
                    : " collate " + colMetaData.CollationName;
                var persistedClause = !definition.IsPersisted
                    ? ""
                    : colMetaData.IsNullable
                        ? " persisted"
                        : " persisted not null";
                var columnTrivia = "--"
                    + colMetaData.ToSqlTypeNameWithoutNullability()
                    + ";"
                    + (colMetaData.IsPrimaryKey ? "PK;" : "")
                    + "colId:"
                    + colMetaData.ColumnId;
                _ = sb.Append("    " + separatorFromPreviousCol + colMetaData.ColumnName + " as " + SqlServerUtils.PrettifySqlExpression(definition.Definition) + collationClause + persistedClause + columnTrivia + "\n");
            } else {
                var identitySpecification = colMetaData.HasAutoIncrementIdentity ? " identity" : "";
                var columnTrivia = "--"
                    + (colMetaData.DefaultValueConstraint is not null ? "hasDefault;" : "")
                    + (colMetaData.IsPrimaryKey ? "PK;" : "")
                    + "colId:"
                    + colMetaData.ColumnId;
                _ = sb.Append("    " + separatorFromPreviousCol + colMetaData.ToSqlColumnDefinition() + identitySpecification + columnTrivia + "\n");
            }
            separatorFromPreviousCol = ", ";
        }
        return sb.Append(")\n").ToString();
    }

    static string IndexesScript(DatabaseDescription.Table table)
    {
        var sb = new StringBuilder();
        foreach (var index in table.Indexes.OrderByDescending(i => i.IndexType.IsClusteredIndex()).ThenBy(o => o.IndexName)) {
            _ = sb.Append(index.IndexCreationScript() + "\n");
        }
        return sb.ToString();
    }

    static string ForeignKeyConstraintsScript(DatabaseDescription.Table table)
    {
        var sb = new StringBuilder();
        foreach (var fk in table.KeysToReferencedParents.OrderBy(o => o.UnqualifiedName)) {
            _ = sb.Append(Regex.Replace(fk.ScriptToAddConstraint().CommandText().Trim(), "[\r \t\n]+", " ").Replace(" on delete no action on update no action", "") + "\n");
        }
        return sb.ToString();
    }

    static string CheckConstraintsScript(DatabaseDescription.Table table)
    {
        var sb = new StringBuilder();
        foreach (var ck in table.CheckConstraints.OrderBy(ck => ck.Name)) {
            _ = sb.Append(ToCreationStatement(table, ck) + "\n");
        }
        return sb.ToString();
    }

    static string DefaultConstraintsScript(DatabaseDescription.Table table)
    {
        var sb = new StringBuilder();
        var defaultConstraintsForTable = table.Columns
            .Where(col => col.DefaultValueConstraint != null)
            .Select(col => (col, defaultConstraint: col.DefaultValueConstraint.AssertNotNull()))
            .OrderBy(dvc => dvc.defaultConstraint.Name);
        foreach (var dfc in defaultConstraintsForTable) {
            _ = sb.Append($"alter table {table.QualifiedName} add constraint {dfc.defaultConstraint.Name} default {SqlServerUtils.PrettifySqlExpression(dfc.defaultConstraint.Definition)} for {dfc.col.ColumnName}\n");
        }
        return sb.ToString();
    }

    static string TriggerScripts(DatabaseDescription.Table table)
    {
        var sb = new StringBuilder();
        foreach (var dmlTrigger in table.Triggers.OrderBy(tr => tr.Name)) {
            _ = sb.Append("go\n");
            _ = sb.Append(dmlTrigger.Definition.Trim() + "\n");
            _ = sb.Append("go\n");
        }
        return sb.ToString();
    }

    public static string ToCreationStatement(DatabaseDescription.Table table, CheckConstraintSqlDefinition checkConstraintDefinition)
    {
        var creation = checkConstraintDefinition.IsNotTrusted
            ? $"alter table {table.QualifiedName} with nocheck add constraint {checkConstraintDefinition.Name} check {SqlServerUtils.PrettifySqlExpression(checkConstraintDefinition.Definition)}"
            : $"alter table {table.QualifiedName} add constraint {checkConstraintDefinition.Name} check {SqlServerUtils.PrettifySqlExpression(checkConstraintDefinition.Definition)}";
        var disabled = checkConstraintDefinition.IsDisabled ? $"\nalter table {table.QualifiedName} nocheck constraint {checkConstraintDefinition.Name}" : "";
        return creation + disabled;
    }

    public string StringifySchema(bool includeNondeterminisiticObjectIds)
    {
        var sb = new StringBuilder();
        foreach (var sequence in db.Sequences.Values.OrderBy(s => s.QualifiedName)) {
            sequence.AppendCreationScript(sb);
        }
        _ = sb.Append('\n');

        foreach (var table in db.AllTables.OrderBy(o => o.QualifiedName)) {
            TableDefinitionScript(sb, table, includeNondeterminisiticObjectIds);
        }
        return sb.ToString();
    }
}
