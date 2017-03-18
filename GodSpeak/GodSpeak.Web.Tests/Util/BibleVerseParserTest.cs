using GodSpeak.Web.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should;

namespace GodSpeak.Web.Tests.Util
{
    [TestClass]
    public class BibleVerseParserTest
    {
        [TestMethod]

        public void TestParseLine()
        {
            AssertLineParsed("Genesis 1:3 Then God said, \"Let there be light\"; and there was light.", "Genesis", 1, 3, "Then God said, \"Let there be light\"; and there was light.");
            AssertLineParsed("Exodus 7:1 Then the LORD said to Moses, \"See, I make you {as} God to Pharaoh, and your brother Aaron shall be your prophet.", "Exodus", 7, 1, "Then the LORD said to Moses, \"See, I make you {as} God to Pharaoh, and your brother Aaron shall be your prophet.");
            AssertLineParsed("Ecclesiastes 10:18 Through indolence the rafters sag, and through slackness the house leaks.", "Ecclesiastes", 10, 18, "Through indolence the rafters sag, and through slackness the house leaks.");
        }

        private static void AssertLineParsed(string line, string expectedBook, int expectedChapter,
            int expectedVerse, string expectedText)
        {
            var parser = new BibleVerseParser();
            var verse = parser.ParseLine(line);

            verse.Book.ShouldEqual(expectedBook);

            verse.Chapter.ShouldEqual(expectedChapter);

            verse.Verse.ShouldEqual(expectedVerse);

            verse.Text.ShouldEqual(expectedText);
        }
    }
}