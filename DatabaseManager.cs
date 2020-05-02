using System;
using System.Globalization;
using I18N.West;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using Steamworks;
using System.Threading;

namespace fr34kyn01535.Uconomy
{
    public class DatabaseManager
    {
        internal DatabaseManager()
        {
            new CP1250(); //Workaround for database encoding issues with mono
            CheckSchema();
        }

        private MySqlConnection CreateConnection()
        {
            MySqlConnection connection = null;
            try
            {
                if (Uconomy.Instance.Configuration.Instance.DatabasePort == 0)
                    Uconomy.Instance.Configuration.Instance.DatabasePort = 3306;
                connection = new MySqlConnection(
                    $"SERVER={Uconomy.Instance.Configuration.Instance.DatabaseAddress};DATABASE={Uconomy.Instance.Configuration.Instance.DatabaseName};UID={Uconomy.Instance.Configuration.Instance.DatabaseUsername};PASSWORD={Uconomy.Instance.Configuration.Instance.DatabasePassword};PORT={Uconomy.Instance.Configuration.Instance.DatabasePort};");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            return connection;
        }

        public void AddHistory(CSteamID id, string pluginName, decimal amount, string reason)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(yes => AddHistoryThread(id, pluginName, amount, reason));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void AddHistoryThread(CSteamID id, string pluginName, decimal amount, string reason)
        {
            ExecuteQuery(false,
                $"insert into `uconomy_history` (steamId,plugin,amount,reason,lastUpdated) values('{id}','{pluginName}','{amount}','{reason}',now())");
        }

        /// <summary>
        /// Retrieves the current balance of a specific account.
        /// </summary>
        /// <param name="id">The Steam 64 ID of the account to retrieve the balance from.</param>
        /// <returns>The balance of the account.</returns>
        public decimal GetBalance(string id)
        {
            decimal output = 0;
            var result = ExecuteQuery(true,
                $"select `balance` from `{Uconomy.Instance.Configuration.Instance.DatabaseTableName}` where `steamId` = '{id}';");

            if (result != null) decimal.TryParse(result.ToString(), out output);
            Uconomy.Instance.OnBalanceChecked(id, output);

            return output;
        }

        public decimal GetPremiumBalance(string id)
        {
            decimal output = 0;
            var result = ExecuteQuery(true,
                $"select `balance` from `{Uconomy.Instance.Configuration.Instance.DatabasePremiumTableName}` where `steamId` = '{id}';");

            if (result != null) decimal.TryParse(result.ToString(), out output);
            Uconomy.Instance.OnBalanceChecked(id, output);

            return output;
        }

        /// <summary>
        /// Increases the account balance of the specific ID with IncreaseBy.
        /// </summary>
        /// <param name="id">Steam 64 ID of the account.</param>
        /// <param name="increaseBy">The amount that the account should be changed with (can be negative).</param>
        /// <returns>The new balance of the account.</returns>
        public decimal IncreaseBalance(string id, decimal increaseBy)
        {
            decimal output = 0;

            var result = ExecuteQuery(true,
                $"update `{Uconomy.Instance.Configuration.Instance.DatabaseTableName}` set `balance` = balance + ({increaseBy.ToString(CultureInfo.InvariantCulture)}) where `steamId` = '{id}'; select `balance` from `{Uconomy.Instance.Configuration.Instance.DatabaseTableName}` where `steamId` = '{id}'");
            if (result != null) decimal.TryParse(result.ToString(), out output);

            Uconomy.Instance.BalanceUpdated(id, increaseBy);
            return output;
        }

        public decimal IncreasePremiumBalance(string id, decimal increaseBy)
        {
            decimal output = 0;

            var result = ExecuteQuery(true,
                $"update `{Uconomy.Instance.Configuration.Instance.DatabasePremiumTableName}` set `balance` = balance + ({increaseBy.ToString(CultureInfo.InvariantCulture)}) where `steamId` = '{id}'; select `balance` from `{Uconomy.Instance.Configuration.Instance.DatabasePremiumTableName}` where `steamId` = '{id}'");
            if (result != null) decimal.TryParse(result.ToString(), out output);

            Uconomy.Instance.BalanceUpdated(id, increaseBy);
            return output;
        }

        /// <summary>
        /// Ensures that the account exists in the database and creates it if it isn't.
        /// </summary>
        /// <param name="id">Steam 64 ID of the account to ensure its existence.</param>
        public void CheckSetupAccount(CSteamID id)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(yes => CheckSetupAccountThread(id));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void CheckSetupAccountThread(CSteamID id)
        {
            var exists = 0;
            var result = ExecuteQuery(true,
                $"SELECT EXISTS(SELECT 1 FROM `{Uconomy.Instance.Configuration.Instance.DatabaseTableName}` WHERE `steamId` ='{id}' LIMIT 1);");

            if (result != null) int.TryParse(result.ToString(), out exists);

            if (exists == 0)
                ExecuteQuery(false,
                    $"insert ignore into `{Uconomy.Instance.Configuration.Instance.DatabaseTableName}` (balance,steamId,lastUpdated) values({Uconomy.Instance.Configuration.Instance.InitialBalance.ToString(CultureInfo.InvariantCulture)},'{id}',now())");
        }

        public void CheckSetupPremiumAccount(CSteamID id)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(yes => CheckSetupPremiumAccountThread(id));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void CheckSetupPremiumAccountThread(CSteamID id)
        {
            var exists = 0;
            var result = ExecuteQuery(true,
                $"SELECT EXISTS(SELECT 1 FROM `{Uconomy.Instance.Configuration.Instance.DatabasePremiumTableName}` WHERE `steamId` ='{id}' LIMIT 1);");

            if (result != null) int.TryParse(result.ToString(), out exists);

            if (exists == 0)
                ExecuteQuery(false,
                    $"insert ignore into `{Uconomy.Instance.Configuration.Instance.DatabasePremiumTableName}` (balance,steamId,lastUpdated) values({Uconomy.Instance.Configuration.Instance.InitialPremiumBalance.ToString(CultureInfo.InvariantCulture)},'{id}',now())");
        }

        internal void CheckSchema()
        {
            var test = ExecuteQuery(true,
                $"show tables like '{Uconomy.Instance.Configuration.Instance.DatabaseTableName}'");

            if (test == null)
                ExecuteQuery(false,
                    $"CREATE TABLE `{Uconomy.Instance.Configuration.Instance.DatabaseTableName}` (`steamId` varchar(32) NOT NULL,`balance` decimal(15,2) NOT NULL DEFAULT '25.00',`lastUpdated` timestamp NOT NULL DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,PRIMARY KEY (`steamId`)) ");

            test = ExecuteQuery(true,
                $"show tables like '{Uconomy.Instance.Configuration.Instance.DatabasePremiumTableName}'");

            if (test == null)
                ExecuteQuery(false,
                    $"CREATE TABLE `{Uconomy.Instance.Configuration.Instance.DatabasePremiumTableName}` (`steamId` varchar(32) NOT NULL,`balance` decimal(15,2) NOT NULL DEFAULT '25.00',`lastUpdated` timestamp NOT NULL DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,PRIMARY KEY (`steamId`)) ");

            test = ExecuteQuery(true,
                $"show tables like 'uconomy_history'");
       
            if (test == null)
                ExecuteQuery(false,
                    $"CREATE TABLE `uconomy_history` (`steamId` varchar(32) NOT NULL,`plugin` text NOT NULL DEFAULT '',`amount` decimal(15,2) NOT NULL DEFAULT '0.00',`reason` text NOT NULL DEFAULT '',`lastUpdated` timestamp NOT NULL DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP) ");
        }

        /// <summary>
        /// Executes a MySql query.
        /// </summary>
        /// <param name="isScalar">If the query is expected to return a value.</param>
        /// <param name="query">The query to execute.</param>
        /// <returns>The value if isScalar is true, null otherwise.</returns>
        public object ExecuteQuery(bool isScalar, string query)
        {
            // This method is to reduce the amount of copy paste that there was within this class.
            // Initiate result and connection globally instead of within TryCatch context.
            var connection = CreateConnection();
            object result = null;

            try
            {
                // Initialize command within try context, and execute within it as well.
                var command = connection.CreateCommand();
                command.CommandText = query;

                connection.Open();
                if (isScalar)
                    result = command.ExecuteScalar();
                else
                    command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Catch and log any errors during execution, like connection or similar.
                Logger.LogException(ex);
            }
            finally
            {
                // No matter what happens, close the connection at the end of execution.
                connection.Close();
            }

            return result;
        }
    }
}