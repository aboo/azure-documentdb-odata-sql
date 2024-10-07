using System;
using Microsoft.OData.UriParser;
using Microsoft.Azure.Documents.OData.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

#if NET6_0
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.OData.ModelBuilder;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.OData.Query;
#else
using System.Net.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Routing;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace azure_documentdb_odata_sql_tests
{
	[TestClass]
	public class ODataToSqlSamples
	{
		/// <summary>
		/// 
		/// </summary>
		private static ODataQueryContext ODataQueryContext { get; set; }

		private static HttpRequest HttpRequest { get; set; }

		private static IServiceProvider Provider { get; set; }

		// Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void ClassInitialize(TestContext testContext)
		{
			var builder = new ODataConventionModelBuilder();
			var type = typeof(MockOpenType);
			var entityTypeConfiguration = builder.AddEntityType(type);
			entityTypeConfiguration.HasKey(type.GetProperty("Id"));
			builder.AddEntitySet(type.Name, entityTypeConfiguration);
			var conf = builder.EntitySet<MockOpenType>(type.Name);
			conf.EntityType.Property(p => p.Id).Name = "id";

			var edmModels = builder.GetEdmModel();
			ODataQueryContext = new ODataQueryContext(edmModels, type, new ODataPath());

			var collection = new ServiceCollection();

#if NET6_0
			collection
				.AddControllers()
				.AddOData();
#else
			collection.AddOData();
#endif

			collection.AddODataQueryFilter();
			collection.AddTransient<ODataUriResolver>();
			collection.AddTransient<ODataQueryValidator>();
			collection.AddTransient<TopQueryValidator>();
			collection.AddTransient<FilterQueryValidator>();
			collection.AddTransient<SkipQueryValidator>();
			collection.AddTransient<OrderByQueryValidator>();

#if NET6_0
			collection.AddTransient<ILoggerFactory, TestLoggingFactory>();
#endif

			Provider = collection.BuildServiceProvider();

			var applicationBuilder = Substitute.For<IApplicationBuilder>();
			applicationBuilder.ApplicationServices.Returns(Provider);

#if !NET6_0
			var routeBuilder = new RouteBuilder(applicationBuilder);
			routeBuilder.EnableDependencyInjection();
#endif
		}

		// Use TestInitialize to run code before running each test 
		[TestInitialize()]
		public void TestInitialize()
		{
			HttpRequest = new DefaultHttpRequest(new DefaultHttpContext
			{
				RequestServices = Provider
			})
			{
				Method = "GET",
				Host = new HostString("http://localhost")
			};
		}

		[TestMethod]
		public void TranslateSelectAllSample()
		{
			HttpRequest.QueryString = QueryString.Empty;
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.SELECT_CLAUSE);
			Assert.AreEqual("SELECT * FROM c ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSelectSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$select=englishName, id"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.SELECT_CLAUSE);
			Assert.AreEqual("SELECT c.englishName, c.id FROM c ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSelectWithEnumSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$select=enumNumber, id"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.SELECT_CLAUSE);
			Assert.AreEqual("SELECT c.enumNumber, c.id FROM c ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSelectAllTopSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=property ne 'str1'&$orderby=companyId DESC,id ASC&$top=15"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.SELECT_CLAUSE | TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT TOP 15 * FROM c ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSelectTopSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$select=p1, p2, p3&$filter=property ne 'str1'&$orderby=companyId DESC,id ASC&$top=15"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.SELECT_CLAUSE | TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT TOP 15 c.p1, c.p2, c.p3 FROM c ", sqlQuery);
		}

		[TestMethod]
		public void TranslateWhereSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost?$filter=englishName eq 'Microsoft' and intField le 5"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.WHERE_CLAUSE);
			Assert.AreEqual("WHERE c.englishName = 'Microsoft' AND c.intField <= 5 ", sqlQuery);
		}

		[TestMethod]
		public void TranslateWhereWithEnumSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost?$filter=enumNumber eq azure_documentdb_odata_sql_tests.MockEnum'ONE' and intField le 5"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.WHERE_CLAUSE);
			Assert.AreEqual("WHERE c.enumNumber = 'ONE' AND c.intField <= 5 ", sqlQuery);
		}

		[TestMethod]
		public void TranslateWhereWithNextedFieldsSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost?$filter=parent/child eq 'childValue' and intField le 5"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.WHERE_CLAUSE);
			Assert.AreEqual("WHERE c.parent.child = 'childValue' AND c.intField <= 5 ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAdditionalWhereSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost?$filter=englishName eq 'Microsoft' and intField le 5"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.WHERE_CLAUSE, "c.dataType = 'MockOpenType'");
			Assert.AreEqual("WHERE (c.dataType = 'MockOpenType') AND (c.englishName = 'Microsoft' AND c.intField <= 5) ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSelectWhereSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost?$filter=englishName eq 'Microsoft'"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.SELECT_CLAUSE | TranslateOptions.WHERE_CLAUSE, "c.dataType = 'MockOpenType'");
			Assert.AreEqual("SELECT * FROM c WHERE (c.dataType = 'MockOpenType') AND (c.englishName = 'Microsoft') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateOrderBySample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=property ne 'str1'&$orderby=companyId desc,id asc"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ORDERBY_CLAUSE);
			Assert.AreEqual("ORDER BY c.companyId DESC, c.id ASC ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSelectOrderBySample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=property ne 'str1'&$orderby=companyId desc,id asc"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE c.property != 'str1' ORDER BY c.companyId DESC, c.id ASC ", sqlQuery);
		}

		[TestMethod]
		public void TranslateContainsSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=contains(englishName, 'Microsoft')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE CONTAINS(c.englishName,'Microsoft') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateStartswithSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=startswith(englishName, 'Microsoft')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE STARTSWITH(c.englishName,'Microsoft') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateEndswithSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=endswith(englishName, 'Microsoft')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ENDSWITH(c.englishName,'Microsoft') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateUpperAndLowerSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=toupper(englishName) eq 'MICROSOFT' or tolower(englishName) eq 'microsoft'"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE UPPER(c.englishName) = 'MICROSOFT' OR LOWER(c.englishName) = 'microsoft' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateLengthSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=length(englishName) ge 10 and length(englishName) lt 15"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE LENGTH(c.englishName) >= 10 AND LENGTH(c.englishName) < 15 ", sqlQuery);
		}

		[TestMethod]
		public void TranslateIndexOfSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=indexof(englishName,'soft') eq 4"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE INDEX_OF(c.englishName,'soft') = 4 ", sqlQuery);
		}

		[TestMethod]
		public void TranslateSubstringSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=substring(englishName, 1, length(englishName)) eq 'icrosoft'"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE SUBSTRING(c.englishName,1,LENGTH(c.englishName)) = 'icrosoft' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateTrimSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=trim(englishName) eq 'Microsoft'"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE LTRIM(RTRIM(c.englishName)) = 'Microsoft' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateConcatSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=concat(englishName, ' Canada') eq 'Microsoft Canada'"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE CONCAT(c.englishName,' Canada') = 'Microsoft Canada' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateMasterSample()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/Post?$select=id, englishName&$filter=title eq 'title1' and property/field ne 'val' or viewedCount ge 5 and (likedCount ne 3 or enumNumber eq azure_documentdb_odata_sql_tests.MockEnum'TWO')&$orderby=_lastClientEditedDateTime asc, createdDateTime desc&$top=30"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL, "c._t = 'dataType'");
			Assert.AreEqual("SELECT TOP 30 c.id, c.englishName FROM c WHERE (c._t = 'dataType') AND (c.title = 'title1' AND c.property.field != 'val' OR c.viewedCount >= 5 AND (c.likedCount != 3 OR c.enumNumber = 'TWO')) ORDER BY c._lastClientEditedDateTime ASC, c.createdDateTime DESC ", sqlQuery);
		}

		[TestMethod]
		public void TranslateDateTime()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=startTime eq 2018-01-08T03:29:00Z"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE c.startTime = '2018-01-08T03:29:00Z' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateDateTime_WhenGreaterThan()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=startTime gt 2018-01-08T03:29:00Z"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE c.startTime > '2018-01-08T03:29:00Z' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateDateTime_WhenLowerThan()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=startTime lt 2018-01-08T03:29:00Z"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE c.startTime < '2018-01-08T03:29:00Z' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenAnyTargetingAListProperty()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=tags/any(t: t eq 'tag1')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.tags,'tag1') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenAnyTargetingAListPropertyWithVariableNamedAsX()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=tags/any(x: x eq 'tag1')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.tags,'tag1') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenAnyTargetingAListPropertyWithVariableHavingNoSpace()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=tags/any(x:x eq 'tag1')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.tags,'tag1') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenAnyTargetingAListPropertyWithVariableNameMoreThanPneLetter()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=tags/any(tag: tag eq 'tag1')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.tags,'tag1') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenAnyTargetingAListOfIntProperty()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=points/any(t: t eq 1)"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.points,1) ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenAnyTargetingAListOfEnumProperty()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=enumNumbers/any(t: t eq azure_documentdb_odata_sql_tests.MockEnum'ONE')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.enumNumbers,'ONE') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenThereAreTwoAnyInTheFilter()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=tags/any(t: t eq 'tag1') and tags/any(t: t eq 'tag2')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.tags,'tag1') AND ARRAY_CONTAINS(c.tags,'tag2') ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAny_WhenThereAreThreeAnyInTheFilter()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=tags/any(t: t eq 'tag1') and tags/any(t: t eq 'tag2') and tags/any(t: t eq 'tag3')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE ARRAY_CONTAINS(c.tags,'tag1') AND ARRAY_CONTAINS(c.tags,'tag2') AND ARRAY_CONTAINS(c.tags,'tag3') ", sqlQuery);
		}
		
		[TestMethod]
		public void TranslateAny_WhenAnyIsNotTheFirstConditionInTheFilter()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=parent/child eq 'childValue' and tags/any(t: t eq 'tag')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);
			
			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT * FROM c WHERE c.parent.child = 'childValue' AND ARRAY_CONTAINS(c.tags,'tag') ", sqlQuery);
		}

        [TestMethod]
		public void TranslateAnyToJoin_WhenQueriedBasedOnChildProperty()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=products/any(p: p/name eq 'test')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.products WHERE p.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenQueriedBasedOnChildPropertyNumber()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=products/any(p: p/price gt 10)"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.products WHERE p.price > 10 ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenQueriedBasedOnChildPropertyBoolean()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=products/any(p: p/shipped eq true)"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.products WHERE p.shipped = true ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenQueriedBasedOnMultipleChildProperty()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=products/any(p: p/name eq 'test') and locations/any(l: l/name eq 'test2')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.products JOIN l IN c.locations WHERE p.name = 'test' AND l.name = 'test2' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenChildHasFieldId()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=competitors/any(p: p/id eq '6a7ad0aa-678e-40f9-8cdf-03e3ab4a4106')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.competitors WHERE p.id = '6a7ad0aa-678e-40f9-8cdf-03e3ab4a4106' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenChildHasFieldIdBasedOnAnotherField()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=competitors/any(p: p/name eq 'test')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.competitors WHERE p.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_MixedLevelProperties()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=competitors/any(p: p/name eq 'test') and englishName eq 'test1'"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN p IN c.competitors WHERE p.name = 'test' AND c.englishName = 'test1' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenQueriedBasedOnChildPropertyDifferentLetter()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=products/any(j:j/name eq 'test')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.products WHERE j.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_Works_WhenCollectionIsInFirstChildElement()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=competitor/locations/any(j:j/name eq 'test')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.competitor.locations WHERE j.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_Works_WhenCollectionIsInSecondChildElement()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=competitor/competitor/locations/any(j:j/name eq 'test')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.competitor.competitor.locations WHERE j.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_Works_WhenCollectionIsInThirdChildElement()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=competitor/competitor/competitor/locations/any(j:j/name eq 'test')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.competitor.competitor.competitor.locations WHERE j.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_QueryBasedOnCorrectOrder_WhenCollectionIsInThirdChildElement()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=competitor/competitorTwo/competitor/locations/any(j:j/name eq 'test')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.competitor.competitorTwo.competitor.locations WHERE j.name = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_QueryBasedOnCorrectOrder_WhenConditionIsBasedOnId()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=competitor/competitorTwo/locations/any(j:j/id eq 'test')"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.competitor.competitorTwo.locations WHERE j.id = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenThereIsOneNestedjoin()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=competitor/locations/any(j:j/locations/any(l:l/id eq 'test'))"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var sqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual("SELECT VALUE c FROM c JOIN j IN c.competitor.locations JOIN l IN j.locations WHERE l.id = 'test' ", sqlQuery);
		}

		[TestMethod]
		public void TranslateAnyToJoin_WhenThereIsOneNestedjoinWithComplexPath()
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=payload/bet/legs/any(l: l/outcomes/any(o: o/id eq 'test'))"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

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
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri("http://localhost/User?$filter=payload/bet/legs/any(l:l/outcomes/any(o:o/competitor/id eq 'test'))"));

			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

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

		[TestMethod]
		public void Translate_ReturnsCorrectResult_WhenARangeVariableIsPresent()
		{
			// arrange 
			var oDataQueryOptions = GetODataQueryOptions("$filter=payload eq null ");

			// act 
			var sqlQuery = Translator.Translate(oDataQueryOptions, TranslateOptions.ALL);

			// assert 
			Assert.AreEqual("SELECT * FROM c WHERE c.payload = null ", sqlQuery);
		}

		[TestMethod]
		public void Translate_ReturnsCorrectResult_WhenARangeVariableIsPresentNotEgual()
		{
			// arrange 
			var oDataQueryOptions = GetODataQueryOptions("$filter=payload ne null ");

			// act 
			var sqlQuery = Translator.Translate(oDataQueryOptions, TranslateOptions.ALL);

			// assert 
			Assert.AreEqual("SELECT * FROM c WHERE c.payload != null ", sqlQuery);
		}

		[TestMethod]
		public void Translate_ReturnsCorrectResult_WhenANestedRangeVariableIsPresent()
		{
			// arrange 
			var oDataQueryOptions = GetODataQueryOptions("$filter=payload/payload eq null ");

			// act 
			var sqlQuery = Translator.Translate(oDataQueryOptions, TranslateOptions.ALL);

			// assert 
			Assert.AreEqual("SELECT * FROM c WHERE c.payload.payload = null ", sqlQuery);
		}

		[TestMethod]
		[DataRow("http://localhost/User?$filter=startswith(tolower(englishName), 'microsoft')", "SELECT * FROM c WHERE STARTSWITH(c.englishName,'microsoft', true) ")]
		[DataRow("http://localhost/User?$filter=contains(tolower(englishName), 'microsoft')", "SELECT * FROM c WHERE CONTAINS(c.englishName,'microsoft', true) ")]
		[DataRow("http://localhost/User?$filter=endswith(tolower(englishName), 'microsoft')", "SELECT * FROM c WHERE ENDSWITH(c.englishName,'microsoft', true) ")]
		[DataRow("http://localhost/User?$filter=startswith(toupper(englishName), 'microsoft')", "SELECT * FROM c WHERE STARTSWITH(c.englishName,'microsoft', true) ")]
		[DataRow("http://localhost/User?$filter=contains(toupper(englishName), 'microsoft')", "SELECT * FROM c WHERE CONTAINS(c.englishName,'microsoft', true) ")]
		[DataRow("http://localhost/User?$filter=endswith(toupper(englishName), 'microsoft')", "SELECT * FROM c WHERE ENDSWITH(c.englishName,'microsoft', true) ")]
		[DataRow("http://localhost/User?$filter=endswith(toupper(englishName), 'microsoft') and startswith(tolower(englishName),'random')", "SELECT * FROM c WHERE ENDSWITH(c.englishName,'microsoft', true) AND STARTSWITH(c.englishName,'random', true) ")]
		[DataRow("http://localhost/User?$filter=endswith(toupper(englishName), 'microsoft') or startswith(englishName,'randoM')", "SELECT * FROM c WHERE ENDSWITH(c.englishName,'microsoft', true) OR STARTSWITH(c.englishName,'randoM') ")]
		[DataRow("http://localhost/User?$filter=endswith(toupper(englishName), 'microsoft') and endswith(tolower(englishName),'random')", "SELECT * FROM c WHERE ENDSWITH(c.englishName,'microsoft', true) AND ENDSWITH(c.englishName,'random', true) ")]
		[DataRow("http://localhost/User?$filter=endswith(toupper(englishName), 'microsoft') and endswith(tolower(englishName),'random')", "SELECT * FROM c WHERE ENDSWITH(c.englishName,'microsoft', true) AND ENDSWITH(c.englishName,'random', true) ")]
		[DataRow("http://localhost/User?$filter=startswith(toupper(englishName), 'microsoft') or startswith(englishName,'randoM')", "SELECT * FROM c WHERE STARTSWITH(c.englishName,'microsoft', true) OR STARTSWITH(c.englishName,'randoM') ")]
		[DataRow("http://localhost/User?$filter=startswith(toupper(englishName), 'microsoft') or startswith(tolower(englishName),'randoM')", "SELECT * FROM c WHERE STARTSWITH(c.englishName,'microsoft', true) OR STARTSWITH(c.englishName,'randoM', true) ")]
		[DataRow("http://localhost/User?$filter=startswith(tolower(englishName), 'microsoft test')", "SELECT * FROM c WHERE STARTSWITH(c.englishName,'microsoft test', true) ")]
		[DataRow("http://localhost/User?$filter=startswith(toupper(englishName), 'microsoft ') or startswith(tolower(englishName),'randoM test')", "SELECT * FROM c WHERE STARTSWITH(c.englishName,'microsoft ', true) OR STARTSWITH(c.englishName,'randoM test', true) ")]
		[DataRow("http://localhost/User?$filter=endswith(toupper(englishName), '') and startswith(tolower(englishName),' ')", "SELECT * FROM c WHERE ENDSWITH(c.englishName,'', true) AND STARTSWITH(c.englishName,' ', true) ")]
		public void Translate_ReturnsCaseInsensitiveStringFunction_WhenODataQueryContainsCaseInsensitiveStringQuery(string uriString, string expectedSqlQuery)
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri(uriString));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var actualSqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual(expectedSqlQuery, actualSqlQuery);
		}

		[TestMethod]
		[DataRow("http://localhost/User?$filter=startswith(englishName, 'microsoft')", "SELECT * FROM c WHERE STARTSWITH(c.englishName,'microsoft') ")]
		[DataRow("http://localhost/User?$filter=contains(englishName, 'microsoft')", "SELECT * FROM c WHERE CONTAINS(c.englishName,'microsoft') ")]
		[DataRow("http://localhost/User?$filter=endswith(englishName, 'microsoft')", "SELECT * FROM c WHERE ENDSWITH(c.englishName,'microsoft') ")]
		public void Translate_ReturnsCaseSensitiveStringFunction_WhenODataQueryDoesNotContainCaseInsensitiveStringQuery(string uriString, string expectedSqlQuery)
		{
			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri(uriString));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);

			var oDataToSqlTranslator = new ODataToSqlTranslator(new SQLQueryFormatter());
			var actualSqlQuery = oDataToSqlTranslator.Translate(oDataQueryOptions, TranslateOptions.ALL & ~TranslateOptions.TOP_CLAUSE);
			Assert.AreEqual(expectedSqlQuery, actualSqlQuery);
		}

#region Helpers
		private static ODataQueryOptions GetODataQueryOptions(string oData)
		{
			const string baseUrl = "http://localhost/User?";

			HttpRequest.QueryString = QueryString.FromUriComponent(new Uri($"{baseUrl}{oData}"));
			var oDataQueryOptions = new ODataQueryOptions(ODataQueryContext, HttpRequest);
			return oDataQueryOptions;
		}

		private static ODataToSqlTranslator Translator => new ODataToSqlTranslator(new SQLQueryFormatter());
#endregion
	}

#if NET6_0
	public class TestLoggingFactory : Microsoft.Extensions.Logging.ILoggerFactory
	{
		private readonly ILogger logger = Substitute.For<ILogger>();
		public void Dispose()
		{
		}

		public void AddProvider(ILoggerProvider provider)
		{
		}

		public ILogger CreateLogger(string categoryName)
		{
			return logger;
		}
	}

#endif
}
