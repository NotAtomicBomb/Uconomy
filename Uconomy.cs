using System;
using System.Collections.Generic;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace fr34kyn01535.Uconomy
{
    public class Uconomy : RocketPlugin<UconomyConfiguration>
    {
        public DatabaseManager Database;
        public static Uconomy Instance;
        public Dictionary<CSteamID, string> Status = new Dictionary<CSteamID, string>();

        public static string MessageColor;
        public static string PremiumMessageColor;

        protected override void Load()
        {
            Instance = this;
            Database = new DatabaseManager();
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
            OnBalanceUpdate += OnBalanceUpdated;
            MessageColor = Configuration.Instance.MessageColor;
            PremiumMessageColor = Configuration.Instance.PremiumMessageColor;
        }

        protected override void Unload()
        {
            Instance = null;
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
        }

        public delegate void PlayerBalanceUpdate(UnturnedPlayer player, decimal amt);

        public event PlayerBalanceUpdate OnBalanceUpdate;

        public delegate void PlayerBalanceCheck(UnturnedPlayer player, decimal balance);

        public event PlayerBalanceCheck OnBalanceCheck;

        public delegate void PlayerPay(UnturnedPlayer sender, string receiver, decimal amt);

        public event PlayerPay OnPlayerPay;

        public override TranslationList DefaultTranslations =>
            new TranslationList
            {
                {"command_balance_show", "Your current balance is: {0} {1} {2}"},
                {"command_premium_balance_show", "Your current premium balance is: {0} {1} {2}"},
                {"command_balance_error_player_not_found", "Failed to find player!"},
                {"command_balance_check_noPermissions", "Insufficent Permissions!"},
                {"command_balance_show_otherPlayer", "{3}'s current balance is: {0} {1} {2}"},
                {"command_premium_balance_show_otherPlayer", "{3}'s current premium balance is: {0} {1} {2}"},
                {"command_pay_invalid", "Invalid arguments"},
                {"command_pay_error_pay_self", "You cant pay yourself"},
                {"command_pay_error_invalid_amount", "Invalid amount"},
                {"command_pay_error_cant_afford", "Your balance does not allow this payment"},
                {"command_premium_pay_error_cant_afford", "Your premium balance does not allow this payment"},
                {"command_pay_error_player_not_found", "Failed to find player"},
                {"command_pay_private", "You paid {0} {1} {2}"},
                {"command_pay_console", "You received a payment of {0} {1} "},
                {"command_pay_other_private", "You received a payment of {0} {1} from {2}"}
            };

        internal void HasBeenPayed(UnturnedPlayer sender, string receiver, decimal amt)
        {
            OnPlayerPay?.Invoke(sender, receiver, amt);
        }

        internal void BalanceUpdated(string steamId, decimal amt)
        {
            if (OnBalanceUpdate == null) return;

            var player = UnturnedPlayer.FromCSteamID(new CSteamID(Convert.ToUInt64(steamId)));
            OnBalanceUpdate(player, amt);
            if (amt >= 0) Database.AddHistory(player.CSteamID, "Uconomy", amt, "Increase");
            if (amt < 0) Database.AddHistory(player.CSteamID, "Uconomy", amt, "Decrease");
        }
        
        private void OnBalanceUpdated(UnturnedPlayer player, decimal amount)
        {
            if (!Status.TryGetValue(player.CSteamID, out _)) { return; }
            if(Status[player.CSteamID] == "on")
            {
                EffectManager.sendUIEffect(Configuration.Instance.TextEffectID, (short)Configuration.Instance.TextEffectID, player.CSteamID, true, Configuration.Instance.MoneyFormat.Replace("{", "<").Replace("}", ">").Replace("[MONEY]", Instance.Database.GetBalance(player.CSteamID.ToString()).ToString()).Replace("[MONEYNAME]", Configuration.Instance.MoneyName), Configuration.Instance.PremiumMoneyFormat.Replace("{", "<").Replace("}", ">").Replace("[MONEY]", Instance.Database.GetPremiumBalance(player.CSteamID.ToString()).ToString()).Replace("[MONEYNAME]", Configuration.Instance.PremiumMoneyName));
            }
            else if (Status[player.CSteamID] == "right")
            {
                EffectManager.sendUIEffect(Instance.Configuration.Instance.TextEffectID2, (short)Instance.Configuration.Instance.TextEffectID2, player.CSteamID, true, Instance.Configuration.Instance.MoneyFormat.Replace("{", "<").Replace("}", ">").Replace("[MONEY]", Instance.Database.GetBalance(player.CSteamID.ToString()).ToString()).Replace("[MONEYNAME]", Instance.Configuration.Instance.MoneyName), Configuration.Instance.PremiumMoneyFormat.Replace("{", "<").Replace("}", ">").Replace("[MONEY]", Instance.Database.GetPremiumBalance(player.CSteamID.ToString()).ToString()).Replace("[MONEYNAME]", Instance.Configuration.Instance.PremiumMoneyName));
            }
        }

        internal void OnBalanceChecked(string steamId, decimal balance)
        {
            if (OnBalanceCheck == null) return;

            var player = UnturnedPlayer.FromCSteamID(new CSteamID(Convert.ToUInt64(steamId)));
            OnBalanceCheck(player, balance);
        }

        public void DoUI(UnturnedPlayer player, string value)
        {
            if(value == "up" || value == "on")
            {
                EffectManager.sendUIEffect(Instance.Configuration.Instance.EffectID, (short)Instance.Configuration.Instance.EffectID, player.CSteamID, true);
                EffectManager.sendUIEffect(Instance.Configuration.Instance.TextEffectID, (short)Instance.Configuration.Instance.TextEffectID, player.CSteamID, true, Instance.Configuration.Instance.MoneyFormat.Replace("{", "<").Replace("}", ">").Replace("[MONEY]", Instance.Database.GetBalance(player.CSteamID.ToString()).ToString()).Replace("[MONEYNAME]", Instance.Configuration.Instance.MoneyName), Configuration.Instance.PremiumMoneyFormat.Replace("{", "<").Replace("}", ">").Replace("[MONEY]", Instance.Database.GetPremiumBalance(player.CSteamID.ToString()).ToString()).Replace("[MONEYNAME]", Instance.Configuration.Instance.PremiumMoneyName));
                UnturnedChat.Say(player, "Showing UconomyUI...", UnturnedChat.GetColorFromName(PremiumMessageColor, Color.white), "https://i.imgur.com/hOhFr7X.png");
                Status[player.CSteamID] = "on";
            }
            else if(value == "right")
            {
                EffectManager.sendUIEffect(Instance.Configuration.Instance.EffectID2, (short)Instance.Configuration.Instance.EffectID2, player.CSteamID, true);
                EffectManager.sendUIEffect(Instance.Configuration.Instance.TextEffectID2, (short)Instance.Configuration.Instance.TextEffectID2, player.CSteamID, true, Instance.Configuration.Instance.MoneyFormat.Replace("{", "<").Replace("}", ">").Replace("[MONEY]", Instance.Database.GetBalance(player.CSteamID.ToString()).ToString()).Replace("[MONEYNAME]", Instance.Configuration.Instance.MoneyName), Configuration.Instance.PremiumMoneyFormat.Replace("{", "<").Replace("}", ">").Replace("[MONEY]", Instance.Database.GetPremiumBalance(player.CSteamID.ToString()).ToString()).Replace("[MONEYNAME]", Instance.Configuration.Instance.PremiumMoneyName));
                UnturnedChat.Say(player, "Showing UconomyUI...", UnturnedChat.GetColorFromName(PremiumMessageColor, Color.white), "https://i.imgur.com/hOhFr7X.png");
                Status[player.CSteamID] = "right";
            }
            else if(value == "off")
            {
                EffectManager.askEffectClearByID(Instance.Configuration.Instance.EffectID, player.CSteamID);
                EffectManager.askEffectClearByID(Instance.Configuration.Instance.EffectID2, player.CSteamID);
                EffectManager.askEffectClearByID(Instance.Configuration.Instance.TextEffectID, player.CSteamID);
                EffectManager.askEffectClearByID(Instance.Configuration.Instance.TextEffectID2, player.CSteamID);
                UnturnedChat.Say(player, "Hiding UconomyUI...", UnturnedChat.GetColorFromName(MessageColor, Color.white), "https://i.imgur.com/hOhFr7X.png");
                Status.Remove(player.CSteamID);
                return;
            }
        }

        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            // Ensure that the account exists within the database.
            Database.CheckSetupAccount(player.CSteamID);
            Database.CheckSetupPremiumAccount(player.CSteamID);

            Status[player.CSteamID] = "on";
            EffectManager.sendUIEffect(Configuration.Instance.EffectID, (short)Configuration.Instance.EffectID, player.CSteamID, true);
            EffectManager.sendUIEffect(Configuration.Instance.TextEffectID, (short)Configuration.Instance.TextEffectID, player.CSteamID, true, Configuration.Instance.MoneyFormat.Replace("{", "<").Replace("}", ">").Replace("[MONEY]", Instance.Database.GetBalance(player.CSteamID.ToString()).ToString()).Replace("[MONEYNAME]", Configuration.Instance.MoneyName), Configuration.Instance.PremiumMoneyFormat.Replace("{", "<").Replace("}", ">").Replace("[MONEY]", Instance.Database.GetPremiumBalance(player.CSteamID.ToString()).ToString()).Replace("[MONEYNAME]", Configuration.Instance.PremiumMoneyName));
        }

        private void Events_OnPlayerDisconnected(UnturnedPlayer player)
        {
            Status.Remove(player.CSteamID);
        }
    }
}