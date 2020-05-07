using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bot.Builder.Community.Adapters.Google.Core.Model.SystemIntents
{
    public class PermissionsIntent : SystemIntent
    {
        public PermissionsIntent()
        {
            Intent = "actions.intent.PERMISSION";
        }

        public PermissionsInputValueData InputValueData { get; set; }
    }

    public class PermissionsInputValueData : IntentInputValueData
    {
        public PermissionsInputValueData()
        {
            Type = "type.googleapis.com/google.actions.v2.PermissionValueSpec";
        }

        public string OptContext { get; set; }

        public List<Permission> Permissions { get; set; } = new List<Permission>();
    }

    public enum Permission
    {
        UNSPECIFIED_PERMISSION,
        NAME,
        DEVICE_PRECISE_LOCATION,
        DEVICE_COARSE_LOCATION,
        UPDATE
    }
}