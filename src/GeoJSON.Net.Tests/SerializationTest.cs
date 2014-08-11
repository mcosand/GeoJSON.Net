﻿using GeoJSON.Net.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

namespace GeoJSON.Net.Tests
{
    [TestClass]
    public class SerializationTest
    {
      [TestMethod]
      public void LineString_Coordinate_Serialization()
      {
        string expected = "\"coordinates\":[[102.0,0.0],[103.0,1.0],[104.0,0.0],[105.0,1.0]]";
        var feature = new GeoJSON.Net.Feature.Feature(
          new LineString(new List<IPosition> {
            new GeographicPosition(0.0, 102.0),
            new GeographicPosition(1.0, 103.0),
            new GeographicPosition(0.0, 104.0),
            new GeographicPosition(1.0, 105.0),
          }), new Dictionary<string, object>
          {
            { "name", "Dinagat Islands" }
          });

        var serialized = GetSerialized(feature, false);

        Assert.AreNotEqual(-1, serialized.IndexOf(expected), expected + "\n" + serialized);
      }

        /// <summary>
        /// Serializes the whole Polygon with properties
        /// </summary>
        [TestMethod]
        public void PointFeatureSerialization() 
        {
            var point = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.GeographicPosition(45.79012, 15.94107));
            var featureProperties = new Dictionary<string, object> { {"Name", "Foo"} };
            var model = new GeoJSON.Net.Feature.Feature(point, featureProperties);
            var serializedData = GetSerialized(model);

            Assert.IsFalse(serializedData.Contains("longitude"));
        }

        private static string GetSerialized(object model, bool indent = true)
        {
          var serializedData = JsonConvert.SerializeObject(model, indent ? Formatting.Indented : Formatting.None, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore });
          return serializedData;
        }

        [TestMethod]
        public void PolygonFeatureSerialization() 
        {
            var coordinates = new List<GeographicPosition> 
                { 
                    new GeographicPosition(52.370725881211314, 4.889259338378906), 
                    new GeographicPosition(52.3711451105601, 4.895267486572266), 
                    new GeographicPosition(52.36931095278263, 4.892091751098633), 
                    new GeographicPosition(52.370725881211314, 4.889259338378906) 
                }.ToList<IPosition>();

            var polygon = new Polygon(new List<LineString> { new LineString(coordinates) });
            var featureProperties = new Dictionary<string, object> { { "Name", "Foo" } };
            var model = new GeoJSON.Net.Feature.Feature(polygon, featureProperties);

            var serializedData = JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore });

        }


        [TestMethod]
        public void PolygonSerialization()
        {
            var coordinates = new List<GeographicPosition> 
                { 
                    new GeographicPosition(52.370725881211314, 4.889259338378906), 
                    new GeographicPosition(52.3711451105601, 4.895267486572266), 
                    new GeographicPosition(52.36931095278263, 4.892091751098633), 
                    new GeographicPosition(52.370725881211314, 4.889259338378906) 
                }.ToList<IPosition>();

            var model = new Polygon(new List<LineString> { new LineString(coordinates) });
            var serializedData = JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore });

            var matches = Regex.Matches(serializedData, @"(?<coordinates>[0-9]+([.,][0-9]+))");

            double lng;
            double.TryParse(matches[0].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out lng);

            //Double precision can pose a problem 
            Assert.IsTrue(Math.Abs(lng - 4.889259338378906) < 0.0000001);

            Assert.IsTrue(!serializedData.Contains("latitude"));
        }

        [TestMethod]
        public void GeographicPositionSerialization()
        {
            var model = new GeoJSON.Net.Geometry.GeographicPosition(112.12, 10);

            var serialized = JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            var matches = Regex.Matches(serialized, @"(\d+.\d+)");
            Assert.IsTrue(matches.Count == 2);
            double lng= 0;
            double.TryParse(matches[0].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out lng);

            Assert.AreEqual(lng, 112.12);
        }


       
    }
}
