using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using UnityEngine;

namespace fr34kyn01535.Uconomy
{
    public class CommandPremiumBalance : IRocketCommand
    {
        public string Name => "premiumbalance";

        public string Help => "Shows the current premium balance";


        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Syntax => "(player)";

        public List<string> Aliases => new List<string> { "pbalance", "pbal" };

        public List<string> Permissions => new List<string> { "uconomy.premiumbalance" };

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if (command.Length == 1)
            {
                if (caller.HasPermission("uconomy.checkbalance"))
                {
                    var target = UnturnedPlayer.FromName(command[0]);
                    if (target != null)
                    {
                        var balance = Uconomy.Instance.Database.GetPremiumBalance(target.Id);
                        UnturnedChat.Say(caller,
                            Uconomy.Instance.Translate("command_premium_balance_show_otherPlayer",
                                Uconomy.Instance.Configuration.Instance.PremiumMoneySymbol, balance,
                                Uconomy.Instance.Configuration.Instance.PremiumMoneyName, target.CharacterName),
                            UnturnedChat.GetColorFromName(Uconomy.PremiumMessageColor, Color.green), "https://i.imgur.com/hOhFr7X.png");
                    }
                    else
                    {
                        UnturnedChat.Say(caller, Uconomy.Instance.Translate("command_balance_error_player_not_found"),
                            UnturnedChat.GetColorFromName(Uconomy.PremiumMessageColor, Color.green), "https://i.imgur.com/FeIvao9.png");
                    }
                }
                else
                {
                    UnturnedChat.Say(caller, Uconomy.Instance.Translate("command_balance_check_noPermissions"),
                        UnturnedChat.GetColorFromName(Uconomy.PremiumMessageColor, Color.green), "https://i.imgur.com/FeIvao9.png");
                }
            }
            else
            {
                var balance = Uconomy.Instance.Database.GetPremiumBalance(caller.Id);
                UnturnedChat.Say(caller,
                    Uconomy.Instance.Translations.Instance.Translate("command_premium_balance_show",
                        Uconomy.Instance.Configuration.Instance.PremiumMoneySymbol, balance,
                        Uconomy.Instance.Configuration.Instance.PremiumMoneyName),
                    UnturnedChat.GetColorFromName(Uconomy.PremiumMessageColor, Color.green), "https://i.imgur.com/hOhFr7X.png");
            }
        }
    }
}