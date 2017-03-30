using GodSpeak.Web.Models;
using GodSpeak.Web.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should;

namespace GodSpeak.Web.Tests.Util
{
    [TestClass]
    public class SeedTextFileParserTest
    {
        [TestMethod]
        public void TestParsePostalCodeGeoLocation()
        {
            var parser = new SeedTextFileParser();

            PostalCodeGeoLocation loc = parser.ParsePostalCodeGeoLocation("US	17748	Mc Elhattan	Pennsylvania	PA	Clinton	035			41.1355	-77.37	1");

            loc.CountryCode.ShouldEqual("US");
            loc.PostalCode.ShouldEqual("17748");
            loc.AdminName1.ShouldEqual("Mc Elhattan");
            loc.AdminCode1.ShouldEqual("Pennsylvania");
            loc.AdminName2.ShouldEqual("PA");
            loc.AdminCode2.ShouldEqual("Clinton");
            loc.AdminName3.ShouldEqual("035");
            loc.AdminCode3.ShouldBeEmpty();
            loc.Latitude.ShouldEqual(41.1355);
            loc.Longitude.ShouldEqual(-77.37);
        }


        [TestMethod]

        public void TestParseBibleVerse()
        {
            AssertLineParsed("Matthew 6:7 ``And when you are praying, do not use meaningless repetition as the Gentiles do, for they suppose that they will be heard for their many words."
                ,"Matthew", 6, 7, "``And when you are praying, do not use meaningless repetition as the Gentiles do, for they suppose that they will be heard for their many words.", "Matthew 6:7");
            AssertLineParsed("Genesis 1:3 Then God said, \"Let there be light\"; and there was light.", 
                "Genesis", 1, 3, "Then God said, \"Let there be light\"; and there was light.", "Genesis 1:3");
            AssertLineParsed("Exodus 7:1 Then the LORD said to Moses, \"See, I make you {as} God to Pharaoh, and your brother Aaron shall be your prophet.", "Exodus", 7, 1, "Then the LORD said to Moses, \"See, I make you {as} God to Pharaoh, and your brother Aaron shall be your prophet.", "Exodus 7:1");
            AssertLineParsed("Ecclesiastes 10:18 Through indolence the rafters sag, and through slackness the house leaks.", "Ecclesiastes", 10, 18, "Through indolence the rafters sag, and through slackness the house leaks.", "Ecclesiastes 10:18");
            AssertLineParsed("1 Samuel 1:1 Now there was a certain man from Ramathaim-zophim from the hill country of Ephraim, and his name was Elkanah the son of Jeroham, the son of Elihu, the son of Tohu, the son of Zuph, an Ephraimite.", "1 Samuel", 1, 1, "Now there was a certain man from Ramathaim-zophim from the hill country of Ephraim, and his name was Elkanah the son of Jeroham, the son of Elihu, the son of Tohu, the son of Zuph, an Ephraimite.", "1 Samuel 1:1");
        }

        private static void AssertLineParsed(string line, string expectedBook, int expectedChapter, int expectedVerse, string expectedText, string expectedShortCode)
        {
            var parser = new SeedTextFileParser();
            var verse = parser.ParseBibleVerse(line);

            verse.Book.ShouldEqual(expectedBook);

            verse.Chapter.ShouldEqual(expectedChapter);

            verse.Verse.ShouldEqual(expectedVerse);

            verse.Text.ShouldEqual(expectedText);

            verse.ShortCode.ShouldEqual(expectedShortCode);
        }
    }
}