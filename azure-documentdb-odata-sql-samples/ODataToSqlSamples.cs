using System;
using System.Net.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using System.Web.OData.Routing;

using Microsoft.Azure.Documents.OData.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace azure_documentdb_odata_sql_tests
{
	[TestClass]
	public class ODataToSqlSamples
	{
		/// <summary>
		/// 
		/// </summary>
		private static ODataQueryContext ODataQueryContext { get; set; }

		/// <summary>
		/// 
		/// </summary>
		private static HttpRequestMessage HttpRequestMessage { get; set; }

		// Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void ClassInitialize(TestContext testContext)
		{
			var builder = new ODataConventionModelBuilder();
			var type = typeof(MockOpenType);
			var entityTypeConfiguration = builder.AddEntityType(type);
			entityTypeConfiguration.HasKey(type.GetProperty("Id"));
			builder.AddEntitySet(type.Name, entityTypeConfiguration);
			var edmModels = builder.GetEdmModel();
			ODataQueryContext = new ODataQueryContext(edmModels, type, new ODataPath());
		}

		// Use TestInitialize to run code before running each test 
		[TestInitialize()]
		public void TestInitialize()
		{
			HttpRequestMessage = new HttpRequestMessage
			{
				Method = HttpMethod.Get
			};
			var config = new System.Web.Http.HttpConfiguration();
			config.EnableDependencyInjection();
			HttpRequestMessage.SetConfiguration(config);
		}

		[TestMethod]
		public void TranslateSelectAllSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.SELECT_CLAUSE);
			Assert.AreEqual("SELECT * FROM c ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSelectSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$select=englishName, id");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.SELECT_CLAUSE);
			Assert.AreEqual("SELECT c.englishName, c.id FROM c ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSelectWithEnumSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$select=enumNumber, id");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.SELECT_CLAUSE);
			Assert.AreEqual("SELECT c.enumNumber, c.id FROM c ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSelectAllTopSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=property ne 'str1'&$orderby=companyId DESC,id ASC&$top=15");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.SELECT_CLAUSE | TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT TOP 15 * FROM c ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSelectTopSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$select=p1, p2, p3&$filter=property ne 'str1'&$orderby=companyId DESC,id ASC&$top=15");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.SELECT_CLAUSE | TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT TOP 15 c.p1, c.p2, c.p3 FROM c ", sqlQuery);
		}

		[TestMethod]
		public void TranslateWhereSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost?$filter=englishName eq 'Microsoft' and intField le 5");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.WHERE_CLAUSE);
			Assert.AreEqual("WHERE c.englishName = 'Microsoft' AND c.intField <= 5 ", sqlQuery);
		}

		[TestMethod]
		public void TranslateWhereWithEnumSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost?$filter=enumNumber eq azure_documentdb_odata_sql_tests.MockEnum'ONE' and intField le 5");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.WHERE_CLAUSE);
			Assert.AreEqual("WHERE c.enumNumber = 'ONE' AND c.intField <= 5 ", sqlQuery);
		}

		[TestMethod]
		public void TranslateWhereWithNextedFieldsSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost?$filter=parent/child eq 'childValue' and intField le 5");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.WHERE_CLAUSE);
			Assert.AreEqual("WHERE c.parent.child = 'childValue' AND c.intField <= 5 ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAdditionalWhereSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost?$filter=englishName eq 'Microsoft' and intField le 5");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.WHERE_CLAUSE, "c.dataType = 'MockOpenType'");
			Assert.AreEqual("WHERE (c.dataType = 'MockOpenType') AND (c.englishName = 'Microsoft' AND c.intField <= 5) ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSelectWhereSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost?$filter=englishName eq 'Microsoft'");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.SELECT_CLAUSE | TranslateOptions.WHERE_CLAUSE, "c.dataType = 'MockOpenType'");
			Assert.AreEqual("SELECT * FROM c WHERE (c.dataType = 'MockOpenType') AND (c.englishName = 'Microsoft') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateOrderBySample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=property ne 'str1'&$orderby=companyId desc,id asc");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ORDERBY_CLAUSE);
			Assert.AreEqual("ORDER BY c.companyId DESC, c.id ASC ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSelectOrderBySample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=property ne 'str1'&$orderby=companyId desc,id asc");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE c.property != 'str1' ORDER BY c.companyId DESC, c.id ASC ", sqlQuery);
		}

		[TestMethod]
		public void TranslateContainsSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=contains(englishName, 'Microsoft')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE CONTAINS(c.englishName,'Microsoft') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateStartswithSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=startswith(englishName, 'Microsoft')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE STARTSWITH(c.englishName,'Microsoft') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateEndswithSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=endswith(englishName, 'Microsoft')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ENDSWITH(c.englishName,'Microsoft') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateUpperAndLowerSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=toupper(englishName) eq 'MICROSOFT' or tolower(englishName) eq 'microsoft'");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE UPPER(c.englishName) = 'MICROSOFT' OR LOWER(c.englishName) = 'microsoft' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateLengthSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=length(englishName) ge 10 and length(englishName) lt 15");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE LENGTH(c.englishName) >= 10 AND LENGTH(c.englishName) < 15 ", sqlQuery);
		}

		[TestMethod]
		public void TranslateIndexOfSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=indexof(englishName,'soft') eq 4");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE INDEX_OF(c.englishName,'soft') = 4 ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSubstringSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=substring(englishName, 1, length(englishName)) eq 'icrosoft'");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE SUBSTRING(c.englishName,1,LENGTH(c.englishName)) = 'icrosoft' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateTrimSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=trim(englishName) eq 'Microsoft'");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE LTRIM(RTRIM(c.englishName)) = 'Microsoft' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateConcatSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=concat(englishName, ' Canada') eq 'Microsoft Canada'");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE CONCAT(c.englishName,' Canada') = 'Microsoft Canada' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateMasterSample()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/Post?$select=id, englishName&$filter=title eq 'title1' and property/field ne 'val' or viewedCount ge 5 and (likedCount ne 3 or enumNumber eq azure_documentdb_odata_sql_tests.MockEnum'TWO')&$orderby=_lastClientEditedDateTime asc, createdDateTime desc&$top=30");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL, "c._t = 'dataType'");
			Assert.AreEqual("SELECT TOP 30 c.id, c.englishName FROM c WHERE (c._t = 'dataType') AND (c.title = 'title1' AND c.property.field != 'val' OR c.viewedCount >= 5 AND (c.likedCount != 3 OR c.enumNumber = 'TWO')) ORDER BY c._lastClientEditedDateTime ASC, c.createdDateTime DESC ", sqlQuery);
		}

		[TestMethod]
		public void TranslateDateTime()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=startTime eq 2018-01-08T03:29:00Z");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE c.startTime = '2018-01-08T03:29:00Z' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateDateTime_WhenGreaterThan()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=startTime gt 2018-01-08T03:29:00Z");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE c.startTime > '2018-01-08T03:29:00Z' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateDateTime_WhenLowerThan()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=startTime lt 2018-01-08T03:29:00Z");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE c.startTime < '2018-01-08T03:29:00Z' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenAnyTargetingAListProperty()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=tags/any(t: t eq 'tag1')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.tags,'tag1') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenAnyTargetingAListPropertyWithVariableNamedAsX()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=tags/any(x: x eq 'tag1')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.tags,'tag1') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenAnyTargetingAListPropertyWithVariableHavingNoSpace()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=tags/any(x:x eq 'tag1')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.tags,'tag1') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenAnyTargetingAListPropertyWithVariableNameMoreThanPneLetter()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=tags/any(tag: tag eq 'tag1')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.tags,'tag1') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenAnyTargetingAListOfIntProperty()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=points/any(t: t eq 1)");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.points,1) ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenAnyTargetingAListOfEnumProperty()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=enumNumbers/any(t: t eq azure_documentdb_odata_sql_tests.MockEnum'ONE')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.enumNumbers,'ONE') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenThereAreTwoAnyInTheFilter()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=tags/any(t: t eq 'tag1') and tags/any(t: t eq 'tag2')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.tags,'tag1') AND ARRAY_CONTAINS(c.tags,'tag2') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenThereAreThreeAnyInTheFilter()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=tags/any(t: t eq 'tag1') and tags/any(t: t eq 'tag2') and tags/any(t: t eq 'tag3')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.tags,'tag1') AND ARRAY_CONTAINS(c.tags,'tag2') AND ARRAY_CONTAINS(c.tags,'tag3') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenQueriedBasedOnChildProperty()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=products/any(p: p/name eq 'test')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.products WHERE p.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenQueriedBasedOnChildPropertyNumber()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=products/any(p: p/price gt 10)");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.products WHERE p.price > 10 ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenQueriedBasedOnChildPropertyBoolean()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=products/any(p: p/shipped eq true)");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.products WHERE p.shipped = true ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenQueriedBasedOnMultipleChildProperty()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=products/any(p: p/name eq 'test') and locations/any(l: l/name eq 'test2')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.products JOIN l IN c.locations WHERE p.name = 'test' AND l.name = 'test2' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenChildHasFieldId()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=competitors/any(p: p/id eq '6a7ad0aa-678e-40f9-8cdf-03e3ab4a4106')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.competitors WHERE p.id = '6a7ad0aa-678e-40f9-8cdf-03e3ab4a4106' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenChildHasFieldIdBasedOnAnotherField()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=competitors/any(p: p/name eq 'test')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.competitors WHERE p.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_MixedLevelProperties()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=competitors/any(p: p/name eq 'test') and englishName eq 'test1'");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.competitors WHERE p.name = 'test' AND c.englishName = 'test1' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenQueriedBasedOnChildPropertyDifferentLetter()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=products/any(j:j/name eq 'test')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.products WHERE j.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_Works_WhenCollectionIsInFirstChildElement()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=competitor/locations/any(j:j/name eq 'test')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.competitor.locations WHERE j.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_Works_WhenCollectionIsInSecondChildElement()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=competitor/competitor/locations/any(j:j/name eq 'test')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.competitor.competitor.locations WHERE j.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_Works_WhenCollectionIsInThirdChildElement()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=competitor/competitor/competitor/locations/any(j:j/name eq 'test')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.competitor.competitor.competitor.locations WHERE j.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_QueryBasedOnCorrectOrder_WhenCollectionIsInThirdChildElement()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=competitor/competitorTwo/competitor/locations/any(j:j/name eq 'test')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.competitor.competitorTwo.competitor.locations WHERE j.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_QueryBasedOnCorrectOrder_WhenConditionIsBasedOnId()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=competitor/competitorTwo/locations/any(j:j/id eq 'test')");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.competitor.competitorTwo.locations WHERE j.id = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenThereIsOneNestedjoin()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=competitor/locations/any(j:j/locations/any(l:l/id eq 'test'))");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.competitor.locations JOIN l IN j.locations WHERE l.id = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenThereIsOneNestedjoinWithComplexPath()
		{
			HttpRequestMessage.RequestUri = new Uri("http://localhost/User?$filter=payload/bet/legs/any(l: l/outcomes/any(o: o/id eq 'test'))");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN l IN c.payload.bet.legs JOIN o IN l.outcomes WHERE o.id = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateEnum_WhenClassDoestHaveId()
		{
			// arrange
			var oDataQueryOptions = GetODataQueryOptions("$filter=payload/bet/status eq azure_documentdb_odata_sql_tests.BetStatus'Accepted'");

			// act
			var sqlQuery = Translator.Translate(oDataQueryOptions, TranslateOptions.ALL);

			// assert
			Assert.AreEqual("SELECT * FROM c WHERE c.payload.bet.status = 'Accepted' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateReservedKeyword_WhenClassHasReservedLowercaseGroupAsPropertyName()
		{
			// arrange
			var oDataQueryOptions = GetODataQueryOptions("$filter=group/id eq 'groupId'");

			// act
			var sqlQuery = Translator.Translate(oDataQueryOptions, TranslateOptions.ALL);

			// assert
			Assert.AreEqual("SELECT * FROM c WHERE c['group'].id = 'groupId' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateReservedKeyword_WhenClassHasSecondLevelReservedLowercaseGroupAsPropertyName()
		{
			// arrange
			var oDataQueryOptions = GetODataQueryOptions("$filter=product/group/id eq 'groupId'");

			// act
			var sqlQuery = Translator.Translate(oDataQueryOptions, TranslateOptions.ALL);

			// assert
			Assert.AreEqual("SELECT * FROM c WHERE c.product['group'].id = 'groupId' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateReservedKeyword_WhenClassHasReservedLowercaseGroupAsPropertyNameInJoin()
		{
			// arrange
			var oDataQueryOptions = GetODataQueryOptions("$filter=competitors/any(d: d/group/id eq 'groupId')");

			// act
			var sqlQuery = Translator.Translate(oDataQueryOptions, TranslateOptions.ALL);

			// assert
			Assert.AreEqual("SELECT VALUE c FROM c JOIN d IN c.competitors WHERE d['group'].id = 'groupId' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateReservedKeyword_WhenClassHasReservedUppercaseGroupAsPropertyName()
		{
			// arrange
			var oDataQueryOptions = GetODataQueryOptions("$filter=Group/id eq 'groupId'");

			// act
			var sqlQuery = Translator.Translate(oDataQueryOptions, TranslateOptions.ALL);

			// assert
			Assert.AreEqual("SELECT * FROM c WHERE c['Group'].id = 'groupId' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateReservedKeyword_WhenClassHasReservedMixedcaseGroupAsPropertyName()
		{
			// arrange
			var oDataQueryOptions = GetODataQueryOptions("$filter=GrOup/id eq 'groupId'");

			// act
			var sqlQuery = Translator.Translate(oDataQueryOptions, TranslateOptions.ALL);

			// assert
			Assert.AreEqual("SELECT * FROM c WHERE c['GrOup'].id = 'groupId' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateReservedKeyword_WhenClassHasReservedGroupAsPropertyName()
		{
			// arrange
			var oDataQueryOptions = GetODataQueryOptions("$filter=GrOup eq 'groupId'");

			// act
			var sqlQuery = Translator.Translate(oDataQueryOptions, TranslateOptions.ALL);

			// assert
			Assert.AreEqual("SELECT * FROM c WHERE c['GrOup'] = 'groupId' ", sqlQuery);
		}

		[TestMethod]
		public void Translate_ReturnsExpectedQuery_WhenAComplexNodeHasFirstLevelConstantCondition()
		{
			// arrange
			var oDataQueryOptions = GetODataQueryOptions("$filter=bonus/balance gt 0");

			// act
			var sqlQuery = Translator.Translate(oDataQueryOptions, TranslateOptions.ALL);

			// assert
			Assert.AreEqual("SELECT * FROM c WHERE c.bonus.balance > 0 ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenThereIsOneNestedjoinAndConditionBasedOnChildProperty()
		{
			HttpRequestMessage.RequestUri =
				new Uri("http://localhost/User?$filter=payload/bet/legs/any(l:l/outcomes/any(o:o/competitor/id eq 'test'))");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery =
				oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual(
				"SELECT VALUE c FROM c JOIN l IN c.payload.bet.legs JOIN o IN l.outcomes WHERE o.competitor.id = 'test' ",
				sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_ReturnsCorrectResult_WhenQueryIsBasedOnANestedProperty()
		{
			// arrange
			var oDataQueryOptions = GetODataQueryOptions("$filter=sportSummaries/any(x: x/single/totalAmount gt 0)");

			// act
			var sqlQuery = Translator.Translate(oDataQueryOptions, TranslateOptions.ALL);

			// assert
			Assert.AreEqual("SELECT VALUE c FROM c JOIN x IN c.sportSummaries WHERE x.single.totalAmount > 0 ", sqlQuery);
		}

		[TestMethod]
		public void Translate_ReturnsCorrectResult_WhenQueryHasAdditionalWhereClauses()
		{
			// arrange
			var oDataQueryOptions = GetODataQueryOptions("$filter=sportSummaries/any(x: x/single/totalAmount gt 0)");
			var additionalWhereClause = "1=1";

			// act
			var sqlQuery = Translator.Translate(oDataQueryOptions, TranslateOptions.ALL, additionalWhereClause);

			// assert
			Assert.AreEqual("SELECT VALUE c FROM c JOIN x IN c.sportSummaries WHERE (1=1) AND (x.single.totalAmount > 0) ", sqlQuery);
		}

		#region Helpers
		private static ODataQueryOptions GetODataQueryOptions(string oData)
		{
			const string baseUrl = "http://localhost/User?";

			HttpRequestMessage.RequestUri = new Uri($"{baseUrl}{oData}");
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequestMessage);
			return oDataQueryOptions;
		}

		private static ODataToSqlTranslator Translator => new ODataToSqlTranslator(new SQLQueryFormatter());
		#endregion
	}
}
