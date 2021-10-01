using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Azure.Documents.OData.Sql.Extensions;
using Microsoft.OData.UriParser;

#if NET462
using System.Web.OData.Query;
#endif

#if NETSTANDARD2_0
using Microsoft.AspNet.OData.Query;
#endif

namespace Microsoft.Azure.Documents.OData.Sql
{
	/// <summary>
	/// TranslateOptions
	/// </summary>
	[Flags]
	public enum TranslateOptions
	{
		/// <summary>
		/// translate option for Sql SELECT clause
		/// </summary>
		SELECT_CLAUSE = 1,

		/// <summary>
		/// translate option for Sql WHERE clause
		/// </summary>
		WHERE_CLAUSE = 1 << 1,

		/// <summary>
		/// translate option for Sql ORDER BY clause
		/// </summary>
		ORDERBY_CLAUSE = 1 << 2,

		/// <summary>
		/// translate option for sql TOP clause
		/// </summary>
		TOP_CLAUSE = 1 << 3,

		JOIN_CLAUSE = 1 << 4,

		/// <summary>
		/// translate option for all Sql clauses: SELECT, WHERE, ORDER BY, and TOP
		/// </summary>
		ALL = SELECT_CLAUSE | WHERE_CLAUSE | ORDERBY_CLAUSE | TOP_CLAUSE | JOIN_CLAUSE
	}

	/// <summary>
	/// ODataToSqlTranslator
	/// </summary>
	public class ODataToSqlTranslator
	{
		private string[] _joinClauses = new string[] { };

		/// <summary>
		/// function that takes in an <see cref="ODataQueryOptions"/>, a string representing the type to filter, and a <see cref="FeedOptions"/>
		/// </summary>
		/// <param name="odataQueryOptions"></param>
		/// <param name="translateOptions"></param>
		/// <param name="additionalWhereClause"></param>
		/// <returns>returns an SQL expression if successfully translated, otherwise a null string</returns>
		public string Translate(ODataQueryOptions odataQueryOptions, TranslateOptions translateOptions, string additionalWhereClause = null)
		{
			string selectClause, whereClause, orderbyClause, topClause, joinClause;
			selectClause = whereClause = orderbyClause = topClause = joinClause = string.Empty;

			// SELECT CLAUSE
			if ((translateOptions & TranslateOptions.SELECT_CLAUSE) == TranslateOptions.SELECT_CLAUSE)
			{
				// TOP CLAUSE
				if ((translateOptions & TranslateOptions.TOP_CLAUSE) == TranslateOptions.TOP_CLAUSE)
				{
					topClause = odataQueryOptions?.Top?.Value > 0
							? $"{Constants.SQLTopSymbol} {odataQueryOptions.Top.Value} "
							: string.Empty;
				}

				selectClause = odataQueryOptions?.SelectExpand?.RawSelect == null
						? "*"
						: string.Join(", ", odataQueryOptions.SelectExpand.RawSelect.Split(',').Select(c => string.Concat("c.", c.Trim())));
				selectClause = $"{Constants.SQLSelectSymbol} {topClause}{selectClause} {Constants.SQLFromSymbol} {Constants.SQLFieldNameSymbol} ";
			}

			// WHERE CLAUSE
			if ((translateOptions & TranslateOptions.WHERE_CLAUSE) == TranslateOptions.WHERE_CLAUSE)
			{
				var customWhereClause = additionalWhereClause == null
						? string.Empty
						: $"{additionalWhereClause}";
				whereClause = odataQueryOptions?.Filter?.FilterClause == null
						? string.Empty
						: $"{this.TranslateFilterClause(odataQueryOptions.Filter.FilterClause)}";
				whereClause = (!string.IsNullOrEmpty(customWhereClause) && !string.IsNullOrEmpty(whereClause))
						? $"({customWhereClause}) AND ({whereClause})"
						: $"{customWhereClause}{whereClause}";
				whereClause = string.IsNullOrEmpty(whereClause)
						? string.Empty
						: $"{Constants.SQLWhereSymbol} {whereClause} ";
			}

			// ORDER BY CLAUSE
			if ((translateOptions & TranslateOptions.ORDERBY_CLAUSE) == TranslateOptions.ORDERBY_CLAUSE)
			{
				orderbyClause = odataQueryOptions?.OrderBy?.OrderByClause == null
						? string.Empty
						: $"{Constants.SQLOrderBySymbol} {this.TranslateOrderByClause(odataQueryOptions.OrderBy.OrderByClause)} ";
			}

			joinClause = string.Join(string.Empty, _joinClauses);
			if (joinClause.Any())
			{
				selectClause = selectClause.Replace($"{Constants.SQLSelectSymbol} {Constants.SQLAsteriskSymbol}",
					$"{Constants.SQLSelectSymbol} {Constants.SQLValueSymbol} {Constants.SQLFieldNameSymbol}");
			}

			var sqlQuery = string.Concat(selectClause, joinClause, whereClause, orderbyClause);
			return ModifyStringFunctions(sqlQuery);
		}

		/// <summary>
		/// Constructor for ODataSqlTranslator
		/// </summary>
		/// <param name="queryFormatter">Optional QueryFormatter, if no formatter provided, a SQLQueryFormatter is used by default</param>
		public ODataToSqlTranslator(QueryFormatterBase queryFormatter = null)
		{
			queryFormatter = queryFormatter ?? new SQLQueryFormatter();
			oDataNodeToStringBuilder = new ODataNodeToStringBuilder(queryFormatter);
		}

		/// <summary>
		/// 
		/// </summary>
		private ODataToSqlTranslator() { }

		/// <summary>Translates a <see cref="FilterClause"/> into a <see cref="FilterClause"/>.</summary>
		/// <param name="filterClause">The filter clause to translate.</param>
		/// <returns>The translated string.</returns>
		private string TranslateFilterClause(FilterClause filterClause)
		{
			var translation = oDataNodeToStringBuilder.TranslateNode(filterClause.Expression);
			var result = translation.FindAndTranslateJoin();
			translation = result.JoinCondition;
			_joinClauses = result.JoinClauses;

			translation = translation.FindAndTranslateAny();
			translation = translation.FindAndTranslateReservedKeywords();
			return translation;
		}

		/// <summary>Translates a <see cref="OrderByClause"/> into a <see cref="OrderByClause"/>.</summary>
		/// <param name="orderByClause">The orderBy clause to translate.</param>
		/// <param name="preExpr">expression built so far.</param>
		/// <returns>The translated string.</returns>
		private string TranslateOrderByClause(OrderByClause orderByClause, string preExpr = null)
		{
			string expr = string.Concat(oDataNodeToStringBuilder.TranslateNode(orderByClause.Expression), Constants.SymbolSpace, orderByClause.Direction == OrderByDirection.Ascending ? Constants.KeywordAscending.ToUpper() : Constants.KeywordDescending.ToUpper());

			expr = string.IsNullOrWhiteSpace(preExpr) ? expr : string.Concat(preExpr, Constants.SymbolComma, Constants.SymbolSpace, expr);

			if (orderByClause.ThenBy != null)
			{
				expr = this.TranslateOrderByClause(orderByClause.ThenBy, expr);
			}

			return expr;
		}

		/// <summary>
		/// Visitor patterned ODataNodeToStringBuilder
		/// </summary>
		private ODataNodeToStringBuilder oDataNodeToStringBuilder { get; set; }

		private string ModifyStringFunctions(string sqlQuery)
		{
			var updatedQuery = sqlQuery;
			var functionNames = new[] { "startswith" , "endswith", "contains" }; 
			var stringFunctionNames = new[] { Constants.SQLUpperSymbol, Constants.SQLLowerSymbol };

			foreach (var functionName in functionNames)
			{
				foreach (var stringFunctionName in stringFunctionNames)
				{
					updatedQuery = UpdateFunction(updatedQuery, functionName, stringFunctionName);
				}
			}

			return updatedQuery;
		}

		private string UpdateFunction(string sqlQuery, string functionName, string stringFunctionName)
		{
			var updatedQuery = sqlQuery;
			var stringsToUpdate = GetStringsToReplace(sqlQuery, functionName, stringFunctionName);

			foreach (var stringToUpdate in stringsToUpdate)
			{
				var propertyName = GetPropertyName(stringToUpdate, stringFunctionName);
				var updated = GetUpdatedQuery(stringToUpdate, propertyName, stringFunctionName);
				updatedQuery = updatedQuery.Replace(stringToUpdate, updated);
			}

			return updatedQuery;
		}

		private List<string> GetStringsToReplace(string query, string functionName, string stringFunctionName)
		{
			List<string> stringsToReplace = new List<string>();
			var regex = $@"{functionName}\({stringFunctionName}\(.*\),.*\)";
			var match = Regex.Match(query, regex, RegexOptions.IgnoreCase);
			if (match.Success)
			{
				foreach (Group matchGroup in match.Groups)
				{
					stringsToReplace.Add(matchGroup.Value);
				}
			}

			return stringsToReplace;
		}

		string GetPropertyName(string functionQuery, string functionName)
		{
			string propertyName = null;
			var regex = $@"{functionName}\((.*)\),";
			var match = Regex.Match(functionQuery, regex, RegexOptions.IgnoreCase);
			if (match.Success)
			{
				if (match.Groups.Count > 1)
				{
					propertyName = match.Groups[1].Value;
				}
			}

			return propertyName;
		}

		string GetUpdatedQuery(string query, string propertyName, string functionName)
		{
			var regex = $@"{functionName}\((.*)\),";
			var updatedQuery = Regex.Replace(query, regex, $"{propertyName},");
			var closing = updatedQuery.LastIndexOf(')');
			updatedQuery = updatedQuery.Substring(0, closing);
			return $"{updatedQuery}, true)";
		}
	}
}
