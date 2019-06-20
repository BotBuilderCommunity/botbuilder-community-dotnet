namespace Luis_Dialog_Sample
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Bot.Builder.Community.Dialogs.Luis;
    using Bot.Builder.Community.Dialogs.Luis.Models;
    using Luis_Dialog_Sample.Models;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// This sample was ported from V3
    /// </summary>
    /// <remarks>
    /// LuisModelAttribute can be added to the class, and LuisDialog will
    /// construct the LuisService dynamically.  Or, the developer can pass
    /// a LuisService to the base class constructor directly.
    /// </remarks>
    /// [LuisModel("7d8ea658-f01a-49f2-a239-2d7ef805dde9", "1cf447f840ee414e87c7b93bb6d5cc63", domain: "westus.api.cognitive.microsoft.com")]
    public class SimpleNoteDialog : LuisDialog<object>
    {
        // Default note title
        private const string DefaultNoteTitle = "default";

        // Name of note title entity
        private const string EntityNoteTitle = "Note.Title";

        // Store notes in a dictionary that uses the title as a key
        private static readonly Dictionary<string, Note> NoteByTitle = new Dictionary<string, Note>();

        // The current note being created by the user.
        private IStatePropertyAccessor<Note> currentNote;

        public SimpleNoteDialog(ConversationState conversationState, IConfiguration configuration)
            : base(nameof(SimpleNoteDialog), new[] { GetLuisService(configuration) })
        {
            this.AddDialog(new PromptWaterfallDialog());

            this.currentNote = conversationState.CreateProperty<Note>("CurrentNote");
        }

        /// <summary>
        /// Create a LuisService using configuration settings.
        /// </summary>
        /// <param name="configuration">IConfiguration used to retrieve app settings for
        /// creating the LuisModelAttribute.</param>
        /// <returns>A LuisService constructed from configuration settings.</returns>
        static ILuisService GetLuisService(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var luisModel = new LuisModelAttribute(configuration["LuisModelId"], configuration["LuisSubscriptionKey"], domain: configuration["LuisHostDomain"]);
            return new LuisService(luisModel);
        }

        /// <summary>
        /// OnContinueDialogAsync is executed on every turn of a dialog after the first turn,
        /// and when a child dialog finishes executing.
        /// </summary>
        /// <param name="innerDc">Current DialogContext</param>
        /// <param name="cancellationToken">Thread CancellationToken</param>
        /// <returns>A DialogTurnResult</returns>
        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var childResult = await innerDc.ContinueDialogAsync(cancellationToken);
            var result = childResult.Result as DialogTurnResult;

            if (result != null)
            {
                var promptResult = result.Result as PromptDialogOptions;
                if (promptResult != null)
                {
                    switch (promptResult.ReturnMethod)
                    {
                        case PromptReturnMethods.After_DeleteTitlePrompt:
                            return await this.After_DeleteTitlePrompt(innerDc, promptResult.Result);
                        case PromptReturnMethods.After_TextPrompt:
                            return await this.After_TextPrompt(innerDc, promptResult.Result);
                        case PromptReturnMethods.After_TitlePrompt:
                            return await this.After_TitlePrompt(innerDc, promptResult.Result);
                        default:
                            throw new InvalidOperationException("Invalid PromptReturnMethods");
                    }
                }
                else
                {
                    return await base.OnContinueDialogAsync(innerDc, cancellationToken);
                }
            }
            else
            {
                return await base.OnContinueDialogAsync(innerDc, cancellationToken);
            }
        }

        /// <summary>
        /// This method overload inspects the result from LUIS to see if a title entity was detected, and finds the note with that title, or the note with the default title, if no title entity was found.
        /// </summary>
        /// <param name="result">The result from LUIS that contains intents and entities that LUIS recognized.</param>
        /// <param name="note">This parameter returns any note that is found in the list of notes that has a matching title.</param>
        /// <returns>true if a note was found, otherwise false</returns>
        private bool TryFindNote(LuisResult result, out Note note)
        {
            note = null;

            string titleToFind;

            EntityRecommendation title;
            if (result.TryFindEntity(EntityNoteTitle, out title))
            {
                titleToFind = title.Entity;
            }
            else
            {
                titleToFind = DefaultNoteTitle;
            }

            return NoteByTitle.TryGetValue(titleToFind, out note); // TryGetValue returns false if no match is found.
        }

        /// <summary>
        /// This method overload takes a string and finds the note with that title.
        /// </summary>
        /// <param name="noteTitle">A string containing the title of the note to search for.</param>
        /// <param name="note">This parameter returns any note that is found in the list of notes that has a matching title.</param>
        /// <returns>true if a note was found, otherwise false</returns>
        private bool TryFindNote(string noteTitle, out Note note)
        {
            bool foundNote = NoteByTitle.TryGetValue(noteTitle, out note); // TryGetValue returns false if no match is found.
            return foundNote;
        }

        /// <summary>
        /// Send a generic help message if an intent without an intent handler is detected.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">The result from LUIS.</param>
        /// <returns>A DialogTurnResult</returns>
        [LuisIntent("")]
        private async Task<DialogTurnResult> None(DialogContext context, LuisResult result)
        {
            string message = $"I'm the Notes bot. I can understand requests to create, delete, and read notes. \n\n Detected intent: " + string.Join(", ", result.Intents.Select(i => i.Intent));
            await context.PostAsync(message);

            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        /// <summary>
        /// Handle the Note.Delete intent. If a title isn't detected in the LUIS result, prompt the user for a title.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">The result from LUIS.</param>
        /// <returns>A DialogTurnResult</returns>
        [LuisIntent("Note.Delete")]
        private async Task<DialogTurnResult> DeleteNote(DialogContext context, LuisResult result)
        {
            Note note;
            if (this.TryFindNote(result, out note))
            {
                NoteByTitle.Remove(note.Title);
                await context.PostAsync($"Note {note.Title} deleted");

                return new DialogTurnResult(DialogTurnStatus.Complete);
            }
            else
            {
                // Prompt the user for a note title
                var options = new PromptDialogOptions()
                {
                    Prompt = "What is the title of the note you want to delete?",
                    ReturnMethod = PromptReturnMethods.After_DeleteTitlePrompt
                };
                return await context.BeginDialogAsync(nameof(PromptWaterfallDialog), options);
            }
        }

        /// <summary>
        /// Handles the Note.ReadAloud intent by displaying a note or notes.
        /// If a title of an existing note is found in the LuisResult, that note is displayed.
        /// If no title is detected in the LuisResult, all of the notes are displayed.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">LUIS result.</param>
        /// <returns>A DialogTurnResult</returns>
        [LuisIntent("Note.ReadAloud")]
        private async Task<DialogTurnResult> FindNote(DialogContext context, LuisResult result)
        {
            Note note;
            if (this.TryFindNote(result, out note))
            {
                await context.PostAsync($"**{note.Title}**: {note.Text}.");
            }
            else
            {
                // Print out all the notes if no specific note name was detected
                string noteList = "Here's the list of all notes: \n\n";
                foreach (KeyValuePair<string, Note> entry in NoteByTitle)
                {
                    Note noteInList = entry.Value;
                    noteList += $"**{noteInList.Title}**: {noteInList.Text}.\n\n";
                }

                await context.PostAsync(noteList);
            }

            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        /// <summary>
        /// Handles the Note.Create intent. Prompts the user for the note title if the title isn't detected in the LuisResult.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">LUIS result.</param>
        /// <returns>A DialogTurnResult</returns>
        [LuisIntent("Note.Create")]
        private async Task<DialogTurnResult> CreateNote(DialogContext context, LuisResult result)
        {
            EntityRecommendation title;
            if (!result.TryFindEntity(EntityNoteTitle, out title))
            {
                // Prompt the user for a note title
                var options = new PromptDialogOptions()
                {
                    Prompt = "What is the title of the note you want to create?",
                    ReturnMethod = PromptReturnMethods.After_TitlePrompt
                };
                return await context.BeginDialogAsync(nameof(PromptWaterfallDialog), options);
            }
            else
            {
                var note = await this.currentNote.GetAsync(context.Context, () => new Note());
                note.Title = title.Entity;
                NoteByTitle[note.Title] = note;

                // Prompt the user for what they want to say in the note
                var options = new PromptDialogOptions()
                {
                    Prompt = "What do you want to say in your note?",
                    ReturnMethod = PromptReturnMethods.After_TextPrompt
                };
                return await context.BeginDialogAsync(nameof(PromptWaterfallDialog), options);
            }
        }

        /// <summary>
        /// Executed after asking the user for the Title of the Note they wish to create.
        /// </summary>
        /// <param name="context">Current dialog context</param>
        /// <param name="noteTitle">Title of the note to create.</param>
        /// <returns>A DialogTurnResult</returns>
        private async Task<DialogTurnResult> After_TitlePrompt(DialogContext context, string noteTitle)
        {
            // Set the title (used for creation, deletion, and reading)
            if (string.IsNullOrEmpty(noteTitle))
            {
                noteTitle = DefaultNoteTitle;
            }

            // Add the new note to the list of notes and also save it in order to add text to it later
            var note = await this.currentNote.GetAsync(context.Context, () => new Note());
            note.Title = noteTitle;
            NoteByTitle[note.Title] = note;

            // Prompt the user for what they want to say in the note
            var options = new PromptDialogOptions()
            {
                Prompt = "What do you want to say in your note?",
                ReturnMethod = PromptReturnMethods.After_TextPrompt
            };
            return await context.BeginDialogAsync(nameof(PromptWaterfallDialog), options);
        }

        /// <summary>
        /// Executed after asking the user for the Text of the Note they wish to create.
        /// </summary>
        /// <param name="context">Current dialog context</param>
        /// <param name="noteText">The Text of the note the user wants to create.</param>
        /// <returns>A DiaogTurnResult</returns>
        private async Task<DialogTurnResult> After_TextPrompt(DialogContext context, string noteText)
        {
            // Set the text of the note
            var note = await this.currentNote.GetAsync(context.Context, () => new Note());
            note.Text = noteText;
            NoteByTitle[note.Title] = note;

            await context.PostAsync($"Created note **{note.Title}** that says \"{note.Text}\".");

            await this.currentNote.DeleteAsync(context.Context);
            return new DialogTurnResult(DialogTurnStatus.Complete);
        }

        /// <summary>
        /// Executed after asking the user for the Title of the Note they wish to delete.
        /// </summary>
        /// <param name="context">Current dialog context</param>
        /// <param name="titleToDelete">The title of the note the user wants to delete</param>
        /// <returns>A DialogTurnResult</returns>
        private async Task<DialogTurnResult> After_DeleteTitlePrompt(DialogContext context, string titleToDelete)
        {
            Note note;
            bool foundNote = NoteByTitle.TryGetValue(titleToDelete, out note);

            if (foundNote)
            {
                NoteByTitle.Remove(note.Title);
                await context.PostAsync($"Note {note.Title} deleted");
            }
            else
            {
                await context.PostAsync($"Did not find note named {titleToDelete}.");
            }

            return new DialogTurnResult(DialogTurnStatus.Complete);
        }
    }
}
