grammar FilterLanguage;

combined:
    criterium (AndOp criterium)* # AndCombined
    | criterium (OrOp criterium)* # OrCombined
    ;

criterium:
    columnName unaryComparer # UnaryCriterium
    | Left=columnName binaryComparer Right=columnName # BinaryColumnCriterium
    | Left=columnName binaryComparer Right=Number # BinaryNumberCriterium
    | LP combined RP # NestedCombined
    ;

unaryComparer:
    IsNull
    | IsNotNull
    ;

binaryComparer:
    LessThan
    | LessThanOrEqual
    | Equal
    | GreaterThanOrEqual
    | GreaterThan
    | NotEqual
    | StartsWith
    | EndsWith
    | Contains
    | In
    | NotIn
    | HasFlag
    ;

columnName:
    ColumnName
    | AndOp
    | OrOp
    | Contains
    ;

AndOp: 'and';
OrOp: 'or';
LessThan: '<';
LessThanOrEqual: '<=';
Equal: '=';
GreaterThanOrEqual: '>=';
GreaterThan: '>';
NotEqual: '!=';
StartsWith: 'starts with';
EndsWith: 'ends with';
Contains: 'contains';
In: 'in';
NotIn: 'not in';
HasFlag: 'has flag';
IsNull: 'is null';
IsNotNull:'is not null';
ColumnName: [a-zA-Z_][a-zA-Z0-9_]*;
Number: [0-9]+(.[0-9]+)?;
LP: '(';
RP: ')';
WS: [ \r\n\t] + -> skip;
