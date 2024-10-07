using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.Documents.OData.Sql.Extensions
{
	public static class StringExtensions
	{
		public static string FindAndTranslateAny(this string translation)
		{
			const string anyRegEx = @"(c\.[\w\.\/]+)\/any\([^:]*:[^=]*= ([^)]*)\)";
			const string arrayContainsFormat = "ARRAY_CONTAINS({0},{1})";
			var finalTranslation = translation;

			var matches = Regex.Matches(translation, anyRegEx);
			foreach (Match match in matches)
			{
				var array = match.Groups[1];
				var value = match.Groups[2];
				var finalValue = string.Format(arrayContainsFormat, array, value);
				finalTranslation = finalTranslation.Replace(match.Value, finalValue);
			}

			return finalTranslation;
		}

		public static (string[] JoinClauses, string JoinCondition) FindAndTranslateJoin(this string translation, string useLetter = "c")
		{
			const int collectionIndex = 3;
			const int letterIndex = 4;
			const int propertyIndex = 5;
			const int operandIndex = 6;
			const int valueIndex = 8;
			const int contentIndex = 5;

			const string anyRegEx = @"(join (.) IN ([^\/]*))\/any\((.):([^=><]*([^\s]*)([^'\w]*)([^\)]*)\))";
			const string clauseTemplate = "{0}.{1} {2} {3}";
			var finalTranslation = translation;

			var matches = Regex.Matches(translation, anyRegEx, RegexOptions.IgnoreCase);
			var joinClauses = new List<string>();
			var subJoinClauses = new List<string>();

			foreach (Match match in matches)
			{
				var content = match.Groups[contentIndex];
				var letter = match.Groups[letterIndex].Value;
				var r = FindAndTranslateJoin(content.Value, letter);
				subJoinClauses.AddRange(r.JoinClauses);
				var subCondition = r.JoinCondition;

				var collection = match.Groups[collectionIndex].Value.Replace("c.", $"{useLetter}.");
				var operand = match.Groups[operandIndex].Value.Trim();
				var property = string.Join(Constants.SymbolDot,
					match.Groups[propertyIndex].Value.Split(operand[0])[0].Split('.').Where(x => x.Length > 1).Select(x => x.Trim()));
				var value = match.Groups[valueIndex];

				var finalValue = !string.IsNullOrWhiteSpace(subCondition) && subCondition.IndexOf("c.", StringComparison.Ordinal) == -1 ? subCondition : string.Format(clauseTemplate, letter, property, operand, value);
				var join = $"{Constants.SQLJoinSymbol} {letter} {Constants.SQLInSymbol} {collection} ";
				joinClauses.Add(join + string.Join(string.Empty, subJoinClauses.ToArray()));
				finalTranslation = finalTranslation.Replace(match.Value, finalValue);
			}

			return (joinClauses.ToArray(), finalTranslation.StripCloseMissingOpenParenthesis());
		}

		public static string StripCloseMissingOpenParenthesis(this string value)
		{
			const char openParenthesis = '(';
			const char closeParenthesis = ')';
			if (string.IsNullOrWhiteSpace(value))
				return value;
			return value.IndexOf(openParenthesis) == -1 && value[value.Length - 1] == closeParenthesis ? value.Substring(0, value.Length - 1) : value;
		}
		
		public static string FindAndTranslateReservedKeywords(this string value)
		{
			var result = string.Copy(value);

			var reservedKeywordsList = new List<string>
			{
				"group"
			};

			foreach (var reservedKeyword in reservedKeywordsList)
			{
				var reservedKeywordRegex = "(\\.)(" + reservedKeyword + ")(\\.| )";
				var matches = Regex.Matches(result, reservedKeywordRegex, RegexOptions.IgnoreCase);
				foreach (Match match in matches)
				{
					var newValue = $"[\'{match.Groups[2].Value}\']{match.Groups[3].Value}";
					result = result.Replace(match.Value, newValue);
				}
			}

			return result;
		}
	}
}
