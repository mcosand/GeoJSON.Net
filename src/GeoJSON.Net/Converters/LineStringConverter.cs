﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LineStringConverter.cs" company="Matthew Cosand">
//   Copyright © Matthew Cosand 2014
// </copyright>
// <summary>
//   Defines the LineStringConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace GeoJSON.Net.Converters
{
    using System;

    using GeoJSON.Net.Geometry;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Globalization;

    /// <summary>
    /// Converter to read and write the <see cref="LineString" /> type.
    /// </summary>
    public class LineStringConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var coordinateElements = value as System.Collections.Generic.List<IPosition>;
            if (coordinateElements != null && coordinateElements.Count > 0)
            {
              var coordinateArray = new JArray();
              foreach (var coordinate in coordinateElements.OfType<GeographicPosition>())
              {
                var coordinateElement = 
                  (coordinate.Altitude.HasValue && coordinate.Altitude != 0)
                    ? new JArray(coordinate.Longitude, coordinate.Latitude, coordinate.Altitude)
                    : new JArray(coordinate.Longitude, coordinate.Latitude);
                coordinateArray.Add(coordinateElement);
              }
              serializer.Serialize(writer, coordinateArray);
            }
            else
                serializer.Serialize(writer, value);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param><param name="objectType">Type of the object.</param><param name="existingValue">The existing value of object being read.</param><param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var inputJsonValue = serializer.Deserialize(reader).ToString();

            //sanitizing input
            inputJsonValue = inputJsonValue.Replace(Environment.NewLine, "");
            inputJsonValue = inputJsonValue.Replace(" ", "");

            var polygonCoordinates = new List<GeographicPosition>();

            //parsing coordinates groups
            MatchCollection coordinateGroups = Regex.Matches(inputJsonValue, @"(\[[-+]{0,1}\d{1,3}.\d+,[-+]{0,1}\d{1,2}.\d+[,\d+.\d+]*\])");
            foreach (Match coordinatePair in coordinateGroups) 
            {
                var coordinates = Regex.Match(coordinatePair.Value, @"(?<longitude>[+-]{0,1}\d+.\d+),(?<latitude>[+-]{0,1}\d+.\d+)(?:,)?(?<altitude>\d+.\d+)*");

                double lng;
                double lat;
                double alt;

                double.TryParse(coordinates.Groups["longitude"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out lng);
                double.TryParse(coordinates.Groups["latitude"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out lat);
                double.TryParse(coordinates.Groups["altitude"].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out alt);

                if (lng != 0 && lat != 0)
                    if (alt == 0)
                        polygonCoordinates.Add(new GeographicPosition(lat, lng));
                    else
                        polygonCoordinates.Add(new GeographicPosition(lat, lng, alt));
                
            }


            return new LineString(polygonCoordinates.ToList<IPosition>());
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LineString);
        }
    }
}
