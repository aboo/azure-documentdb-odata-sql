namespace Microsoft.Azure.Documents.OData.Sql
{
	/// <summary>
	/// Provides constant string/char values
	/// </summary>
	public static class Constants
	{
		#region SQL Clauses

		/// <summary>
		/// Sql "SELECT" clause
		/// </summary>
		public const string SQLSelectSymbol = "SELECT";

		public const string SQLValueSymbol = "VALUE";

		/// <summary>
		/// Sql "WHERE" clause
		/// </summary>
		public const string SQLWhereSymbol = "WHERE";

		public const string SQLJoinSymbol = "JOIN";

		public const string SQLInSymbol = "IN";

		/// <summary>
		/// Sql "TOP" clause
		/// </summary>
		public const string SQLTopSymbol = "TOP";

		/// <summary>
		/// Sql "FROM" clause
		/// </summary>
		public const string SQLFromSymbol = "FROM";

		/// <summary>
		/// Sql "ORDER BY" clause
		/// </summary>
		public const string SQLOrderBySymbol = "ORDER BY";

		/// <summary>
		/// Sql "UPPER" function
		/// </summary>
		public const string SQLUpperSymbol = "UPPER";

		/// <summary>
		/// Sql "LOWER" function
		/// </summary>
		public const string SQLLowerSymbol = "LOWER";

		/// <summary>
		/// Sql "INDEX_OF" function
		/// </summary>
		public const string SQLIndexOfSymbol = "INDEX_OF";

		/// <summary>
		/// Sql "LTRIM" function
		/// </summary>
		public const string SQLLtrimSymbol = "LTRIM";

		/// <summary>
		/// Sql "RTRIM" function
		/// </summary>
		public const string SQLRtrimSymbol = "RTRIM";

		/// <summary>
		/// Sql "*" wild card
		/// </summary>
		public const string SQLAsteriskSymbol = "*";

		/// <summary>
		/// Sql <c>"c"</c> default filed name
		/// </summary>
		public const string SQLFieldNameSymbol = "c";

		/// <summary>
		/// Sql equal operator
		/// </summary>
		public const string SQLEqualSymbol = "=";

		/// <summary>
		/// Sql not equal operator
		/// </summary>
		public const string SQLNotEqualSymbol = "!=";

		/// <summary>
		/// Sql greater than operator
		/// </summary>
		public const string SQLGreaterThanSymbol = ">";

		/// <summary>
		/// Sql greater than or equal operator
		/// </summary>
		public const string SQLGreaterThanOrEqualSymbol = ">=";

		/// <summary>
		/// Sql less than operator
		/// </summary>
		public const string SQLLessThanSymbol = "<";

		/// <summary>
		/// Sql less than or equal operator
		/// </summary>
		public const string SQLLessThanOrEqualSymbol = "<=";

		/// <summary>
		/// Sql "AND" operator
		/// </summary>
		public const string SQLAndSymbol = "AND";

		/// <summary>
		/// Sql "OR" operator
		/// </summary>
		public const string SQLOrSymbol = "OR";

		/// <summary>
		/// Sql "DISTINCT" keyword
		/// </summary>
		public const string SQLDistinct = "DISTINCT";
		#endregion

		#region Symbols
		/// <summary>
		/// '?' Aggregator to merge request parameters
		/// </summary>
		public const char RequestParamsPrefix = '?';

		/// <summary>
		/// <c>'&amp;'</c> constant as aggregator to merge request parameters
		/// </summary>
		public const char RequestParamsAggregator = '&';

		/// <summary>
		/// '/' constant to represent the forward slash used in a query.
		/// </summary>
		public const string SymbolForwardSlash = "/";

		/// <summary>
		/// '.' constant to represent the dot.
		/// </summary>
		public const string SymbolDot = ".";

		/// <summary>
		/// ":" keyword for expression.
		/// </summary>
		public const string SymbolColon = ":";

		/// <summary>
		/// '=' constant to represent an assignment in name=value
		/// </summary>
		public const char SymbolEqual = '=';

		/// <summary>
		/// '(' constant to represent an open parenthesis
		/// </summary>
		public const char SymbolOpenParen = '(';

		/// <summary>
		/// ')' constant to represent an closed parenthesis
		/// </summary>
		public const char SymbolClosedParen = ')';

		/// <summary>
		/// '\'' constant to represent a single quote as prefix/suffix for literals
		/// /// </summary>
		public const char SymbolSingleQuote = '\'';

		/// <summary>
		/// ' ' constant to represent a single white space for literals
		/// /// </summary>
		public const char SymbolSpace = ' ';

		/// <summary>
		/// ',' constant to represent a single comma for literals
		/// /// </summary>
		public const char SymbolComma = ',';

		/// <summary>
		/// '-' constant to represent an negate unary operator.
		/// </summary>
		public const string SymbolNegate = "-";

		/// <summary>
		/// Empty JSON Array
		/// </summary>
		public const string SymbolEmptyJsonArray = "[]";

		/// <summary>
		/// Open Square Bracket
		/// </summary>
		public const string SymbolOpenSquareBracket = "[";

		/// <summary>
		/// Close Square Bracket
		/// </summary>
		public const string SymbolCloseSquareBracket = "]";

		/// <summary>
		/// Open Curly Bracket
		/// </summary>
		public const string SymbolOpenCurlyBracket = "{";

		/// <summary>
		/// Begin a JSON Array of string
		/// </summary>
		public const string SymbolBeginJsonStringArray = "[\"";

		/// <summary>
		/// End a JSON Array of string
		/// </summary>
		public const string SymbolEngJsonStringArray = "\"]";
		#endregion

		#region Keywords
		/// <summary>
		/// <c>"asc"</c> keyword for expressions.
		/// </summary>
		public const string KeywordAscending = "asc";

		/// <summary>
		/// <c>"desc"</c> keyword for expressions.
		/// </summary>
		public const string KeywordDescending = "desc";

		/// <summary>
		/// <c>"add"</c> keyword for expressions.
		/// </summary>
		public const string KeywordAdd = "add";

		/// <summary>
		/// <c>"sub"</c> keyword for expressions.
		/// </summary>
		public const string KeywordSub = "sub";

		/// <summary>
		/// <c>"div"</c> keyword for expressions.
		/// </summary>
		public const string KeywordDivide = "div";

		/// <summary>
		/// <c>"mod"</c> keyword for expressions.
		/// </summary>
		public const string KeywordModulo = "mod";

		/// <summary>
		/// <c>"mul"</c> keyword for expressions.
		/// </summary>
		public const string KeywordMultiply = "mul";

		/// <summary>
		/// <c>"toupper"</c> keyword for expressions.
		/// </summary>
		public const string KeywordToUpper = "toupper";

		/// <summary>
		/// <c>"tolower"</c> keyword for expressions.
		/// </summary>
		public const string KeywordToLower = "tolower";

		/// <summary>
		/// <c>"indexof"</c> keyword for expressions.
		/// </summary>
		public const string KeywordIndexOf = "indexof";

		/// <summary>
		/// <c>"trim"</c> keyword for expressions.
		/// </summary>
		public const string KeywordTrim = "trim";

		/// <summary>
		/// <c>'&amp;'</c> constant to represent the concatenation of query parts.
		/// </summary>
		public const string SymbolQueryConcatenate = "&";

		/// <summary>
		/// "$" sign that starts with all built-in query selectors such as $filter and $orderby
		/// </summary>
		public const string SymbolQuerySelector = "$";

		/// <summary>
		/// the expression operator and
		/// </summary>
		public const string KeywordAnd = "and";

		/// <summary>
		/// the expression operator or
		/// </summary>
		public const string KeywordOr = "or";

		/// <summary>
		/// the expression operator greater than
		/// </summary>
		public const string KeywordGreaterThan = "gt";

		/// <summary>
		/// the expression operator greater than or equal
		/// </summary>
		public const string KeywordGreaterThanOrEqual = "ge";

		/// <summary>
		/// the expression operator less than
		/// </summary>
		public const string KeywordLessThan = "lt";

		/// <summary>
		/// the expression operator less than or equal to
		/// </summary>
		public const string KeywordLessThanOrEqual = "le";

		/// <summary>
		/// the expression operator less than or equal to
		/// </summary>
		public const string KeywordEqual = "eq";

		/// <summary>
		/// the expression operator not equal
		/// </summary>
		public const string KeywordNotEqual = "ne";

		/// <summary>
		/// "all" keyword for expressions.
		/// </summary>
		public const string KeywordAll = "all";

		/// <summary>
		/// "any" keyword for expressions.
		/// </summary>
		public const string KeywordAny = "any";

		/// <summary>
		/// "null" keyword for expressions.
		/// </summary>
		public const string KeywordNull = "null";

		/// <summary>
		/// "not" keyword for expressions.
		/// </summary>
		public const string KeywordNot = "not";

		/// <summary>
		/// "max" keyword for expressions.
		/// </summary>
		public const string KeywordMax = "max";

		/// <summary>
		/// "has" keyword for expressions.
		/// </summary>
		public const string KeywordHas = "has";

		/// <summary>
		/// "NOT" keyword for search option.
		/// </summary>
		public const string SearchKeywordNot = "NOT";

		/// <summary>
		/// "AND" keyword for search option.
		/// </summary>
		public const string SearchKeywordAnd = "AND";

		/// <summary>
		/// "OR" keyword for search option.
		/// </summary>
		public const string SearchKeywordOr = "OR";
		#endregion
	}
}
