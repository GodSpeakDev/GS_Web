using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography.X509Certificates;
using System.Web.Helpers;
using GodSpeak.Web.Models;
using GodSpeak.Web.Util;
using Microsoft.Owin.Security.Provider;
using Newtonsoft.Json;

namespace GodSpeak.Web.Repositories
{
    public interface IInMemoryDataRepository
    {
        Dictionary<string, BibleVerse> VerseCache { get; }
        Dictionary<string, PostalCodeGeoLocation> PostalCodeGeoCache { get; }

        Dictionary<string, NoPostalCodeGeoLocation> NoPostalCodeGeoCache { get; }

        Dictionary<string, string> CountryCodeNameMap { get; }
    }

    public class InMemoryDataRepository : IInMemoryDataRepository
    {
//        private static readonly Dictionary<string,BibleVerse> _verseCache = new Dictionary<string, BibleVerse>();

//        private static readonly Dictionary<string, PostalCodeGeoLocation> _postalCodeGeoCache = new Dictionary<string, PostalCodeGeoLocation>();

        private const string VerseCacheKey = "verse_cache";

        private const string ZipGeoCacheKey = "zip_geo_cache";

        private const string NoPostalGeoCacheKey = "no_postal_code_geo_cache";

        public Dictionary<string, BibleVerse> VerseCache
        {
            get
            {
                ObjectCache cache = MemoryCache.Default;
                
                if (!cache.Contains(VerseCacheKey))
                    LoadBibleVerses();
                return cache[VerseCacheKey] as Dictionary<string, BibleVerse>;
            }
        }

        public Dictionary<string, PostalCodeGeoLocation> PostalCodeGeoCache
        {
            get
            {
                ObjectCache cache = MemoryCache.Default;
                if (!cache.Contains(ZipGeoCacheKey))
                    LoadPostalCodeGeoLocations();
                return cache[ZipGeoCacheKey] as Dictionary<string, PostalCodeGeoLocation>;   
            }
        }

        public Dictionary<string, NoPostalCodeGeoLocation> NoPostalCodeGeoCache
        {
            get
            {
                ObjectCache cache = MemoryCache.Default;
                if (!cache.Contains(NoPostalGeoCacheKey))
                    LoadNoPostalCodeGeoLocations();
                return cache[NoPostalGeoCacheKey] as Dictionary<string, NoPostalCodeGeoLocation>;
            }
        }
        private readonly Dictionary<string, string> _countryCodeNameMap = new Dictionary<string, string>();
        public Dictionary<string, string> CountryCodeNameMap
        {
            get
            {
                if (!_countryCodeNameMap.Any())
                    LoadCountryCodeNames();
                return _countryCodeNameMap;
                
            }
        }

       

        protected string AppDataPath
        {
            get
            {
                var binFolderPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.AppDomain.CurrentDomain.RelativeSearchPath ?? "");
                binFolderPath += "App_Data/";
                if(Directory.Exists(binFolderPath))
                    return binFolderPath;

                string path = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/");
                return path;
            }
        }

        public void LoadBibleVerses()
        {
            ObjectCache cache = MemoryCache.Default;
            var parser = new SeedTextFileParser();
            var verseCache = new Dictionary<string, BibleVerse>();

            var fileLines = cache.Contains("verses_line") ? (IEnumerable<string>)cache["verses_line"] : File.ReadLines(AppDataPath + "NASBNAME.TXT");

            cache.Set("verses_line", fileLines, new CacheItemPolicy()
            {
                Priority = CacheItemPriority.NotRemovable
            });

            foreach (var line in fileLines)
            {
                if (string.IsNullOrEmpty(line) || line.Trim() == "")
                    continue;

                try
                {
                    var verse = parser.ParseBibleVerse(line);
                    if (!verseCache.ContainsKey(verse.ShortCode))
                        verseCache[verse.ShortCode] = verse;
                }
                catch(Exception ex)
                {
                    throw new Exception("Error debugging line:\r" + line);
                }
            }
            cache[VerseCacheKey] = verseCache;
        }

        private void LoadCountryCodeNames()
        {
            foreach (var line in File.ReadLines(AppDataPath + "country-codes.csv"))
            {
                var parts = line.Split(',');
                try
                {
                    _countryCodeNameMap[parts[3]] = parts[1];
                }
                catch
                {
                    throw new Exception($"Error parsing code/name line: {line}");
                }
            }
        }

        public void LoadNoPostalCodeGeoLocations()
        {
            ObjectCache cache = MemoryCache.Default;
            var _noPostalCodeGeoCache = new Dictionary<string, NoPostalCodeGeoLocation>();

            var policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.NotRemovable;

            var jsonText = File.ReadAllText(AppDataPath + "no_postal_code_countries.json");

            var entries = JsonConvert.DeserializeObject<List<NoPostalCodeGeoLocation>>(jsonText);
            foreach (var loc in entries)
            {
                var key = CountryCodeNameMap.Keys.FirstOrDefault(k => CountryCodeNameMap[k] == loc.Country);
                if (key != null)
                {
                    _noPostalCodeGeoCache[key] = loc;
                }
                else
                {
                    Debug.WriteLine("Can't find:" + loc.Country);
                }
            }

            cache.Set(NoPostalGeoCacheKey, _noPostalCodeGeoCache, policy);
        }

        public void LoadPostalCodeGeoLocations()
        {
            var parser = new SeedTextFileParser();
            ObjectCache cache = MemoryCache.Default;
            var _postalCodeGeoCache = new Dictionary<string, PostalCodeGeoLocation>();



            var fileLines = cache.Contains("zip_lines")?(IEnumerable<string>)cache["zip_lines"] : File.ReadLines(AppDataPath + "allCountries.txt");

            cache.Set("zip_lines", fileLines, new CacheItemPolicy()
            {
                Priority = CacheItemPriority.NotRemovable
            });

            foreach (var line in fileLines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                try
                {
                    var location = parser.ParsePostalCodeGeoLocation(line);
                    var key = $"{location.CountryCode}-{location.PostalCode}";
                    if (!_postalCodeGeoCache.ContainsKey(key))
                        _postalCodeGeoCache[key] = location;
                }
                catch 
                {
                    throw new Exception("Error parsing line:\r" + line);

                }
            }

            
            var policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.NotRemovable;
            

            cache.Set(ZipGeoCacheKey, _postalCodeGeoCache, policy);
        }

    }
}