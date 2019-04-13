// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConvertPageEventDataToSearchPageEvent.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2016
// </copyright>
// <summary>
// Defines the ConvertPageEventDataToSearchPageEvent class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Support.Commerce.CustomModels.PageEvents.Converters
{
  using Analytics.Model;
  using Analytics.XConnect.DataAccess.Pipelines.ConvertToXConnectEventPipeline;
  using Newtonsoft.Json;
  using Sitecore.Commerce.AnalyticsData;
  using System.Collections.Generic;
  using System.Globalization;
  using XConnect;
  using Sitecore.Commerce.CustomModels;
  using Sitecore.Commerce.CustomModels.PageEvents;

  /// <summary>
  /// Converts page event data to the currency chosen page event POCO XConnect object.
  /// </summary>
  /// <seealso cref="Sitecore.Analytics.XConnect.DataAccess.Pipelines.ConvertToXConnectEventPipeline.ConvertPageEventDataToEventBase" />
  public class ConvertPageEventDataToSearchPageEvent : ConverterBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertPageEventDataToSearchPageEvent" /> class.
    /// </summary>
    public ConvertPageEventDataToSearchPageEvent() : base()
    {
    }

    /// <summary>
    /// Populates the given xConnect event with the content of the MongoDB json page event content.
    /// </summary>
    /// <param name="pageEventJson">The page event json.</param>
    /// <param name="event">The event.</param>
    public void PopulateEventFromJson(string pageEventJson, Event @event)
    {
      pageEventJson = this.CleanupMongoMigrationJson(pageEventJson);

      dynamic json = JsonConvert.DeserializeObject(pageEventJson);

      var pageEventData = new PageEventData();

      var data = AnalyticsDataInitializerFactory.Create<SearchAnalyticsData>();

      Dictionary<string, object> customValuesDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json.CustomValues.ToString());

      data.Deserialize(customValuesDict);

      this.PopulatePageEventFromBsonCustomValues(data, pageEventData);

      this.TranslateEvent(pageEventData, @event as SearchPageEvent);
    }

    /// <summary>
    /// Determines whether this instance can process page event data.
    /// </summary>
    /// <param name="pageEventData">The page event data.</param>
    /// <returns>
    ///   <c>true</c> if this instance can process page event data; otherwise, <c>false</c>.
    /// </returns>
    protected override bool CanProcessPageEventData(PageEventData pageEventData)
    {
      return pageEventData.PageEventDefinitionId == SearchPageEvent.ID;
    }

    /// <summary>
    /// Creates the event.
    /// </summary>
    /// <param name="pageEventData">The page event data.</param>
    /// <returns>The page event.</returns>
    protected override Event CreateEvent(PageEventData pageEventData)
    {
      var pageEvent = new SearchPageEvent(pageEventData.DateTime);

      this.TranslateEvent(pageEventData, pageEvent);

      return pageEvent;
    }

    /// <summary>
    /// Translates the given page event data into the xConnect custom model.
    /// </summary>
    /// <param name="pageEventData">The page event data.</param>
    /// <param name="event">The event.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
    protected void TranslateEvent(PageEventData pageEventData, SearchPageEvent @event)
    {
      if (pageEventData.CustomValues.ContainsKey("SearchTerm") && pageEventData.CustomValues.ContainsKey("ShopName") && pageEventData.CustomValues.ContainsKey("NumberOfHits"))
      {
        @event.Keywords = pageEventData.CustomValues["SearchTerm"] as string;
        @event.ShopName = pageEventData.CustomValues["ShopName"] as string;
        @event.SearchTerm = pageEventData.CustomValues["SearchTerm"] as string;
        @event.NumberOfHits = System.Convert.ToInt32(pageEventData.CustomValues["NumberOfHits"], CultureInfo.InvariantCulture);
      }
      else
      {
        @event.Keywords = pageEventData.Data;
      }
    }
  }
}
