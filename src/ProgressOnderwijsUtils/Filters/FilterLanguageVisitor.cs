using System;
using ProgressOnderwijsUtils.FilterLanguage;

namespace ProgressOnderwijsUtils
{
    sealed class FilterLanguageVisitor : FilterLanguageBaseVisitor<FilterBase>
    {
        public override FilterBase VisitBinaryCriteriumWithColumn(FilterLanguageParser.BinaryCriteriumWithColumnContext context)
        {
            return Filter.CreateCriterium(
                context.Left.GetText(),
                Filter.ParseComparerSerializationString(context.binaryComparer().GetText()).Value,
                new ColumnReference(context.Right.GetText()));
        }
    }
}
