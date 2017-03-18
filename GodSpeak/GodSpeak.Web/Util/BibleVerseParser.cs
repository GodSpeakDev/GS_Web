using System.Linq;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Util
{
    public class BibleVerseParser
    {
        public BibleVerse ParseLine(string line)
        {
            var parts = line.Split(' ');
            var locationParts = parts[1].Split(':');
            return new BibleVerse()
            {
                Book = parts[0],
                Chapter = int.Parse(locationParts[0]),
                Verse = int.Parse(locationParts[1]),
                Text = string.Join(" ", parts.ToList().GetRange(2, parts.Length - 2))
            };
        }

        
    }
}