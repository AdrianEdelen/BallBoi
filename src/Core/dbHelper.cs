using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    internal static class dbHelper
    {

        internal static int CreateUserTableIfNotExists(SqliteConnection conn)
        {
            conn.Open();
            Console.WriteLine("Creating User Table if it doesn't exist");
            var sqlString = $"CREATE TABLE IF NOT EXISTS USERS (Id INTEGER PRIMARY KEY, NAME TEXT NOT NULL, DISCORDID TEXT NOT NULL, ACKWARN INTEGER NOT NULL, AVAILTOKENS INTEGER NOT NULL)";

            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlString;
            var rowsAffected = cmd.ExecuteNonQuery();
            Console.WriteLine($"Rows Affected: {rowsAffected}");
            conn.Close();
            return rowsAffected;
        }
        internal static int CreatePromptTableIfNotExists(SqliteConnection conn)
        {
            conn.Open();
            Console.WriteLine("Creating prmpt Table if it doesn't exist");
            var sqlString = $"CREATE TABLE IF NOT EXISTS PROMPTS (Id INTEGER PRIMARY KEY, USER INTEGER NOT NULL, TOKENS INTEGER NOT NULL, PROMPT TEXT NOT NULL)";

            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlString;
            var rowsAffected = cmd.ExecuteNonQuery();
            Console.WriteLine($"Rows Affected: {rowsAffected}");
            conn.Close();
            return rowsAffected;
        }
        internal static int CreateResponseTableIfNotExists(SqliteConnection conn)
        {
            conn.Open();
            Console.WriteLine("Creating response Table if it doesn't exist");
            var sqlString = $"CREATE TABLE IF NOT EXISTS RESPONSES (Id INTEGER PRIMARY KEY, USER INTEGER NOT NULL, TOKENS INTEGER NOT NULL, RESPONSE TEXT NOT NULL)";

            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlString;
            var rowsAffected = cmd.ExecuteNonQuery();
            Console.WriteLine($"Rows Affected: {rowsAffected}");
            conn.Close();
            return rowsAffected;
        }
        internal static int AddNewUserToUsersTable(SqliteConnection conn, ulong discordID, string name)
        {
            conn.Open();
            Console.WriteLine($"Adding a new user to the DB {name} | {discordID}");
            bool isAck = true;
            int availTok = 5000;

            var sqlString = $"INSERT INTO USERS (NAME, DISCORDID, ACKWARN, AVAILTOKENS) VALUES ('{name}', '{discordID}', '{isAck}', '{availTok}');";
            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlString;
            var rowsAffected = cmd.ExecuteNonQuery();
            Console.WriteLine($"rows Affected {rowsAffected}");
            conn.Close();
            return rowsAffected;
        }
        internal static dbUser GetUserFromUsersTable(SqliteConnection conn, ulong discordId)
        {
            conn.Open();
            Console.WriteLine($"Getting user: {discordId} from DB");
            SqliteDataReader reader;
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM USERS WHERE DISCORDID = {discordId};";
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = int.Parse(reader.GetString(0));
                string name = reader.GetString(1);
                ulong discId = ulong.Parse(reader.GetString(2));
                bool ackWarn = bool.Parse(reader.GetString(3));
                int tokens = int.Parse(reader.GetString(4));

                Console.WriteLine($"Found user {name}");
                conn.Close();
                return new dbUser(id, name, discId, ackWarn, tokens);
                
            }
            Console.WriteLine("No User found");
            conn.Close();
            return null;
        }

        internal static List<dbUser> GetAllUsers(SqliteConnection conn)
        {
            conn.Open();
            Console.WriteLine("Getting all users");
            SqliteDataReader reader;
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM USERS;";
            reader = cmd.ExecuteReader();
            var users = new List<dbUser>();
            while (reader.Read())
            {
                int id = int.Parse(reader.GetString(0));
                string name = reader.GetString(1);
                ulong discId = ulong.Parse(reader.GetString(2));
                bool ackWarn = bool.Parse(reader.GetString(3));
                int tokens = int.Parse(reader.GetString(4));

                Console.WriteLine($"Found user {name}");
                
                users.Add(new dbUser(id, name, discId, ackWarn, tokens));

            }
            conn.Close();
            return users;
        }

        internal static bool UserExistsInDB(SqliteConnection conn, ulong discordId)
        {
            conn.Open();
            Console.WriteLine($"checking if user {discordId} exists in the db");
            SqliteDataReader reader;
            SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT * FROM USERS WHERE DISCORDID = {discordId};";
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine("found a user");
                string info = reader.GetString(0);
                Console.WriteLine(info);
                conn.Close();
                return true;
            }
            Console.WriteLine("no user found");
            conn.Close();
            return false;

        }
        internal static dbUser GetUpdatedUser(SqliteConnection conn, dbUser user)
        {
            
            Console.WriteLine("getting updated user info from database");
            return GetUserFromUsersTable(conn, user.DiscordId);
        }
        
        internal static bool UserHasTokens(SqliteConnection conn, dbUser user)
        {
            //user.
            return false;
        }

        internal static int UpdateUserTokens(SqliteConnection conn, dbUser user, int tokensUsed)
        {
            conn.Open();
            Console.WriteLine("Updating user token count");
            var newTokenCount = user.AvailableTokens - tokensUsed;

            var sqlString = $"UPDATE USERS SET AVAILTOKENS = {newTokenCount} WHERE Id = {user.Id};";
            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlString;
            
            var rowsaffected = cmd.ExecuteNonQuery();
            conn.Close();
            return rowsaffected;
        }
        internal static int AddUserTokens(SqliteConnection conn, dbUser user, int tokensToAdd)
        {
            conn.Open();
            Console.WriteLine("Updating user token count(awarding tokens");
            var newTokenCount = user.AvailableTokens + tokensToAdd;

            var sqlString = $"UPDATE USERS SET AVAILTOKENS = {newTokenCount} WHERE Id = {user.Id};";
            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlString;

            var rowsaffected = cmd.ExecuteNonQuery();
            conn.Close();
            return rowsaffected;
        }

        internal static int InsertPrompt(SqliteConnection conn, dbUser user, int tokens, string prompt)
        {

            //PROMPTS (Id INTEGER PRIMARY KEY, USER INTEGER NOT NULL, TOKENS INTEGER NOT NULL, PROMPT TEXT NOT NULL";

            conn.Open();
            var sqlString = $"INSERT INTO PROMPTS (USER, TOKENS, PROMPT) VALUES ('{user.Id}', '{tokens}', '{prompt}');";
            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlString;
            var rowsAffected = cmd.ExecuteNonQuery();
            Console.WriteLine($"rows Affected {rowsAffected}");
            conn.Close();
            return rowsAffected;
        }
        internal static int InsertResponse(SqliteConnection conn, dbUser user, int tokens, string response)
        {
            // RESPONSES (Id INTEGER PRIMARY KEY, USER INTEGER NOT NULL, TOKENS INTEGER NOT NULL, RESPONSE TEXT NOT NULL";
            conn.Open();
            var sqlString = $"INSERT INTO RESPONSES (USER, TOKENS, RESPONSE) VALUES ('{user.Id}', '{tokens}', '{response}');";
            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlString;
            var rowsAffected = cmd.ExecuteNonQuery();
            Console.WriteLine($"rows Affected {rowsAffected}");
            conn.Close();
            return rowsAffected;
        }

    }

    internal class dbUser
    {
        internal int Id { get; set; }
        internal string Name { get; set; }
        internal ulong DiscordId { get; set; }
        internal bool AcknowledgeWarn { get; set; } = false;
        internal int AvailableTokens { get; set; }

        public dbUser(int id, string name, ulong discId, bool ackWarn, int tokens)
        {
            Id = id;
            Name = name;
            DiscordId = discId;
            AcknowledgeWarn = ackWarn;
            AvailableTokens = tokens;
        }

    }
}
