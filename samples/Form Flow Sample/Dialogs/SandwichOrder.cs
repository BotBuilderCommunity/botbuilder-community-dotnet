using System;
using System.Collections.Generic;
using Bot.Builder.Community.Dialogs.FormFlow;
#pragma warning disable 649

//from https://github.com/Microsoft/BotBuilder/blob/master/CSharp/Samples/SimpleSandwichBot/Sandwich.cs

// The SandwichOrder is an example of a simple form.  It must be serializable so FormFlow can save each answer in Bot State..
// The order of fields defines the default order in which questions will be asked.
// Enumerations shows the valid options for each field in the SandwichOrder and the order is the sequence the values 
// will be presented in the conversation.
namespace FormFlow_Sample.Dialogs
{
    public enum SandwichOptions
    {
        BLT, BlackForestHam, BuffaloChicken, ChickenAndBaconRanchMelt, ColdCutCombo, MeatballMarinara,
        OvenRoastedChicken, RoastBeef, RotisserieStyleChicken, SpicyItalian, SteakAndCheese, SweetOnionTeriyaki, Tuna,
        TurkeyBreast, Veggie
    };
    public enum LengthOptions { SixInch, FootLong };
    public enum BreadOptions { NineGrainWheat, NineGrainHoneyOat, Italian, ItalianHerbsAndCheese, Flatbread };
    public enum CheeseOptions { American, MontereyCheddar, Pepperjack };
    public enum ToppingOptions
    {
        Avocado, BananaPeppers, Cucumbers, GreenBellPeppers, Jalapenos,
        Lettuce, Olives, Pickles, RedOnion, Spinach, Tomatoes, Everything
    };
    public enum SauceOptions
    {
        ChipotleSouthwest, HoneyMustard, LightMayonnaise, RegularMayonnaise,
        Mustard, Oil, Pepper, Ranch, SweetOnion, Vinegar
    };

    [Serializable]
    public class SandwichOrder
    {
        public SandwichOptions? Sandwich;
        public LengthOptions? Length;
        public BreadOptions? Bread;
        public CheeseOptions? Cheese;
        public List<ToppingOptions> Toppings;
        public List<SauceOptions> Sauce;

        public static IForm<SandwichOrder> BuildForm()
        {
            //todo: FormCanceledException is not handled
            return new FormBuilder<SandwichOrder>()
                    .Message("Welcome to the simple sandwich order bot!")
                    .AddRemainingFields()
                    .Message("Thanks for ordering a sanwich!")
                    .Build();
        }
    };
}
