﻿using Discord;
using Discord.Commands;
using Discord.Modules;
using NadekoBot.Extensions;
using System.Linq;

namespace NadekoBot.Modules.Gambling
{
    internal class GamblingModule : DiscordModule
    {

        public GamblingModule()
        {
            commands.Add(new DrawCommand(this));
            commands.Add(new FlipCoinCommand(this));
            commands.Add(new DiceRollCommand(this));
        }

        public override string Prefix { get; } = NadekoBot.Config.CommandPrefixes.Gambling;

        public override void Install(ModuleManager manager)
        {
            manager.CreateCommands("", cgb =>
            {
                cgb.AddCheck(Classes.Permissions.PermissionChecker.Instance);

                commands.ForEach(com => com.Init(cgb));

                cgb.CreateCommand(Prefix + "raffle")
                  .Description("Prints a name and ID of a random user from the online list from the (optional) role.")
                  .Parameter("role", ParameterType.Optional)
                  .Do(RaffleFunc());
                cgb.CreateCommand(Prefix + "$$")
                  .Description("Check how many NadekoFlowers you have.")
                  .Do(NadekoFlowerCheckFunc());
            });
        }

        private static System.Func<CommandEventArgs, System.Threading.Tasks.Task> NadekoFlowerCheckFunc()
        {
            return async e =>
            {
                var pts = Classes.DbHandler.Instance.GetStateByUserId((long)e.User.Id)?.Value ?? 0;
                var str = $"`You have {pts} NadekoFlowers".SnPl((int)pts) + "`\n";
                for (var i = 0; i < pts; i++)
                {
                    str += "🌸";
                }
                await e.Channel.SendMessage(str);
            };
        }

        private static System.Func<CommandEventArgs, System.Threading.Tasks.Task> RaffleFunc()
        {
            return async e =>
            {
                var arg = string.IsNullOrWhiteSpace(e.GetArg("role")) ? "@everyone" : e.GetArg("role");
                var role = e.Server.FindRoles(arg).FirstOrDefault();
                if (role == null)
                {
                    await e.Channel.SendMessage("💢 Role not found.");
                    return;
                }
                var members = role.Members.Where(u => u.Status == Discord.UserStatus.Online); // only online
                var membersArray = members as User[] ?? members.ToArray();
                var usr = membersArray[new System.Random().Next(0, membersArray.Length)];
                await e.Channel.SendMessage($"**Raffled user:** {usr.Name} (id: {usr.Id})");
            };
        }
    }
}
