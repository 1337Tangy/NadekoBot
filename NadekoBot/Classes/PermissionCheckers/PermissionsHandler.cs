﻿using Discord;
using Discord.Commands.Permissions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace NadekoBot.Classes.Permissions {
    public static class PermissionsHandler {
        public static ConcurrentDictionary<Server, ServerPermissions> _permissionsDict =
            new ConcurrentDictionary<Server, ServerPermissions>();

        private static void WriteServerToJson(Server server) {
            string pathToFile = $"data/permissions/{server.Id}.json";
            File.WriteAllText(pathToFile, Newtonsoft.Json.JsonConvert.SerializeObject(_permissionsDict[server], Newtonsoft.Json.Formatting.Indented));
        }

        public static void WriteToJson() {
            Directory.CreateDirectory("data/permissions/");
            foreach (var kvp in _permissionsDict) {
                WriteServerToJson(kvp.Key);
            }
        }
    }
    /// <summary>
    /// Holds a permission list
    /// </summary>
    public class Permissions {
        /// <summary>
        /// Module name with allowed/disallowed
        /// </summary>
        public Dictionary<string, bool> modules { get; set; }
        /// <summary>
        /// Command name with allowed/disallowed
        /// </summary>
        public Dictionary<string, bool> commands { get; set; }
    }

    public class PermissionsContainer {
        /// <summary>
        /// The id of the thing (user/server/channel)
        /// </summary>
        public string Id { get; set; } //a string because of the role name.
        /// <summary>
        /// Permission object bound to the id of something/role name
        /// </summary>
        public Permissions Permissions { get; set; }

        public PermissionsContainer() { }
    }

    public class ServerPermissions : PermissionsContainer {
        /// <summary>
        /// The guy who can edit the permissions
        /// </summary>
        public string PermissionsControllerRoleName { get; set; }
        /// <summary>
        /// Does it print the error when a restriction occurs
        /// </summary>
        public bool Verbose { get; set; }
        
        public List<PermissionsContainer> UserPermissions { get; set; }
        public List<PermissionsContainer> ChannelPermissions { get; set; }
        public List<PermissionsContainer> RolePermissions { get; set; }
    }
}