using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using Rocket.Unturned.Chat;

namespace fr34kyn01535.Uconomy
{
    public class CommandUconomyUI : IRocketCommand
    {
        public string Name => "uconomyui";
        public string Help => "Show/Hide UconomyUI";
        public string Syntax => "";
        public List<string> Aliases => new List<string>() { "uui" };
        public List<string> Permissions => new List<string>() { "uconomy.ui" };
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command[0] == "on")
            {
                Uconomy.Instance.DoUI(player, command[0]);
            }
            else if (command[0] == "off")
            {
                Uconomy.Instance.DoUI(player, command[0]);
            }
            else
            {
                UnturnedChat.Say(caller, "Usage: /uui <up | right | on | off>", UnturnedChat.GetColorFromName(Uconomy.MessageColor, UnityEngine.Color.white), "https://i.imgur.com/hOhFr7X.png");
            }
        }
    }
}
