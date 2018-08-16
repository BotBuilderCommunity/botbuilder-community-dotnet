using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Builder.Community.Recognizers.Fuzzy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bot.Builder.Community.Recognizers.Tests
{
    [TestClass]
    public class Fuzzy_RecognizerTests
    {
        [TestMethod]
        [TestCategory("Recognizers")]
        public async Task FuzzyRecognizer_TestScores_DefaultOptions()
        {
            var choices = new List<string>()
            {
                "Phil Coulson",
                "Peggy Carter",
                "Gary Pretty",
                "Peter Parker",
                "Tony Stark",
                "Bruce Banner",
                "Garry Pritti"
            };

            var fuzzyRecognizer = new FuzzyRecognizer();

            var result = await fuzzyRecognizer.Recognize(choices, "Gary Prety");

            Assert.IsNotNull(result, "Recognizer result should not be null");
            Assert.IsTrue(result.Matches.First().Choice == "Gary Pretty");
            Assert.AreEqual(result.Matches.Count, 2, "Incorrect number of matches");
        }

        [TestMethod]
        [TestCategory("Recognizers")]
        public async Task FuzzyRecognizer_TestIgnoreNonAlphanumeric_True()
        {
            var choices = new List<string>()
            {
                "Ti^na Hende*rson",
                "N&i$co^le Wa*ker",
            };

            var fuzzyRecognizer = new FuzzyRecognizer(new FuzzyRecognizerOptions()
            {
                IgnoreNonAlphanumeric = true
            });

            var result = await fuzzyRecognizer.Recognize(choices, "Nicole Waker");

            Assert.IsNotNull(result, "Recognizer result should not be null");
            Assert.IsTrue(result.Matches.First().Choice == "N&i$co^le Wa*ker");
            Assert.AreEqual(result.Matches.Count, 1, "Incorrect number of matches");
            Assert.IsTrue(result.Matches.First().Score > 0.9, "Incorrect score");
        }

        [TestMethod]
        [TestCategory("Recognizers")]
        public async Task FuzzyRecognizer_TestIgnoreNonAlphanumeric_False()
        {
            var choices = new List<string>()
            {
                "Ti^na Hende*rson",
                "N&i$co^le Wa*ker",
            };

            var fuzzyRecognizer = new FuzzyRecognizer(new FuzzyRecognizerOptions()
            {
                IgnoreNonAlphanumeric = false,

            });

            var result = await fuzzyRecognizer.Recognize(choices, "Nicole Waker");

            Assert.IsNotNull(result, "Recognizer result should not be null");
            Assert.AreEqual(result.Matches.First().Score, 0.75, "Incorrect number of matches");
        }

        [TestMethod]
        [TestCategory("Recognizers")]
        public async Task FuzzyRecognizer_TestIgnoreCase_True()
        {
            var choices = new List<string>()
            {
                "GARY PRETTY",
                "Gary Pretty"
            };

            var fuzzyRecognizer = new FuzzyRecognizer(new FuzzyRecognizerOptions()
            {
                IgnoreCase = true
            });

            var result = await fuzzyRecognizer.Recognize(choices, "Gary Pretty");

            Assert.IsNotNull(result, "Recognizer result should not be null");
            Assert.AreEqual(result.Matches.Count, 2, "Incorrect number of matches");
            Assert.AreEqual(result.Matches.Count(m => m.Score == 1), 2);
        }

        [TestMethod]
        [TestCategory("Recognizers")]
        public async Task FuzzyRecognizer_TestIgnoreCase_False()
        {
            var choices = new List<string>()
            {
                "GARY PRETTY",
                "Gary Pretty"
            };

            var fuzzyRecognizer = new FuzzyRecognizer(new FuzzyRecognizerOptions()
            {
                IgnoreCase = false
            });

            var result = await fuzzyRecognizer.Recognize(choices, "Gary Pretty");

            Assert.IsNotNull(result, "Recognizer result should not be null");
            Assert.AreEqual(result.Matches.Count, 1, "Incorrect number of matches");
            Assert.AreEqual(result.Matches.First().Choice, "Gary Pretty", "Incorrect number of matches");
            Assert.AreEqual(result.Matches.Count(m => m.Score == 1), 1);
        }

        [TestMethod]
        [TestCategory("Recognizers")]
        [ExpectedException(typeof(ArgumentNullException), "A null utterance did not throw an exception")]

        public async Task FuzzyRecognizer_Null_Utterance_Exception()
        {
            var choices = new List<string>()
            {
                "GARY PRETTY",
                "Gary Pretty"
            };

            var fuzzyRecognizer = new FuzzyRecognizer(new FuzzyRecognizerOptions()
            {
                IgnoreCase = false
            });

            var result = await fuzzyRecognizer.Recognize(choices, null);
        }

        [TestMethod]
        [TestCategory("Recognizers")]
        [ExpectedException(typeof(ArgumentNullException), "A null choice list did not throw an exception")]

        public async Task FuzzyRecognizer_Null_Choices_Exception()
        {
            var fuzzyRecognizer = new FuzzyRecognizer(new FuzzyRecognizerOptions()
            {
                IgnoreCase = false
            });

            var result = await fuzzyRecognizer.Recognize(null, "Gary Pretty");
        }

        [TestMethod]
        [TestCategory("Recognizers")]
        public async Task FuzzyRecognizer_Empty_Choices_List_Returns_Empty_Result()
        {
            var fuzzyRecognizer = new FuzzyRecognizer(new FuzzyRecognizerOptions()
            {
                IgnoreCase = false
            });

            var result = await fuzzyRecognizer.Recognize(new List<string>(), "Gary Pretty");

            Assert.IsTrue(result.Matches.Count == 0);
        }
    }
}
