## Fuzzy Matching Recognizer

This is part of the [Bot Builder Community Extensions](https://github.com/garypretty/botbuilder-community) project which contains various pieces of middleware, recognizers and other components for use with the Bot Builder .NET SDK v4.

The Fuzzy Recognizer allows you to compare a specified string against a list of 1 or more other strings.  The result is a list of string that are close enough to match with the comparison string (above a given threshold which you can set).  This can be useful when taking input from the user where spelling mistakes may be common, e.g. names or people, companies or other entities etc.

The Fuzzy Recognizer is available via [NuGet](https://www.nuget.org/packages/Bot.Builder.Community.Recognizers.FuzzyRecognizer/).

Install it into your project using the following command in the package manager;
```
    PM> Install-Package Bot.Builder.Community.Recognizers.FuzzyRecognizer
```

To use the recognizer, you can simply pass in a list of possible choices (the list of strings you wish to check for matches) and the string to be matched against.  In the example below we are checking for any matches in the list against "Gary Prety".

```cs
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
```

The above code will return a result object with 2 matches, in descending order by match score;

* Gary Pretty - score: 0.90
* Garry Pretti - score: 0.66

You can also, optionally, pass in a FuzzyRecognizerOptions object when creating your instance of the recognizer to alter some of the default behaviour.

```cs
    var fuzzyRecognizer = new FuzzyRecognizer(new FuzzyRecognizerOptions()
        {
            // The threshold for which matches above this level will be 
            // returned in the results object. Defaults to 0.6.
            Threshold = 0.6
            // Should non-alphanumeric characters be taken into account when 
            // matching? Defaults to true.
            IgnoreNonAlphanumeric = true,
            // Should case be ignored when matching. Defaults to true and all
            // matching is done by converting all input to lowercase.
            IgnoreCase = true
        });
```
