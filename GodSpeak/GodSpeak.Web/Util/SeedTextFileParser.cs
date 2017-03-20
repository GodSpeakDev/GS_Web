using System;
using System.Linq;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Util
{
    public class SeedTextFileParser
    {
        public BibleVerse ParseBibleVerse(string line)
        {
            var parts = line.Split(' ').ToList();
            var locationParts = parts.First(s => s.Contains(":")).Split(':');
            var locationIndex = parts.FindIndex(s => s.Contains(":"));
            return new BibleVerse()
            {
                Book = string.Join(" ", parts.GetRange(0, locationIndex)),
                Chapter = int.Parse(locationParts[0]),
                Verse = int.Parse(locationParts[1]),
                Text = string.Join(" ", parts.GetRange(locationIndex + 1, parts.Count - locationIndex - 1)),
                ShortCode = string.Join(" ", parts.GetRange(0, locationIndex + 1))
            };
        }


        public PostalCodeGeoLocation ParsePostalCodeGeoLocation(string empty)
        {
            var parts = empty.Split('\t');
            double lat = 0;
            double longitude = 0;
            try
            {
                lat = double.Parse(parts[9].Trim());
            }
            catch
            {
                throw new Exception($"Error trying to parse latitude {parts[9]}");
            }
            try
            {
                longitude = double.Parse(parts[10].Trim());
            }
            catch
            {
                throw new Exception($"Error trying to parse longitude {parts[9]}");
            }
            return new PostalCodeGeoLocation()
            {
                CountryCode = parts[0],
                PostalCode = parts[1],
                AdminName1 = parts[2],
                AdminCode1 = parts[3],
                AdminName2 = parts[4],
                AdminCode2 = parts[5],
                AdminName3 = parts[6],
                AdminCode3 = parts[7],
                Latitude = lat,
                Longitude = longitude
                
            };
        }
    }
}