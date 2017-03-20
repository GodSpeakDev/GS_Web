using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using GodSpeak.Web.Models;
using GodSpeak.Web.Util;

namespace GodSpeak.Web.Repositories
{
    public interface IInMemoryDataRepository
    {
        Dictionary<string, BibleVerse> VerseCache { get; }
        Dictionary<string, PostalCodeGeoLocation> PostalCodeGeoCache { get; }
    }

    public class InMemoryDataRepository : IInMemoryDataRepository
    {
        private readonly Dictionary<string,BibleVerse> _verseCache = new Dictionary<string, BibleVerse>();

        private readonly Dictionary<string, PostalCodeGeoLocation> _postalCodeGeoCache = new Dictionary<string, PostalCodeGeoLocation>();


        public Dictionary<string, BibleVerse> VerseCache
        {
            get
            {
                if(!_verseCache.Any())
                    LoadBibleVerses();
                return _verseCache;
            }
        }

        public Dictionary<string, PostalCodeGeoLocation> PostalCodeGeoCache
        {
            get
            {
                if(!_postalCodeGeoCache.Any())
                    LoadPostalCodeGeoLocations();
                return _postalCodeGeoCache;   
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
            var parser = new SeedTextFileParser();
            

            foreach (var line in File.ReadLines(AppDataPath + "NASBNAME.TXT"))
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                try
                {
                    var verse = parser.ParseBibleVerse(line);
                    if (!_verseCache.ContainsKey(verse.ShortCode))
                        _verseCache[verse.ShortCode] = verse;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error debugging line:\r" + line);
                }
            }
        }

        public void LoadPostalCodeGeoLocations()
        {
            var parser = new SeedTextFileParser();


            foreach (var line in File.ReadLines(AppDataPath + "allCountries.txt"))
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
                catch (Exception ex)
                {
                    throw new Exception("Error debugging line:\r" + line);
                }
            }
        }

    }
}