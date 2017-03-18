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
            var parser = new BibleVerseParser();
            var verse = parser.ParseLine("Genesis 1:3 Then God said, \"Let there be light\"; and there was light.");
            verse.Book.ShouldEqual("Genesis");
            verse.Chapter.ShouldEqual(1);
            verse.Verse.ShouldEqual(3);
            verse.Text.ShouldEqual("Then God said, \"Let there be light\"; and there was light.");
        }
    }
}