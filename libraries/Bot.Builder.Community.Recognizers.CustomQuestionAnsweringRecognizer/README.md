# Custom Question Answering Recognizer 

## Summary
This recognizer helps you add a custom recognizer to a Bot Composer empty bot in order to use [Azure Question Answering](https://azure.microsoft.com/en-us/products/cognitive-services/question-answering/) (instead of using QnA Maker).

## Quickstart
1. Install the [package](https://www.nuget.org/packages/Bot.Builder.Community.Recognizers.CustomQuestionAnsweringRecognizer/) to your Composer project.
2. Change the root dialog's recognizer type to custom.
4. Update the settings in the custom recognizer with the hostname, project name and your keys.
3. Use custom intent triggers or the built in QnAIntent trigger as usual in the root dialog.

## Settings
If using the Question Answering recognizer, use the following definition

```
{
 "$kind": "Bot.Builder.Community.Recognizers.CustomQuestionAnsweringRecognizer",
  "hostname": "<your endpoint, including https://>",
  "projectName": "<your project name>",
  "endpointKey": "<your endpoint key>"
}
```

Because the Question Answering recognizer is a modified version of the existing QnAMaker recognizer, the workflow elements (such as multi turn) work the same. In addition the same QnAMaker events and telemetry are written out to the logs.

## Limitations

* Composer does not integrate natively with Question Answering, so managing the question/answer pairs must be done in the language studio portal instead of Composer.