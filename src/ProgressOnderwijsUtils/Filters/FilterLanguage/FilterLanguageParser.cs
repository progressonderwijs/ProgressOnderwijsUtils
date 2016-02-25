//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.5.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from FilterLanguage.g4 by ANTLR 4.5.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace ProgressOnderwijsUtils.FilterLanguage {
using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.5.2")]
[System.CLSCompliant(false)]
public partial class FilterLanguageParser : Parser {
	public const int
		AndOp=1, OrOp=2, LessThan=3, LessThanOrEqual=4, Equal=5, GreaterThanOrEqual=6, 
		GreaterThan=7, NotEqual=8, StartsWith=9, EndsWith=10, Contains=11, In=12, 
		NotIn=13, HasFlag=14, IsNull=15, IsNotNull=16, ColumnName=17, Number=18, 
		LP=19, RP=20, WS=21;
	public const int
		RULE_combined = 0, RULE_criterium = 1, RULE_unaryComparer = 2, RULE_binaryComparer = 3, 
		RULE_columnName = 4;
	public static readonly string[] ruleNames = {
		"combined", "criterium", "unaryComparer", "binaryComparer", "columnName"
	};

	private static readonly string[] _LiteralNames = {
		null, "'and'", "'or'", "'<'", "'<='", "'='", "'>='", "'>'", "'!='", "'starts with'", 
		"'ends with'", "'contains'", "'in'", "'not in'", "'has flag'", "'is null'", 
		"'is not null'", null, null, "'('", "')'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "AndOp", "OrOp", "LessThan", "LessThanOrEqual", "Equal", "GreaterThanOrEqual", 
		"GreaterThan", "NotEqual", "StartsWith", "EndsWith", "Contains", "In", 
		"NotIn", "HasFlag", "IsNull", "IsNotNull", "ColumnName", "Number", "LP", 
		"RP", "WS"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "FilterLanguage.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string SerializedAtn { get { return _serializedATN; } }

	public FilterLanguageParser(ITokenStream input)
		: base(input)
	{
		Interpreter = new ParserATNSimulator(this,_ATN);
	}
	public partial class CombinedContext : ParserRuleContext {
		public CombinedContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_combined; } }
	 
		public CombinedContext() { }
		public virtual void CopyFrom(CombinedContext context) {
			base.CopyFrom(context);
		}
	}
	public partial class OrCombinedContext : CombinedContext {
		public CriteriumContext[] criterium() {
			return GetRuleContexts<CriteriumContext>();
		}
		public CriteriumContext criterium(int i) {
			return GetRuleContext<CriteriumContext>(i);
		}
		public ITerminalNode[] OrOp() { return GetTokens(FilterLanguageParser.OrOp); }
		public ITerminalNode OrOp(int i) {
			return GetToken(FilterLanguageParser.OrOp, i);
		}
		public OrCombinedContext(CombinedContext context) { CopyFrom(context); }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IFilterLanguageVisitor<TResult> typedVisitor = visitor as IFilterLanguageVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitOrCombined(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class AndCombinedContext : CombinedContext {
		public CriteriumContext[] criterium() {
			return GetRuleContexts<CriteriumContext>();
		}
		public CriteriumContext criterium(int i) {
			return GetRuleContext<CriteriumContext>(i);
		}
		public ITerminalNode[] AndOp() { return GetTokens(FilterLanguageParser.AndOp); }
		public ITerminalNode AndOp(int i) {
			return GetToken(FilterLanguageParser.AndOp, i);
		}
		public AndCombinedContext(CombinedContext context) { CopyFrom(context); }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IFilterLanguageVisitor<TResult> typedVisitor = visitor as IFilterLanguageVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitAndCombined(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public CombinedContext combined() {
		CombinedContext _localctx = new CombinedContext(Context, State);
		EnterRule(_localctx, 0, RULE_combined);
		int _la;
		try {
			State = 26;
			ErrorHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(TokenStream,2,Context) ) {
			case 1:
				_localctx = new AndCombinedContext(_localctx);
				EnterOuterAlt(_localctx, 1);
				{
				State = 10; criterium();
				State = 15;
				ErrorHandler.Sync(this);
				_la = TokenStream.La(1);
				while (_la==AndOp) {
					{
					{
					State = 11; Match(AndOp);
					State = 12; criterium();
					}
					}
					State = 17;
					ErrorHandler.Sync(this);
					_la = TokenStream.La(1);
				}
				}
				break;
			case 2:
				_localctx = new OrCombinedContext(_localctx);
				EnterOuterAlt(_localctx, 2);
				{
				State = 18; criterium();
				State = 23;
				ErrorHandler.Sync(this);
				_la = TokenStream.La(1);
				while (_la==OrOp) {
					{
					{
					State = 19; Match(OrOp);
					State = 20; criterium();
					}
					}
					State = 25;
					ErrorHandler.Sync(this);
					_la = TokenStream.La(1);
				}
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class CriteriumContext : ParserRuleContext {
		public CriteriumContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_criterium; } }
	 
		public CriteriumContext() { }
		public virtual void CopyFrom(CriteriumContext context) {
			base.CopyFrom(context);
		}
	}
	public partial class BinaryCriteriumWithNumberContext : CriteriumContext {
		public ColumnNameContext Left;
		public IToken Right;
		public BinaryComparerContext binaryComparer() {
			return GetRuleContext<BinaryComparerContext>(0);
		}
		public ColumnNameContext columnName() {
			return GetRuleContext<ColumnNameContext>(0);
		}
		public ITerminalNode Number() { return GetToken(FilterLanguageParser.Number, 0); }
		public BinaryCriteriumWithNumberContext(CriteriumContext context) { CopyFrom(context); }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IFilterLanguageVisitor<TResult> typedVisitor = visitor as IFilterLanguageVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitBinaryCriteriumWithNumber(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class UnaryCriteriumContext : CriteriumContext {
		public ColumnNameContext columnName() {
			return GetRuleContext<ColumnNameContext>(0);
		}
		public UnaryComparerContext unaryComparer() {
			return GetRuleContext<UnaryComparerContext>(0);
		}
		public UnaryCriteriumContext(CriteriumContext context) { CopyFrom(context); }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IFilterLanguageVisitor<TResult> typedVisitor = visitor as IFilterLanguageVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitUnaryCriterium(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class BinaryCriteriumWithColumnContext : CriteriumContext {
		public ColumnNameContext Left;
		public ColumnNameContext Right;
		public BinaryComparerContext binaryComparer() {
			return GetRuleContext<BinaryComparerContext>(0);
		}
		public ColumnNameContext[] columnName() {
			return GetRuleContexts<ColumnNameContext>();
		}
		public ColumnNameContext columnName(int i) {
			return GetRuleContext<ColumnNameContext>(i);
		}
		public BinaryCriteriumWithColumnContext(CriteriumContext context) { CopyFrom(context); }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IFilterLanguageVisitor<TResult> typedVisitor = visitor as IFilterLanguageVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitBinaryCriteriumWithColumn(this);
			else return visitor.VisitChildren(this);
		}
	}
	public partial class NestedCombinedContext : CriteriumContext {
		public ITerminalNode LP() { return GetToken(FilterLanguageParser.LP, 0); }
		public CombinedContext combined() {
			return GetRuleContext<CombinedContext>(0);
		}
		public ITerminalNode RP() { return GetToken(FilterLanguageParser.RP, 0); }
		public NestedCombinedContext(CriteriumContext context) { CopyFrom(context); }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IFilterLanguageVisitor<TResult> typedVisitor = visitor as IFilterLanguageVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitNestedCombined(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public CriteriumContext criterium() {
		CriteriumContext _localctx = new CriteriumContext(Context, State);
		EnterRule(_localctx, 2, RULE_criterium);
		try {
			State = 43;
			ErrorHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(TokenStream,3,Context) ) {
			case 1:
				_localctx = new UnaryCriteriumContext(_localctx);
				EnterOuterAlt(_localctx, 1);
				{
				State = 28; columnName();
				State = 29; unaryComparer();
				}
				break;
			case 2:
				_localctx = new BinaryCriteriumWithColumnContext(_localctx);
				EnterOuterAlt(_localctx, 2);
				{
				State = 31; ((BinaryCriteriumWithColumnContext)_localctx).Left = columnName();
				State = 32; binaryComparer();
				State = 33; ((BinaryCriteriumWithColumnContext)_localctx).Right = columnName();
				}
				break;
			case 3:
				_localctx = new BinaryCriteriumWithNumberContext(_localctx);
				EnterOuterAlt(_localctx, 3);
				{
				State = 35; ((BinaryCriteriumWithNumberContext)_localctx).Left = columnName();
				State = 36; binaryComparer();
				State = 37; ((BinaryCriteriumWithNumberContext)_localctx).Right = Match(Number);
				}
				break;
			case 4:
				_localctx = new NestedCombinedContext(_localctx);
				EnterOuterAlt(_localctx, 4);
				{
				State = 39; Match(LP);
				State = 40; combined();
				State = 41; Match(RP);
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class UnaryComparerContext : ParserRuleContext {
		public ITerminalNode IsNull() { return GetToken(FilterLanguageParser.IsNull, 0); }
		public ITerminalNode IsNotNull() { return GetToken(FilterLanguageParser.IsNotNull, 0); }
		public UnaryComparerContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_unaryComparer; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IFilterLanguageVisitor<TResult> typedVisitor = visitor as IFilterLanguageVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitUnaryComparer(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public UnaryComparerContext unaryComparer() {
		UnaryComparerContext _localctx = new UnaryComparerContext(Context, State);
		EnterRule(_localctx, 4, RULE_unaryComparer);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 45;
			_la = TokenStream.La(1);
			if ( !(_la==IsNull || _la==IsNotNull) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class BinaryComparerContext : ParserRuleContext {
		public ITerminalNode LessThan() { return GetToken(FilterLanguageParser.LessThan, 0); }
		public ITerminalNode LessThanOrEqual() { return GetToken(FilterLanguageParser.LessThanOrEqual, 0); }
		public ITerminalNode Equal() { return GetToken(FilterLanguageParser.Equal, 0); }
		public ITerminalNode GreaterThanOrEqual() { return GetToken(FilterLanguageParser.GreaterThanOrEqual, 0); }
		public ITerminalNode GreaterThan() { return GetToken(FilterLanguageParser.GreaterThan, 0); }
		public ITerminalNode NotEqual() { return GetToken(FilterLanguageParser.NotEqual, 0); }
		public ITerminalNode StartsWith() { return GetToken(FilterLanguageParser.StartsWith, 0); }
		public ITerminalNode EndsWith() { return GetToken(FilterLanguageParser.EndsWith, 0); }
		public ITerminalNode Contains() { return GetToken(FilterLanguageParser.Contains, 0); }
		public ITerminalNode In() { return GetToken(FilterLanguageParser.In, 0); }
		public ITerminalNode NotIn() { return GetToken(FilterLanguageParser.NotIn, 0); }
		public ITerminalNode HasFlag() { return GetToken(FilterLanguageParser.HasFlag, 0); }
		public BinaryComparerContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_binaryComparer; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IFilterLanguageVisitor<TResult> typedVisitor = visitor as IFilterLanguageVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitBinaryComparer(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public BinaryComparerContext binaryComparer() {
		BinaryComparerContext _localctx = new BinaryComparerContext(Context, State);
		EnterRule(_localctx, 6, RULE_binaryComparer);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 47;
			_la = TokenStream.La(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << LessThan) | (1L << LessThanOrEqual) | (1L << Equal) | (1L << GreaterThanOrEqual) | (1L << GreaterThan) | (1L << NotEqual) | (1L << StartsWith) | (1L << EndsWith) | (1L << Contains) | (1L << In) | (1L << NotIn) | (1L << HasFlag))) != 0)) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ColumnNameContext : ParserRuleContext {
		public ITerminalNode ColumnName() { return GetToken(FilterLanguageParser.ColumnName, 0); }
		public ITerminalNode AndOp() { return GetToken(FilterLanguageParser.AndOp, 0); }
		public ITerminalNode OrOp() { return GetToken(FilterLanguageParser.OrOp, 0); }
		public ITerminalNode In() { return GetToken(FilterLanguageParser.In, 0); }
		public ITerminalNode Contains() { return GetToken(FilterLanguageParser.Contains, 0); }
		public ColumnNameContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_columnName; } }
		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor) {
			IFilterLanguageVisitor<TResult> typedVisitor = visitor as IFilterLanguageVisitor<TResult>;
			if (typedVisitor != null) return typedVisitor.VisitColumnName(this);
			else return visitor.VisitChildren(this);
		}
	}

	[RuleVersion(0)]
	public ColumnNameContext columnName() {
		ColumnNameContext _localctx = new ColumnNameContext(Context, State);
		EnterRule(_localctx, 8, RULE_columnName);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 49;
			_la = TokenStream.La(1);
			if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << AndOp) | (1L << OrOp) | (1L << Contains) | (1L << In) | (1L << ColumnName))) != 0)) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	private static string _serializedATN = _serializeATN();
	private static string _serializeATN()
	{
	    StringBuilder sb = new StringBuilder();
	    sb.Append("\x3\x430\xD6D1\x8206\xAD2D\x4417\xAEF1\x8D80\xAADD\x3\x17");
		sb.Append("\x36\x4\x2\t\x2\x4\x3\t\x3\x4\x4\t\x4\x4\x5\t\x5\x4\x6\t\x6");
		sb.Append("\x3\x2\x3\x2\x3\x2\a\x2\x10\n\x2\f\x2\xE\x2\x13\v\x2\x3\x2\x3");
		sb.Append("\x2\x3\x2\a\x2\x18\n\x2\f\x2\xE\x2\x1B\v\x2\x5\x2\x1D\n\x2\x3");
		sb.Append("\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3\x3");
		sb.Append("\x3\x3\x3\x3\x3\x3\x3\x3\x3\x5\x3.\n\x3\x3\x4\x3\x4\x3\x5\x3");
		sb.Append("\x5\x3\x6\x3\x6\x3\x6\x2\x2\a\x2\x4\x6\b\n\x2\x5\x3\x2\x11\x12");
		sb.Append("\x3\x2\x5\x10\x5\x2\x3\x4\r\xE\x13\x13\x36\x2\x1C\x3\x2\x2\x2");
		sb.Append("\x4-\x3\x2\x2\x2\x6/\x3\x2\x2\x2\b\x31\x3\x2\x2\x2\n\x33\x3");
		sb.Append("\x2\x2\x2\f\x11\x5\x4\x3\x2\r\xE\a\x3\x2\x2\xE\x10\x5\x4\x3");
		sb.Append("\x2\xF\r\x3\x2\x2\x2\x10\x13\x3\x2\x2\x2\x11\xF\x3\x2\x2\x2");
		sb.Append("\x11\x12\x3\x2\x2\x2\x12\x1D\x3\x2\x2\x2\x13\x11\x3\x2\x2\x2");
		sb.Append("\x14\x19\x5\x4\x3\x2\x15\x16\a\x4\x2\x2\x16\x18\x5\x4\x3\x2");
		sb.Append("\x17\x15\x3\x2\x2\x2\x18\x1B\x3\x2\x2\x2\x19\x17\x3\x2\x2\x2");
		sb.Append("\x19\x1A\x3\x2\x2\x2\x1A\x1D\x3\x2\x2\x2\x1B\x19\x3\x2\x2\x2");
		sb.Append("\x1C\f\x3\x2\x2\x2\x1C\x14\x3\x2\x2\x2\x1D\x3\x3\x2\x2\x2\x1E");
		sb.Append("\x1F\x5\n\x6\x2\x1F \x5\x6\x4\x2 .\x3\x2\x2\x2!\"\x5\n\x6\x2");
		sb.Append("\"#\x5\b\x5\x2#$\x5\n\x6\x2$.\x3\x2\x2\x2%&\x5\n\x6\x2&\'\x5");
		sb.Append("\b\x5\x2\'(\a\x14\x2\x2(.\x3\x2\x2\x2)*\a\x15\x2\x2*+\x5\x2");
		sb.Append("\x2\x2+,\a\x16\x2\x2,.\x3\x2\x2\x2-\x1E\x3\x2\x2\x2-!\x3\x2");
		sb.Append("\x2\x2-%\x3\x2\x2\x2-)\x3\x2\x2\x2.\x5\x3\x2\x2\x2/\x30\t\x2");
		sb.Append("\x2\x2\x30\a\x3\x2\x2\x2\x31\x32\t\x3\x2\x2\x32\t\x3\x2\x2\x2");
		sb.Append("\x33\x34\t\x4\x2\x2\x34\v\x3\x2\x2\x2\x6\x11\x19\x1C-");
	    return sb.ToString();
	}

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN.ToCharArray());
}
} // namespace ProgressOnderwijsUtils.FilterLanguage
