using System.Collections.Generic;
using System.Runtime.Serialization;

using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace azure_documentdb_odata_sql_tests
{
	[DataContract]
	public enum MockEnum
	{
		[EnumMember]
		ZERO,

		[EnumMember]
		ONE,

		[EnumMember]
		TWO
	}


	[DataContract]
	public class MockOpenType
	{
		/// <summary>
		/// The property name for EnglishName
		/// </summary>
		public const string EnglishNamePropertyName = "englishName";

		/// <summary>
		/// The property name for EnumNumber
		/// </summary>
		public const string EnumNumberPropertyName = "enumNumber";

		public const string TagsPropertyName = "tags";
		public const string PointsPropertyName = "points";
		public const string EnumNumbersPropertyName = "enumNumbers";
		public const string ProductsPropertyName = "products";
		public const string LocationsPropertyName = "locations";
		public const string CompetitorsPropertyName = "competitors";
		public const string CompetitorPropertyName = "competitor";
		public const string PayloadPropertyName = "payload";
		public const string BalancePropertyName = "balance";
		public const string BonusPropertyName = "bonus";
		public const string SportSummariesPropertyName = "sportSummaries";
		public const string IdPropertyName = "id";

		/// <summary>
		/// Id
		/// </summary>
		[DataMember(Name = IdPropertyName)]
		[JsonProperty(PropertyName = IdPropertyName)]
		public string Id { get; set; }

		/// <summary>
		/// EnglishName
		/// </summary>
		[DataMember(Name = EnglishNamePropertyName)]
		[JsonProperty(PropertyName = EnglishNamePropertyName)]
		public string EnglishName { get; set; }

		/// <summary>
		/// EnumNumber
		/// </summary>
		[DataMember(Name = EnumNumberPropertyName)]
		[JsonProperty(PropertyName = EnumNumberPropertyName)]
		public MockEnum EnumNumber { get; set; }

		/// <summary>
		/// PropertyBag to make Edm open-typed
		/// </summary>
		public Dictionary<string, object> PropertyBag { get; set; }

		[DataMember(Name = TagsPropertyName)]
		[JsonProperty(PropertyName = TagsPropertyName)]
		public List<string> Tags { get; set; }

		[DataMember(Name = PointsPropertyName)]
		[JsonProperty(PropertyName = PointsPropertyName)]
		public List<int> Points { get; set; }

		[DataMember(Name = EnumNumbersPropertyName)]
		[JsonProperty(PropertyName = EnumNumbersPropertyName)]
		public List<MockEnum> EnumNumbers { get; set; }

		[DataMember(Name = ProductsPropertyName)]
		[JsonProperty(PropertyName = ProductsPropertyName)]
		public List<Product> Products { get; set; }

		[DataMember(Name = CompetitorsPropertyName)]
		[JsonProperty(PropertyName = CompetitorsPropertyName)]
		public List<Competitor> Competitors{ get; set; }

		[DataMember(Name = LocationsPropertyName)]
		[JsonProperty(PropertyName = LocationsPropertyName)]
		public List<Location> Locations { get; set; }

		[DataMember(Name = CompetitorPropertyName)]
		[JsonProperty(PropertyName = CompetitorPropertyName)]
		public Competitor Competitor { get; set; }

		[DataMember(Name = PayloadPropertyName)]
		[JsonProperty(PropertyName = PayloadPropertyName)]
		public Payload Payload { get; set; }

		[DataMember(Name = BonusPropertyName)]
		[JsonProperty(PropertyName = BonusPropertyName)]
		public Bonus Bonus { get; set; }

		[DataMember(Name = SportSummariesPropertyName)]
		[JsonProperty(PropertyName = SportSummariesPropertyName)]
		public List<SportSummary> SportSummaries { get; set; }
	}

	[DataContract]
	public class Bonus
	{
		public const string BalancePropertyName = "balance";

		[DataMember(Name = BalancePropertyName)]
		[JsonProperty(PropertyName = BalancePropertyName)]
		public decimal Balance { get; set; }
	}

	[DataContract]
	public class Competitor
	{
		public const string IdPropertyName = "id";
		public const string NamePropertyName = "name";
		public const string LocationsPropertyName = "locations";
		public const string CompetitorPropertyName = "competitor";
		public const string CompetitorTwoPropertyName = "competitorTwo";
		public const string GroupPropertyName = "group";

		[DataMember(Name = IdPropertyName)]
		[JsonProperty(PropertyName = IdPropertyName)]
		public string Id { get; set; }


		[DataMember(Name = NamePropertyName)]
		[JsonProperty(PropertyName = NamePropertyName)]
		public string Name { get; set; }

		[DataMember(Name = LocationsPropertyName)]
		[JsonProperty(PropertyName = LocationsPropertyName)]
		public List<Location> Locations { get; set; }

		[DataMember(Name = CompetitorPropertyName)]
		[JsonProperty(PropertyName = CompetitorPropertyName)]
		public Competitor Competitor1 { get; set; }

		[DataMember(Name = CompetitorTwoPropertyName)]
		[JsonProperty(PropertyName = CompetitorTwoPropertyName)]
		public Competitor Competitor2 { get; set; }

		[DataMember(Name = GroupPropertyName)]
		[JsonProperty(PropertyName = GroupPropertyName)]
		public Group Group { get; set; }
	}

	[DataContract]
	public class Product
	{
		public const string NamePropertyName = "name";
		public const string PricePropertyName = "price";
		public const string ShippedPropertyName = "shipped";

		[DataMember(Name = NamePropertyName)]
		[JsonProperty(PropertyName = NamePropertyName)]
		public string Name { get; set; }

		[DataMember(Name = PricePropertyName)]
		[JsonProperty(PropertyName = PricePropertyName)]
		public int Price { get; set; }


		[DataMember(Name = ShippedPropertyName)]
		[JsonProperty(PropertyName = ShippedPropertyName)]
		public bool Shipped { get; set; }
	}

	[DataContract]
	public class Location
	{
		public const string NamePropertyName = "name";
		public const string IdPropertyName = "id";
		public const string LocationsPropertyName = "locations";

		[DataMember(Name = IdPropertyName)]
		[JsonProperty(PropertyName = IdPropertyName)]
		public string Id { get; set; }

		[DataMember(Name = NamePropertyName)]
		[JsonProperty(PropertyName = NamePropertyName)]
		public string Name { get; set; }

		[DataMember(Name = LocationsPropertyName)]
		[JsonProperty(PropertyName = LocationsPropertyName)]
		public List<Location> Locations { get; set; }
	}

	[DataContract]
	public class Payload
	{
		public const string BetPropertyName = "bet";
		public const string PayloadPropertyName = "payload";

		[DataMember(Name = BetPropertyName)]
		[JsonProperty(PropertyName = BetPropertyName)]
		public Bet Bet { get; set; }

		[DataMember(Name = PayloadPropertyName)]
		[JsonProperty(PropertyName = PayloadPropertyName)]
		public Payload2 Payload2 { get; set; }
	}

	[DataContract]
	public class Payload2
	{
		public const string BetPropertyName = "bet";

		[DataMember(Name = BetPropertyName)]
		[JsonProperty(PropertyName = BetPropertyName)]
		public Bet Bet { get; set; }
	}

	[DataContract]
	public class Bet
	{
		public const string StatusPropertyName = "status";
		public const string LegsPropertyName = "legs";

		[DataMember(Name = StatusPropertyName)]
		[JsonProperty(PropertyName = StatusPropertyName)]
		public BetStatus Status { get; set; }

		[DataMember(Name = LegsPropertyName)]
		[JsonProperty(PropertyName = LegsPropertyName)]
		public List<Leg> Legs { get; set; }
	}

	[DataContract]
	public enum BetStatus
	{
		[EnumMember]
		Accepted,

		[EnumMember]
		Processing
	}

	[DataContract]
	public class Leg
	{
		public const string IdPropertyName = "id";
		public const string EventPropertyName = "event";
		public const string OutcomesPropertyName = "outcomes";

		[DataMember(Name = IdPropertyName)]
		[JsonProperty(PropertyName = IdPropertyName)]
		public string Id { get; set; }

		[DataMember(Name = EventPropertyName)]
		[JsonProperty(PropertyName = EventPropertyName)]
		public Event Event { get; set; }

		[DataMember(Name = OutcomesPropertyName)]
		[JsonProperty(PropertyName = OutcomesPropertyName)]
		public List<Outcome> Outcomes { get; set; }
	}

	[DataContract]
	public class Outcome
	{
		public const string IdPropertyName = "id";
		public const string CompetitorPropertyName = "competitor";

		[DataMember(Name = IdPropertyName)]
		[JsonProperty(PropertyName = IdPropertyName)]
		public string Id { get; set; }

		[DataMember(Name = CompetitorPropertyName)]
		[JsonProperty(PropertyName = CompetitorPropertyName)]
		public Competitor Competitor { get; set; }
	}

	[DataContract]
	public class Event
	{
		public const string IdPropertyName = "id";

		[DataMember(Name = IdPropertyName)]
		[JsonProperty(PropertyName = IdPropertyName)]
		public string Id { get; set; }
	}

	[DataContract]
	public class Group
	{
		public const string IdPropertyName = "id";

		[DataMember(Name = IdPropertyName)]
		[JsonProperty(PropertyName = IdPropertyName)]
		public string Id { get; set; }
	}

	[DataContract]
	public class SportSummary
	{
		public const string SinglePropertyName = "single";

		[DataMember(Name = SinglePropertyName)]
		[JsonProperty(PropertyName = SinglePropertyName)]
		public SingleSummary Single { get; set; }
	}

	[DataContract]
	public class SingleSummary
	{
		public const string TotalAmountPropertyName = "totalAmount";

		[DataMember(Name = TotalAmountPropertyName)]
		[JsonProperty(PropertyName = TotalAmountPropertyName)]
		public int TotalAmount { get; set; }
	}
}
