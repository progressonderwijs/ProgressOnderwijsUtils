namespace ProgressOnderwijsUtils.SchemaReflection;

public sealed class DatabaseDescription
{
    public RawDatabaseDescription RawDescription { get; }
    public readonly IReadOnlyDictionary<string, SequenceSqlDefinition> Sequences;
    readonly IReadOnlyDictionary<DbObjectId, Table> tableById;
    readonly IReadOnlyDictionary<DbObjectId, Table> tableTypeById;
    readonly IReadOnlyDictionary<DbObjectId, View> viewById;
    readonly IReadOnlyDictionary<string, Table> tableByQualifiedName;
    public readonly ILookup<string, ForeignKey> ForeignKeyConstraintsByUnqualifiedName;
    readonly ILookup<DbObjectId, ForeignKey> fksByReferencedParentObjectId;
    readonly ILookup<DbObjectId, ForeignKey> fksByReferencingChildObjectId;

    public DatabaseDescription(RawDatabaseDescription rawDescription)
    {
        RawDescription = rawDescription;
        var rawSchemaById = rawDescription.IndexById();

        Sequences = rawDescription.Sequences.ToDictionary(s => s.QualifiedName, StringComparer.OrdinalIgnoreCase);
        tableById = rawDescription.Tables.ToDictionary(o => o.ObjectId, o => new Table(o, rawSchemaById, this));
        tableTypeById = rawDescription.TableTypes.ToDictionary(o => o.ObjectId, o => new Table(o, rawSchemaById, this));
        viewById = rawDescription.Views.ToDictionary(o => o.ObjectId, o => new View(o, rawSchemaById, this));
        var fkObjects = rawDescription.ForeignKeys.ArraySelect(o => new ForeignKey(o, tableById));
        fksByReferencedParentObjectId = fkObjects.ToLookup(fk => fk.ReferencedParentTable.ObjectId);
        fksByReferencingChildObjectId = fkObjects.ToLookup(fk => fk.ReferencingChildTable.ObjectId);
        tableByQualifiedName = tableById.Values.ToDictionary(o => o.QualifiedName, StringComparer.OrdinalIgnoreCase);
        ForeignKeyConstraintsByUnqualifiedName = fkObjects.ToLookup(o => o.UnqualifiedName, StringComparer.OrdinalIgnoreCase);
    }

    public static DatabaseDescription LoadFromSchemaTables(SqlConnection conn)
        => new(RawDatabaseDescription.Load(conn));

    public IEnumerable<Table> AllTables
        => tableById.Values;

    public IEnumerable<Table> AllTablesTypes
        => tableTypeById.Values;

    public IEnumerable<View> AllViews
        => viewById.Values;

    public IEnumerable<CheckConstraintSqlDefinition> AllCheckConstraints
        => AllTables.SelectMany(t => t.CheckConstraints);

    public Table GetTableByName(string qualifiedName)
        => TryGetTableByName(qualifiedName) ?? throw new ArgumentException($"Unknown table '{qualifiedName}'.", nameof(qualifiedName));

    public Table? TryGetTableByName(string qualifiedName)
        => tableByQualifiedName.TryGetValue(qualifiedName, out var id) ? id : null;

    public Table? TryGetTableById(DbObjectId id)
        => tableById.GetValueOrDefault(id);

    public sealed record ForeignKeyColumn(Column<Table> ReferencedParentColumn, Column<Table> ReferencingChildColumn);

    public sealed class ForeignKey
    {
        public readonly Table ReferencedParentTable;
        public readonly Table ReferencingChildTable;
        public readonly IReadOnlyList<ForeignKeyColumn> Columns;
        public readonly string UnqualifiedName;
        public readonly FkReferentialAction DeleteReferentialAction;
        public readonly FkReferentialAction UpdateReferentialAction;

        internal ForeignKey(ForeignKeySqlDefinition fkDef, IReadOnlyDictionary<DbObjectId, Table> tablesById)
        {
            ReferencedParentTable = tablesById[fkDef.ReferencedParentTable];
            ReferencingChildTable = tablesById[fkDef.ReferencingChildTable];
            Columns = fkDef.Columns.ArraySelect(pair => new ForeignKeyColumn(ReferencedParentTable.ColumnsById[pair.ReferencedParentColumn], ReferencingChildTable.ColumnsById[pair.ReferencingChildColumn]));
            UnqualifiedName = fkDef.ConstraintName;
            DeleteReferentialAction = fkDef.DeleteReferentialAction;
            UpdateReferentialAction = fkDef.UpdateReferentialAction;
        }

        public string QualifiedName
            => $"{ReferencingChildTable.SchemaName}.{UnqualifiedName}";

        public ParameterizedSql ScriptToAddConstraint()
            => SQL(
                $@"
                    alter table {ReferencingChildTable.QualifiedNameSql}
                    add constraint {ParameterizedSql.CreateDynamic(UnqualifiedName)}
                        foreign key ({ParameterizedSql.CreateDynamic(Columns.Select(fkc => fkc.ReferencingChildColumn.ColumnName).JoinStrings(", "))}) 
                        references {ReferencedParentTable.QualifiedNameSql}
                            ({ParameterizedSql.CreateDynamic(Columns.Select(fkc => fkc.ReferencedParentColumn.ColumnName).JoinStrings(", "))})
                        on delete {DeleteReferentialAction.AsSql()}
                        on update {UpdateReferentialAction.AsSql()};
                    "
            );

        public ParameterizedSql ScriptToDropConstraint()
            => SQL($"alter table {ReferencingChildTable.QualifiedNameSql} drop constraint {ParameterizedSql.CreateDynamic(UnqualifiedName)};\n");
    }

    public sealed class Index<TObject>
        where TObject : ObjectWithColumns<TObject>
    {
        public readonly TObject ContainingObject;
        readonly DbObjectIndex IndexMetaData;

        internal Index(TObject containingObject, DbObjectIndex indexMetaData, DatabaseDescriptionById dataByTableId)
        {
            ContainingObject = containingObject;
            IndexMetaData = indexMetaData;

            List<IndexColumn> cols = new(), included = new();
            foreach (var col in dataByTableId.IndexColumns[(ObjectId, IndexId)]) {
                (col.IsIncluded ? included : cols).Add(new(this, col));
            }
            IndexColumns = cols.ToArray();
            Array.Sort(IndexColumns, colOrdering);
            IncludedColumns = included.ToArray();
            Array.Sort(IncludedColumns, colOrdering);
        }

        static readonly Comparison<IndexColumn> colOrdering = (a, b) => (a.UnderlyingMetaData.KeyOrdinal, a.UnderlyingMetaData.IndexColumnId).CompareTo((b.UnderlyingMetaData.KeyOrdinal, b.UnderlyingMetaData.IndexColumnId));
        public IndexColumn[] IncludedColumns { get; }
        public IndexColumn[] IndexColumns { get; }

        public readonly record struct IndexColumn(Index<TObject> Index, DbObjectIndexColumn UnderlyingMetaData)
        {
            public Column<TObject> Column
                => Index.ContainingObject.ColumnsById[UnderlyingMetaData.ColumnId];

            public bool IsDescending
                => UnderlyingMetaData.IsDescending;
        }

        public DbObjectId ObjectId
            => IndexMetaData.ObjectId;

        public DbIndexId IndexId
            => IndexMetaData.IndexId;

        public SqlIndexType IndexType
            => IndexMetaData.IndexType;

        public string? IndexName
            => IndexMetaData.IndexName;

        public string IndexCreationScript()
        {
            var include = IndexType.IsColumnStore() || IncludedColumns.None() ? "" : $" include ({IncludedColumns.Select(col => col.Column.ColumnName).JoinStrings(", ")})";
            var filter = Filter is null ? "" : $" where {SqlServerUtils.PrettifySqlExpression(Filter)}";
            var compression = DataCompressionType is SqlCompressionType.None or SqlCompressionType.ColumnStore ? "" : $" with (data_compression={DataCompressionType.ToSqlName().CommandText()})";
            var defaultIndexType = IsPrimaryKey ? SqlIndexType.ClusteredIndex : SqlIndexType.NonClusteredIndex;
            var indexType = IndexType == defaultIndexType ? "" : " " + IndexType.ToSqlName().CommandText();
            var indexColumns = IndexType == SqlIndexType.ClusteredColumnStore
                ? ""
                : $" ({IndexColumns.Select(col => col.Column.ColumnName + (col.IsDescending ? " desc" : "")).JoinStrings(", ")})";

            if (IsPrimaryKey || IsUniqueConstraint) {
                var constraintType = IsPrimaryKey ? " primary key" : " unique";
                return $"alter table {ContainingObject.QualifiedName} add constraint {IndexName}{constraintType}{indexType}{indexColumns}{compression}";
            } else if (IndexType == SqlIndexType.Heap) {
                return $"-- {ContainingObject.QualifiedName} is a heap{compression}";
            } else {
                var unique = IsUnique ? " unique" : "";
                return $"create{unique}{indexType} index {IndexName} on {ContainingObject.QualifiedName}{indexColumns}{include}{filter}{compression}";
            }
        }

        public bool IsUnique
            => IndexMetaData.IsUnique;

        public bool IsUniqueConstraint
            => IndexMetaData.IsUniqueConstraint;

        public bool IsPrimaryKey
            => IndexMetaData.IsPrimaryKey;

        public SqlCompressionType DataCompressionType
            => IndexMetaData.DataCompressionType;

        public string? Filter
            => IndexMetaData.Filter;
    }

    public sealed class Column<TObject> : IDbColumn
        where TObject : IDbNamedObject
    {
        public readonly TObject ContainingObject;
        public DbColumnMetaData ColumnMetaData { get; }
        public readonly DefaultValueConstraintSqlDefinition? DefaultValueConstraint;
        public readonly ComputedColumnSqlDefinition? ComputedAs;

        internal Column(TObject containingObject, DbColumnMetaData columnMetaData, DatabaseDescriptionById rawSchemaById)
        {
            ColumnMetaData = columnMetaData;
            ContainingObject = containingObject;
            DefaultValueConstraint = rawSchemaById.DefaultValues.GetValueOrDefault((columnMetaData.DbObjectId, columnMetaData.ColumnId));
            ComputedAs = rawSchemaById.ComputedColumns.GetValueOrDefault((columnMetaData.DbObjectId, columnMetaData.ColumnId));
        }

        public DbColumnId ColumnId
            => ColumnMetaData.ColumnId;

        public string ColumnName
            => ColumnMetaData.ColumnName;

        public bool IsPrimaryKey
            => ColumnMetaData.IsPrimaryKey;

        public bool IsRowVersion
            => ColumnMetaData.IsRowVersion;

        public bool IsComputed
            => ColumnMetaData.IsComputed;

        public bool IsNullable
            => ColumnMetaData.IsNullable;

        public SqlSystemTypeId UserTypeId
            => ColumnMetaData.UserTypeId;

        public ParameterizedSql SqlColumnName()
            => ColumnMetaData.SqlColumnName();

        public bool IsString
            => ColumnMetaData.IsString;

        public bool IsUnicode
            => ColumnMetaData.IsUnicode;

        public short MaxLength
            => ColumnMetaData.MaxLength;

        public byte Precision
            => ColumnMetaData.Precision;

        public byte Scale
            => ColumnMetaData.Scale;

        public bool HasAutoIncrementIdentity
            => ColumnMetaData.HasAutoIncrementIdentity;

        public string? CollationName
            => ColumnMetaData.CollationName;
    }

    static Column<TObject> DefineColumn<TObject>(ObjectWithColumns<TObject> containingObject, DatabaseDescriptionById rawSchemaById, DbColumnMetaData col)
        where TObject : ObjectWithColumns<TObject>
        => new((TObject)containingObject, col, rawSchemaById);

    public abstract class ObjectWithColumns<TObject> : IDbNamedObject
        where TObject : ObjectWithColumns<TObject>
    {
        private protected ObjectWithColumns(DbNamedObjectId namedObjectId, DatabaseDescriptionById rawSchemaById, DatabaseDescription database)
        {
            Database = database;
            NamedObjectId = namedObjectId;
            Columns = rawSchemaById.Columns.GetValueOrDefault(namedObjectId.ObjectId).EmptyIfNull().ArraySelect(col => DefineColumn(this, rawSchemaById, col));
            ColumnsById = Columns.ToDictionary(o => o.ColumnId);
            Indexes = rawSchemaById.Indexes[namedObjectId.ObjectId].Select(index => new Index<TObject>((TObject)this, index, rawSchemaById)).ToArray();
        }

        public Column<TObject>[] Columns { get; }
        public IReadOnlyDictionary<DbColumnId, Column<TObject>> ColumnsById { get; }
        protected readonly DbNamedObjectId NamedObjectId;
        public DatabaseDescription Database { get; }
        public Index<TObject>[] Indexes { get; }

        public DbObjectId ObjectId
            => NamedObjectId.ObjectId;

        public string QualifiedName
            => NamedObjectId.QualifiedName;
    }

    public sealed class Table : ObjectWithColumns<Table>
    {
        public readonly TriggerSqlDefinition[] DmlTableTriggers;
        public readonly CheckConstraintSqlDefinition[] CheckConstraints;

        internal Table(DbNamedObjectId namedTableId, DatabaseDescriptionById rawSchemaById, DatabaseDescription database) : base(namedTableId, rawSchemaById, database)
        {
            DmlTableTriggers = rawSchemaById.DmlTableTriggers.GetValueOrDefault(ObjectId).EmptyIfNull();
            CheckConstraints = rawSchemaById.CheckConstraints.GetValueOrDefault(ObjectId).EmptyIfNull();
        }

        public string SchemaName
            => DbQualifiedNameUtils.SchemaFromQualifiedName(QualifiedName);

        public string UnqualifiedName
            => DbQualifiedNameUtils.UnqualifiedTableName(QualifiedName);

        public ParameterizedSql QualifiedNameSql
            => ParameterizedSql.CreateDynamic(QualifiedName);

        public IEnumerable<Column<Table>> PrimaryKey
            => Columns.Where(c => c.IsPrimaryKey);

        public IEnumerable<Table> AllDependantTables
            => Utils.TransitiveClosure(
                new[] { ObjectId, },
                reachable => Database.fksByReferencedParentObjectId[reachable].Select(fk => fk.ReferencingChildTable.ObjectId)
            ).Select(id => Database.tableById[id]);

        public IEnumerable<ForeignKey> KeysToReferencedParents
            => Database.fksByReferencingChildObjectId[ObjectId];

        public IEnumerable<ForeignKey> KeysFromReferencingChildren
            => Database.fksByReferencedParentObjectId[ObjectId];

        public ForeignKeyInfo[] ChildColumnsReferencingColumn(string pkColumn)
            => KeysFromReferencingChildren
                .SelectMany(
                    fk =>
                        fk.Columns
                            .Where(fkCol => fkCol.ReferencedParentColumn.ColumnName.EqualsOrdinalCaseInsensitive(pkColumn))
                            .Select(fkCol => new ForeignKeyInfo(fk.ReferencingChildTable.QualifiedName, fkCol.ReferencingChildColumn.ColumnName))
                ).ToArray();
    }

    public sealed class View : ObjectWithColumns<View>
    {
        public readonly Table[] ReferencedTables;

        internal View(DbNamedObjectId namedObject, DatabaseDescriptionById rawSchemaById, DatabaseDescription db) : base(namedObject, rawSchemaById, db)
        {
            ReferencedTables = rawSchemaById.SqlExpressionDependsOn[namedObject.ObjectId].Select(db.TryGetTableById).WhereNotNull().ToArray();
        }

        public string SchemaName
            => DbQualifiedNameUtils.SchemaFromQualifiedName(QualifiedName);

        public string UnqualifiedName
            => DbQualifiedNameUtils.UnqualifiedTableName(QualifiedName);

        public ParameterizedSql QualifiedNameSql
            => ParameterizedSql.CreateDynamic(QualifiedName);
    }
}
