using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.Request
{
    public class ConversationRequest
    {
        public bool IsInSandbox { get; set; }
        public Surface Surface { get; set; }
        public Input[] Inputs { get; set; }
        public User User { get; set; }
        public Conversation Conversation { get; set; }
        public AvailableSurface[] AvailableSurfaces { get; set; }
    }

    public class Surface
    {
        public Capability[] Capabilities { get; set; }
    }

    public class Input
    {
        public RawInput[] RawInputs { get; set; }
        public Argument[] Arguments { get; set; }
        public string Intent { get; set; }
    }

    public class RawInput
    {
        public string Query { get; set; }
        public string InputType { get; set; }
        public string Url { get; set; }
    }

    public class Argument
    {   
        public string RawText { get; set; }
        public string TextValue { get; set; }
        public string Name { get; set; }
        public JObject Extension { get; set; }
        public int IntValue { get; set; }
        public float FloatValue { get; set; }
        public bool BoolValue { get; set; }
        public DateTime DateTimeValue { get; set; }
        public Location PlaceValue { get; set; }
        public JObject StructuredValue { get; set; }
    }

    public class Location
    {
        public LatLng Coordinates { get; set; }
        public string FormattedAddress { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public PostalAddress PostalAddress { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Notes { get; set; }
        public string PlaceId { get; set; }
    }

    public class PostalAddress
    {
        public int Revision { get; set; }
        public string RegionCode { get; set; }
        public string LanguageCode { get; set; }
        public string PostalCode { get; set; }
        public string SortingCode { get; set; }
        public string AdministrativeArea { get; set; }
        public string Locality { get; set; }
        public string Sublocality { get; set; }
        public string[] AddressLines { get; set; }
        public string[] Recipients { get; set; }
        public string Organization { get; set; }
    }

    public class LatLng
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

    public class User
    {
        public string IdToken { get; set; }

        public UserProfile Profile { get; set; }

        public string AccessToken { get; set; }

        public string[] Permissions;

        public string Locale { get; set; }

        public DateTime LastSeen { get; set; }

        public JObject UserStorage { get; set; }

        public PackageEntitlement[] PackageEntitlements { get; set; }

        public string UserVerificationStatus { get; set; }

        public string UserId { get; set; }
    }

    public class PackageEntitlement
    {
        public string PackageName { get; set; }

        public Entitlement[] Entitlements { get; set; }
    }

    public class Entitlement
    {
        public string Sku { get; set; }

        public string SkuType { get; set; }

        public SignedData InAppDetails { get; set; }
    }

    public class SignedData
    {
        public object InAppPurchaseData { get; set; }

        public string InAppDataSignature { get; set; }
    }

    public class UserProfile
    {
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
    }

    public class Conversation
    {
        public string ConversationId { get; set; }
        public string Type { get; set; }
        public string ConversationToken { get; set; }
    }

    public class Capability
    {
        public string Name { get; set; }
    }

    public class AvailableSurface
    {
        public Capability[] Capabilities { get; set; }
    }
}
