using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Builder.Community.Dialogs.FormFlow;
using Bot.Builder.Community.Dialogs.FormFlow.Advanced;
#pragma warning disable 649

namespace FormFlow_Sample.Dialogs
{
    //https://docs.microsoft.com/en-us/azure/bot-service/dotnet/bot-builder-dotnet-formflow-formbuilder?view=azure-bot-service-3.0

    [Serializable]
    public class BuilderSandwich : SandwichOrder
    {
        [Optional]
        [Template(TemplateUsage.NoPreference, "None")]
        public string Specials;
      
        public string DeliveryAddress;
        public DateTime DeliveryTime;

        public static IForm<BuilderSandwich> BuildForm()
        {
            OnCompletionAsyncDelegate<BuilderSandwich> processOrder = async (context, state) =>
            {
                await context.Context.SendActivityAsync("We are currently processing your sandwich. We will message you the status.");
            };

            return new FormBuilder<BuilderSandwich>()
                .Message("Welcome to the sandwich order bot!")
                .Field(nameof(Sandwich))
                .Field(nameof(Length))
                .Field(nameof(Bread))
                .Field(nameof(Cheese))
                .Field(nameof(Toppings),
                    validate: async (state, value) =>
                    {
                        var values = ((List<object>)value).OfType<ToppingOptions>();
                        var result = new ValidateResult { IsValid = true, Value = values };
                        if (values != null && values.Contains(ToppingOptions.Everything))
                        {
                            result.Value = (from ToppingOptions topping in Enum.GetValues(typeof(ToppingOptions))
                                            where topping != ToppingOptions.Everything && !values.Contains(topping)
                                            select topping).ToList();
                        }
                        return result;
                    })
                .Message("For sandwich toppings you have selected {Toppings}.")
                .Field(nameof(SandwichOrder.Sauce))
                .Field(new FieldReflector<BuilderSandwich>(nameof(Specials))
                    .SetType(null)
                    .SetActive((state) => state.Length == LengthOptions.FootLong)
                    .SetDefine(async (state, field) =>
                    {
                        field
                            .AddDescription("cookie", "Free cookie")
                            .AddTerms("cookie", "cookie", "free cookie")
                            .AddDescription("drink", "Free large drink")
                            .AddTerms("drink", "drink", "free drink");
                        return true;
                    }))
                .Confirm(async (state) =>
                {
                    var cost = 0.0;
                    switch (state.Length)
                    {
                        case LengthOptions.SixInch: cost = 5.0; break;
                        case LengthOptions.FootLong: cost = 6.50; break;
                    }
                    return new PromptAttribute($"Total for your sandwich is {cost:C2} is that ok?");
                })
                .Field(nameof(BuilderSandwich.DeliveryAddress),
                    validate: async (state, response) =>
                    {
                        var result = new ValidateResult { IsValid = true, Value = response };
                        var address = (response as string).Trim();
                        if (address.Length > 0 && (address[0] < '0' || address[0] > '9'))
                        {
                            result.Feedback = "Address must start with a number.";
                            result.IsValid = false;
                        }
                        return result;
                    })
                .Field(nameof(BuilderSandwich.DeliveryTime), "What time do you want your sandwich delivered? {||}")
				.Confirm("Do you want to order your {Length} {Sandwich} on {Bread} {&Bread} with {[{Cheese} {Toppings} {Sauce}]} to be sent to {DeliveryAddress} {?at {DeliveryTime:t}}?")
				.AddRemainingFields()
                .Message("Thanks for ordering a sandwich!")
                .OnCompletion(processOrder)
                .Build();
        }
    };
}
