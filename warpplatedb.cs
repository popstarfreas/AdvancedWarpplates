using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Data;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace AdvancedWarpplates
{
    public class WarpplateDB
    {
        private IDbConnection database;
        private object syncLock = new object();

        public WarpplateDB(IDbConnection db)
        {
            database = db;

            var table = new SqlTable("Warpplates",
                new SqlColumn("X1", MySqlDbType.Int32),
                new SqlColumn("Y1", MySqlDbType.Int32),
                new SqlColumn("width", MySqlDbType.Int32),
                new SqlColumn("height", MySqlDbType.Int32),
                new SqlColumn("WarpplateName", MySqlDbType.VarChar, 50) { Primary = true },
                new SqlColumn("WorldID", MySqlDbType.Text),
                new SqlColumn("UserIds", MySqlDbType.Text),
                new SqlColumn("Protected", MySqlDbType.Int32),
                new SqlColumn("WarpplateDestination", MySqlDbType.VarChar, 50),
                new SqlColumn("Type", MySqlDbType.Int32) { DefaultValue = "0", NotNull = true },
                new SqlColumn("Delay", MySqlDbType.Int32),
                new SqlColumn("Label", MySqlDbType.Text)
            );
            var creator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            creator.EnsureTableStructure(table);


            Task.Run(async () =>
            {
                await ReloadAllWarpplates();
            });

        }

        internal async Task<QueryResult> QueryReader(string query, params object[] args)
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (syncLock)
                    {
                        return database.QueryReader(query, args);
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                    return null;
                }
            });
        }

        internal async Task<int> Query(string query, params object[] args)
        {
            return await Task.Run(() =>
            {
                try
                {
                    lock (syncLock)
                    {
                        return database.Query(query, args);
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.Error(ex.ToString());
                    return 0;
                }
            });
        }

        public async Task ConvertDB()
        {
            try
            {
                await Query("UPDATE Warpplates SET WorldID=@0, UserIds='', Delay=4", Main.worldID.ToString());
                await ReloadAllWarpplates();
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }
        }

        public async Task<List<Warpplate>> ReloadAllWarpplates()
        {
            List<Warpplate> warpplates = new List<Warpplate>();

            try
            {
                using (var reader = await QueryReader("SELECT * FROM Warpplates WHERE WorldID=@0", Main.worldID.ToString()))
                {
                    while (reader.Read())
                    {
                        int X1 = reader.Get<int>("X1");
                        int Y1 = reader.Get<int>("Y1");
                        int height = reader.Get<int>("height");
                        int width = reader.Get<int>("width");
                        int Protected = reader.Get<int>("Protected");
                        string mergedids = reader.Get<string>("UserIds");
                        string name = reader.Get<string>("WarpplateName");
                        int type = reader.Get<int>("Type");
                        string warpdest = reader.Get<string>("WarpplateDestination");
                        int Delay = reader.Get<int>("Delay");
                        string label = reader.Get<string>("Label");

                        string[] splitids = mergedids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        Warpplate r = new Warpplate(new Vector2(X1, Y1), new Rectangle(X1, Y1, width, height), name, warpdest, Protected != 0, Main.worldID.ToString(), label, type);
                        r.Delay = Delay;

                        try
                        {
                            for (int i = 0; i < splitids.Length; i++)
                            {
                                int id;

                                if (Int32.TryParse(splitids[i], out id)) // if unparsable, it's not an int, so silently skip
                                    r.AllowedIDs.Add(id);
                                else
                                    TShock.Log.Warn("One of your UserIDs is not a usable integer: " + splitids[i]);
                            }
                        }
                        catch (Exception e)
                        {
                            TShock.Log.Error("Your database contains invalid UserIDs (they should be ints).");
                            TShock.Log.Error("A lot of things will fail because of this. You must manually delete and re-create the allowed field.");
                            TShock.Log.Error(e.ToString());
                            TShock.Log.Error(e.StackTrace);
                        }

                        warpplates.Add(r);
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }

            return warpplates;
        }

        public async Task<bool> AddWarpplate(int tx, int ty, int width, int height, string Warpplatename, string Warpdest, string worldid)
        {
            try
            {
                int result = await Query("INSERT INTO Warpplates (X1, Y1, width, height, WarpplateName, WorldID, UserIds, Protected, WarpplateDestination, Delay, Label, Type) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, 0);",
                    tx, ty, width, height, Warpplatename, worldid, "", 1, Warpdest, 4, "");

                return result > 0;
            }
            catch (Exception ex)
            {
				TShock.Log.Error(ex.ToString());
            }
            return false;
        }

        public async Task<bool> DeleteWarpplate(Warpplate warpplate)
        {
            if (warpplate != null)
            {
                int result = await Query("DELETE FROM Warpplates WHERE WarpplateName=@0 AND WorldID=@1", warpplate.Name, Main.worldID.ToString());
                return result > 0;
            }
            return false;
        }

        public async Task<bool> SetWarpplateState(Warpplate warpplate, bool state)
        {
            try
            {
                int result = await Query("UPDATE Warpplates SET Protected=@0 WHERE WarpplateName=@1 AND WorldID=@2", state ? 1 : 0, warpplate.Name, Main.worldID.ToString());
                return result > 0;
            }
            catch (Exception ex)
            {
				TShock.Log.Error(ex.ToString());
            }

            return false;
        }

        public async Task<bool> UpdateWarpplate(Warpplate warpplate)
        {
            try
            {
                int result = await Query("UPDATE Warpplates SET width=@0, height=@1, Delay=@2, Label=@3 WHERE WarpplateName=@4 AND WorldID=@5",
                    warpplate.Area.Width, warpplate.Area.Height, warpplate.Delay, warpplate.Label, warpplate.Name, Main.worldID.ToString());

                return result > 0;
            }
            catch (Exception ex)
            {
				TShock.Log.Error(ex.ToString());
            }

            return false;
        }

        public async Task<bool> RemoveDestination(Warpplate warpplate)
        {
            int result = await Query("UPDATE Warpplates SET WarpplateDestination=@0 WHERE WarpplateName=@1 AND WorldID=@2", "", warpplate.Name, Main.worldID.ToString());

            return result > 0;
        }

        public async Task<bool> SetDestination(Warpplate warpplate, string warpplateDestination, int type = 0)
        {
            int result = await Query("UPDATE Warpplates SET WarpplateDestination=@0, Type=@3 WHERE WarpplateName=@1 AND WorldID=@2;", warpplateDestination, warpplate.Name, Main.worldID.ToString(), type);
            warpplate.Destination = warpplateDestination;

            return result > 0;
        }
    }
}