using System;

namespace Bot.Builder.Community.Dialogs.Location
{
    [Flags]
    public enum LocationOptions
    {
        /// <summary>
        /// No options.
        /// </summary>
        None = 0,

        /// <summary>
        /// Some of the channels (e.g. Facebook) has a built in
        /// location widget. Use this option to indicate if you want
        /// the <c>LocationDialog</c> to use it when available.
        /// </summary>
        UseNativeControl = 1,

        /// <summary>
        /// Use this option if you want the location dialog to reverse lookup
        /// geo-coordinates before returning. This can be useful if you depend
        /// on the channel location service or native control to get user location
        /// but still want the control to return to you a full address.
        /// </summary>
        /// <remarks>
        /// Due to the accuracy limitations of reverse geo-coders, we only use it to capture
        /// <see cref="PostalAddress.Locality"/>, <see cref="PostalAddress.Region"/>,
        /// <see cref="PostalAddress.Country"/>, and <see cref="PostalAddress.PostalCode"/>
        /// </remarks>
        ReverseGeocode = 2,

        /// <summary>
        /// Use this option if you do not want the <c>LocationDialog</c> to offer
        /// keeping track of the user's favorite locations.
        /// </summary>
        SkipFavorites = 4,

        /// <summary>
        /// Use this option if you want the location dialog to skip the final 
        /// confirmation before returning the location
        /// </summary>
        SkipFinalConfirmation = 8
    }
}
