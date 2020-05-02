using Rocket.API;

namespace fr34kyn01535.Uconomy
{
    public class UconomyConfiguration : IRocketPluginConfiguration
    {
        public string DatabaseAddress;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public string DatabasePremiumTableName;
        public int DatabasePort;

        public decimal InitialBalance;
        public decimal InitialPremiumBalance;
        public string MoneySymbol;
        public string PremiumMoneySymbol;
        public string MoneyName;
        public string PremiumMoneyName;
        public string MessageColor;
        public string PremiumMessageColor;
        public string MoneyFormat;
        public string PremiumMoneyFormat;
        public ushort EffectID;
        public ushort EffectID2;
        public ushort TextEffectID;
        public ushort TextEffectID2;

        public void LoadDefaults()
        {
            DatabaseAddress = "127.0.0.1";
            DatabaseUsername = "unturned";
            DatabasePassword = "password";
            DatabaseName = "unturned";
            DatabaseTableName = "uconomy";
            DatabasePremiumTableName = "uconomy_premium";
            DatabasePort = 3306;
            InitialBalance = 30;
            InitialPremiumBalance = 30;
            MoneySymbol = "P";
            PremiumMoneySymbol = "T";
            MoneyName = "POX";
            PremiumMoneyName = "TOR";
            MessageColor = "blue";
            PremiumMessageColor = "cyan";
            MoneyFormat = "[MONEY] {color=#62C765}[MONEYNAME]{/color}";
            PremiumMoneyFormat = "[MONEY] {color=#BD75FF}[MONEYNAME]{/color}";
            EffectID = 10010;
            EffectID2 = 10012;
            TextEffectID = 10011;
            TextEffectID2 = 10013;
        }
    }
}
