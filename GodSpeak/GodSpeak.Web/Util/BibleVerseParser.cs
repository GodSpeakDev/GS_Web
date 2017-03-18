using System.Linq;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Util
{
    public class BibleVerseParser
    {
        public BibleVerse ParseLine(string line)
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

        
    }
}