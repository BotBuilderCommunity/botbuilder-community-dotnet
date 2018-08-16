using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bot.Builder.Community.Recognizers.Fuzzy
{
    public class FuzzyRecognizer
    {
        private readonly FuzzyRecognizerOptions _fuzzyRecognizerOptions;

        public FuzzyRecognizer(FuzzyRecognizerOptions fuzzyRecognizerOptions = null)
        {
            _fuzzyRecognizerOptions = fuzzyRecognizerOptions ?? new FuzzyRecognizerOptions();
        }

        public async Task<FuzzyRecognizerResult> Recognize(IEnumerable<string> choices, string utterance)
        {
            if (string.IsNullOrEmpty(utterance))
                throw new ArgumentNullException(nameof(utterance));

            if (choices == null)
                throw new ArgumentNullException(nameof(choices));

            return FindAllMatches(choices, utterance, _fuzzyRecognizerOptions);
        }

        private static FuzzyRecognizerResult FindAllMatches(IEnumerable<string> choices, string utterance, FuzzyRecognizerOptions options)
        {
            var result = new FuzzyRecognizerResult()
            {
                Matches = new List<FuzzyMatch>()
            };

            var choicesList = choices as IList<string> ?? choices.ToList();

            if (!choicesList.Any())
                return result;

            string utteranceToCheck = utterance;

            if (options.IgnoreCase)
            {
                utteranceToCheck = utteranceToCheck.ToLower();
            }

            if (options.IgnoreNonAlphanumeric)
            {
                utteranceToCheck = Regex.Replace(utteranceToCheck, "[^a-zA-Z0-9_. ]+", "", RegexOptions.Compiled).Trim();
            }

            foreach (var choice in choicesList)
            {
                double score = 0;

                var choiceValue = choice.Trim();

                if (options.IgnoreNonAlphanumeric)
                    choiceValue = Regex.Replace(choiceValue, "[^a-zA-Z0-9_. ]+", "", RegexOptions.Compiled);

                if (options.IgnoreCase)
                    choiceValue = choiceValue.ToLower();

                var editDistance = EditDistance(choiceValue, utteranceToCheck);
                var maxLength = (double)Math.Max(utteranceToCheck.Length, choiceValue.Length);
                var matchResult = maxLength - editDistance;
                score = matchResult / maxLength; 

                if (score >= options.Threshold)
                {
                    result.Matches.Add(new FuzzyMatch { Choice = choice, Score = score });
                }
            }

            result.Matches = result.Matches.OrderByDescending(m => m.Score).ToList();

            return result;
        }

        /// <summary>
        /// Code for calculating distance from Stephen Toub 
        /// https://blogs.msdn.microsoft.com/toub/2006/05/05/generic-levenshtein-edit-distance-with-c/
        /// </summary>
        public static int EditDistance<T>(IEnumerable<T> x, IEnumerable<T> y) where T : IEquatable<T>
        {
            // Validate parameters
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");

            // Convert the parameters into IList instances
            // in order to obtain indexing capabilities
            IList<T> first = x as IList<T> ?? new List<T>(x);
            IList<T> second = y as IList<T> ?? new List<T>(y);
            // Get the length of both.  If either is 0, return
            // the length of the other, since that number of insertions
            // would be required.
            int n = first.Count, m = second.Count;
            if (n == 0) return m;
            if (m == 0) return n;
            // Rather than maintain an entire matrix (which would require O(n*m) space),
            // just store the current row and the next row, each of which has a length m+1,
            // so just O(m) space. Initialize the current row.
            int curRow = 0, nextRow = 1;
            int[][] rows = new int[][] { new int[m + 1], new int[m + 1] };
            for (int j = 0; j <= m; ++j) rows[curRow][j] = j;
            // For each virtual row (since we only have physical storage for two)
            for (int i = 1; i <= n; ++i)
            {
                // Fill in the values in the row
                rows[nextRow][0] = i;
                for (int j = 1; j <= m; ++j)
                {
                    int dist1 = rows[curRow][j] + 1;
                    int dist2 = rows[nextRow][j - 1] + 1;
                    int dist3 = rows[curRow][j - 1] +
                        (first[i - 1].Equals(second[j - 1]) ? 0 : 1);
                    rows[nextRow][j] = Math.Min(dist1, Math.Min(dist2, dist3));
                }
                // Swap the current and next rows
                if (curRow == 0)
                {
                    curRow = 1;
                    nextRow = 0;
                }
                else
                {
                    curRow = 0;
                    nextRow = 1;
                }
            }
            // Return the computed edit distance
            return rows[curRow][m];
        }
    }

    public class FuzzyRecognizerResult
    {
        public FuzzyRecognizerResult()
        {
            Matches = new List<FuzzyMatch>();
        }

        public List<FuzzyMatch> Matches { get; set; }
    }

    public class FuzzyRecognizerOptions
    {
        public FuzzyRecognizerOptions(double threshold = 0.6, bool ignoreCase = true, bool ignoreNonAlphanumeric = true)
        {
            Threshold = threshold;
            IgnoreCase = ignoreCase;
            IgnoreNonAlphanumeric = ignoreNonAlphanumeric;
        }

        public bool IgnoreNonAlphanumeric { get; set; }

        public bool IgnoreCase { get; set; }

        public double Threshold { get; set; }
    }

    public class FuzzyMatch
    {
        public string Choice { get; set; }
        public double Score { get; set; }
    }
}
