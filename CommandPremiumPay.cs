using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;
using UnityEngine;

namespace fr34kyn01535.Uconomy
{
    public class CommandPremiumPay : IRocketCommand
    {
        public string Help => "Pays a specific player premium money from your account";

        public string Name => "premiumpay";

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Syntax => "<player> <amount>";

        public List<string> Aliases => new List<string> { "ppay" };

        public List<string> Permissions => new List<string> { "uconomy.premiumpay" };

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if (command.Length != 2)
            {
                UnturnedChat.Say(caller, Uconomy.Instance.Translations.Instance.Translate("command_pay_invalid"),
                    UnturnedChat.GetColorFromName(Uconomy.PremiumMessageColor, Color.green), "https://i.imgur.com/FeIvao9.png");
                return;
            }

            var otherPlayer = command.GetCSteamIDParameter(0)?.ToString();
            var otherPlayerOnline = UnturnedPlayer.FromName(command[0]);
            if (otherPlayerOnline != null) otherPlayer = otherPlayerOnline.Id;

            if (otherPlayer != null)
            {
                if (caller.Id == otherPlayer)
                {
                    UnturnedChat.Say(caller,
                        Uconomy.Instance.Translations.Instance.Translate("command_pay_error_pay_self"),
                        UnturnedChat.GetColorFromName(Uconomy.PremiumMessageColor, Color.green), "https://i.imgur.com/FeIvao9.png");
                    return;
                }

                if (!decimal.TryParse(command[1], out var amount) || amount <= 0)
                {
                    UnturnedChat.Say(caller,
                        Uconomy.Instance.Translations.Instance.Translate("command_pay_error_invalid_amount"),
                        UnturnedChat.GetColorFromName(Uconomy.PremiumMessageColor, Color.green), "https://i.imgur.com/FeIvao9.png");
                    return;
                }

                if (caller is ConsolePlayer)
                {
                    Uconomy.Instance.Database.IncreasePremiumBalance(otherPlayer, amount);
                    if (otherPlayerOnline != null)
                        UnturnedChat.Say(otherPlayerOnline,
                            Uconomy.Instance.Translations.Instance.Translate("command_pay_console", amount,
                                Uconomy.Instance.Configuration.Instance.PremiumMoneyName),
                            UnturnedChat.GetColorFromName(Uconomy.PremiumMessageColor, Color.green));
                }
                else
                {
                    var myBalance = Uconomy.Instance.Database.GetPremiumBalance(caller.Id);
                    if (myBalance - amount <= 0)
                    {
                        UnturnedChat.Say(caller,
                            Uconomy.Instance.Translations.Instance.Translate("command_premium_pay_error_cant_afford"),
                            UnturnedChat.GetColorFromName(Uconomy.PremiumMessageColor, Color.green), "https://i.imgur.com/FeIvao9.png");
                    }
                    else
                    {
                        Uconomy.Instance.Database.IncreasePremiumBalance(caller.Id, -amount);
                        if (otherPlayerOnline != null)
                            UnturnedChat.Say(caller,
                                Uconomy.Instance.Translations.Instance.Translate("command_pay_private",
                                    otherPlayerOnline.CharacterName, amount,
                                    Uconomy.Instance.Configuration.Instance.PremiumMoneyName),
                                UnturnedChat.GetColorFromName(Uconomy.PremiumMessageColor, Color.green), "https://i.imgur.com/hOhFr7X.png");
                        else
                            UnturnedChat.Say(caller,
                                Uconomy.Instance.Translations.Instance.Translate("command_pay_private", otherPlayer,
                                    amount, Uconomy.Instance.Configuration.Instance.PremiumMoneyName),
                                UnturnedChat.GetColorFromName(Uconomy.PremiumMessageColor, Color.green), "https://i.imgur.com/hOhFr7X.png");

                        Uconomy.Instance.Database.IncreasePremiumBalance(otherPlayer, amount);
                        if (otherPlayerOnline != null)
                            UnturnedChat.Say(otherPlayerOnline.CSteamID,
                                Uconomy.Instance.Translations.Instance.Translate("command_pay_other_private", amount,
                                    Uconomy.Instance.Configuration.Instance.PremiumMoneyName, caller.DisplayName),
                                UnturnedChat.GetColorFromName(Uconomy.PremiumMessageColor, Color.green), "https://i.imgur.com/hOhFr7X.png");

                        Uconomy.Instance.HasBeenPayed((UnturnedPlayer)caller, otherPlayer, amount);
                        Uconomy.Instance.Database.AddHistory(otherPlayerOnline.CSteamID, "Uconomy", amount, "Paid by " + caller.DisplayName);
                        Uconomy.Instance.Database.AddHistory(((UnturnedPlayer)caller).CSteamID, "Uconomy", -amount, "Paid to " + caller.DisplayName);
                    }
                }
            }
            else
            {
                UnturnedChat.Say(caller,
                    Uconomy.Instance.Translations.Instance.Translate("command_pay_error_player_not_found"), "https://i.imgur.com/FeIvao9.png");
            }
        }
    }
}