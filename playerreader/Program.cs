using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using Rests;
using System.ComponentModel;
using TShockAPI.DB;
using System.Collections;

namespace PlayerReader
{
    [ApiVersion(2, 1)]
    public class PlayerReader : TerrariaPlugin
    {
        public override string Author => "Quinci";

        public override string Description => "Adds a rest endpoint for more player data.";

        public override string Name => "Player Rest";

        public override Version Version => new Version(1, 0, 0, 0);

        public PlayerReader(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInit);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInit);
            }
            base.Dispose(disposing);
        }

        private void OnInit(EventArgs e)
        {
            TShock.RestApi.RegisterRedirect("/readplayers", "/readplayers");
            TShock.RestApi.Register(new SecureRestCommand("/readplayers", PlayerRead, RestPermissions.restuserinfo));
            TShock.RestApi.Register(new SecureRestCommand("/darkgaming/v1/players", PlayerList, RestPermissions.restuserinfo));
        }

        private object PlayerFind(EscapedParameterCollection parameters)
        {
            string name = parameters["player"];
            if (string.IsNullOrWhiteSpace(name))
            {
                return new RestObject("400")
                {
                    Error = "Missing or empty 'player' parameter"
                };
            }

            var found = TSPlayer.FindByNameOrID(name);
            if (found.Count == 1)
            {
                return found[0];
            }
            else if (found.Count == 0)
            {
                UserAccount account = TShock.UserAccounts.GetUserAccountByName(name);
                if (account != null)
                {
                    try
                    {
                        using (var reader = TShock.DB.QueryReader("SELECT Inventory, Health, Mana, MaxHealth, MaxMana FROM sscinventory WHERE Account=@0", account.ID))
                        {
                            if (reader.Read())
                            {
                                List<NetItem> inventoryList = reader.Get<string>("Inventory").Split('~').Select(NetItem.Parse).ToList();
                                var player = new Player();
                                for (int i = 0; i < NetItem.MaxInventory; i++)
                                {
                                    if (i < NetItem.InventoryIndex.Item2)
                                    {
                                        //0-58
                                        player.inventory[i].netDefaults(inventoryList[i].NetId);

                                        if (player.inventory[i].netID != 0)
                                        {
                                            player.inventory[i].stack = inventoryList[i].Stack;
                                            player.inventory[i].prefix = inventoryList[i].PrefixId;
                                        }
                                    }
                                    else if (i < NetItem.ArmorIndex.Item2)
                                    {
                                        //59-78
                                        var index = i - NetItem.ArmorIndex.Item1;
                                        player.armor[index].netDefaults(inventoryList[i].NetId);

                                        if (player.armor[index].netID != 0)
                                        {
                                            player.armor[index].stack = inventoryList[i].Stack;
                                            player.armor[index].prefix = (byte)inventoryList[i].PrefixId;
                                        }
                                    }
                                    else if (i < NetItem.DyeIndex.Item2)
                                    {
                                        //79-88
                                        var index = i - NetItem.DyeIndex.Item1;
                                        player.dye[index].netDefaults(inventoryList[i].NetId);

                                        if (player.dye[index].netID != 0)
                                        {
                                            player.dye[index].stack = inventoryList[i].Stack;
                                            player.dye[index].prefix = (byte)inventoryList[i].PrefixId;
                                        }
                                    }
                                    else if (i < NetItem.MiscEquipIndex.Item2)
                                    {
                                        //89-93
                                        var index = i - NetItem.MiscEquipIndex.Item1;
                                        player.miscEquips[index].netDefaults(inventoryList[i].NetId);

                                        if (player.miscEquips[index].netID != 0)
                                        {
                                            player.miscEquips[index].stack = inventoryList[i].Stack;
                                            player.miscEquips[index].prefix = (byte)inventoryList[i].PrefixId;
                                        }
                                    }
                                    else if (i < NetItem.MiscDyeIndex.Item2)
                                    {
                                        //93-98
                                        var index = i - NetItem.MiscDyeIndex.Item1;
                                        player.miscDyes[index].netDefaults(inventoryList[i].NetId);

                                        if (player.miscDyes[index].netID != 0)
                                        {
                                            player.miscDyes[index].stack = inventoryList[i].Stack;
                                            player.miscDyes[index].prefix = (byte)inventoryList[i].PrefixId;
                                        }
                                    }
                                    else if (i < NetItem.PiggyIndex.Item2)
                                    {
                                        //98-138
                                        var index = i - NetItem.PiggyIndex.Item1;
                                        player.bank.item[index].netDefaults(inventoryList[i].NetId);

                                        if (player.bank.item[index].netID != 0)
                                        {
                                            player.bank.item[index].stack = inventoryList[i].Stack;
                                            player.bank.item[index].prefix = (byte)inventoryList[i].PrefixId;
                                        }
                                    }
                                    else if (i < NetItem.SafeIndex.Item2)
                                    {
                                        //138-178
                                        var index = i - NetItem.SafeIndex.Item1;
                                        player.bank2.item[index].netDefaults(inventoryList[i].NetId);

                                        if (player.bank2.item[index].netID != 0)
                                        {
                                            player.bank2.item[index].stack = inventoryList[i].Stack;
                                            player.bank2.item[index].prefix = (byte)inventoryList[i].PrefixId;
                                        }
                                    }
                                    else if (i < NetItem.TrashIndex.Item2)
                                    {
                                        //179-219
                                        var index = i - NetItem.TrashIndex.Item1;
                                        player.trashItem.netDefaults(inventoryList[i].NetId);

                                        if (player.trashItem.netID != 0)
                                        {
                                            player.trashItem.stack = inventoryList[i].Stack;
                                            player.trashItem.prefix = (byte)inventoryList[i].PrefixId;
                                        }
                                    }
                                    else if (i < NetItem.ForgeIndex.Item2)
                                    {
                                        //220
                                        var index = i - NetItem.ForgeIndex.Item1;
                                        player.bank3.item[index].netDefaults(inventoryList[i].NetId);

                                        if (player.bank3.item[index].netID != 0)
                                        {
                                            player.bank3.item[index].stack = inventoryList[i].Stack;
                                            player.bank3.item[index].Prefix((byte)inventoryList[i].PrefixId);
                                        }
                                    }
                                    else if (i < NetItem.VoidIndex.Item2)
                                    {
                                        //260
                                        var index = i - NetItem.VoidIndex.Item1;
                                        player.bank4.item[index].netDefaults(inventoryList[i].NetId);

                                        if (player.bank4.item[index].netID != 0)
                                        {
                                            player.bank4.item[index].stack = inventoryList[i].Stack;
                                            player.bank4.item[index].Prefix((byte)inventoryList[i].PrefixId);
                                        }
                                    }
                                }
                                var items = new
                                {
                                    // NetItem has an explicitly-defined cast to convert from Item
                                    // Select must be used as Cast<T> doesn't support user-defined casts
                                    inventory = player.inventory.Where(i => i.active).Select(i => (NetItem)i),
                                    equipment = player.armor.Where(i => i.active).Select(i => (NetItem)i),
                                    dyes = player.dye.Where(i => i.active).Select(i => (NetItem)i),
                                    miscEquip = player.miscEquips.Where(i => i.active).Select(i => (NetItem)i),
                                    miscDye = player.miscDyes.Where(i => i.active).Select(i => (NetItem)i),
                                    piggy = player.bank.item.Where(i => i.active).Select(i => (NetItem)i),
                                    safe = player.bank2.item.Where(i => i.active).Select(i => (NetItem)i),
                                    trash = (NetItem)player.trashItem,
                                    forge = player.bank3.item.Where(i => i.active).Select(i => (NetItem)i),
                                    vault = player.bank4.item.Where(i => i.active).Select(i => (NetItem)i),
                                };

                                int[] buffs = new int[22];

                                int health = reader.Get<int>("Health");
                                int mana = reader.Get<int>("Mana");
                                int maxHealth = reader.Get<int>("MaxHealth");
                                int maxMana = reader.Get<int>("MaxMana");

                                return new RestObject
                                {
                                    {"online" , false},
                                    {"nickname", account.Name},
                                    {"username", account.Name},
                                    {"group", account.Group},
                                    {"registered", account.Registered},
                                    {"muted", false },
                                    { "selectedItemSlot", 0 },
                                    {"position", "Player is offilne"},
                                    {"items", items},
                                    {"buffs", buffs},
                                    {"health", health},
                                    {"maxHealth", maxHealth},
                                    {"maxHealthWithBuffs", maxHealth},
                                    {"mana", mana},
                                    {"maxMana", maxMana},
                                    {"maxManaWithBuffs", maxMana},
                                    {"dead", false},
                                    {"difficulty", 0},
                                };
                            }
                            else
                            {
                                return new RestObject("400") { Error = "DB could not be read." };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.Error(ex.ToString());
                        return new RestObject("400") { Error = "Player " + name + " was not found online or saved offline." };
                    }
                }
                else
                {
                    return new RestObject("400")
                    {
                        Error = $"Player {name} was not found online or saved offline."
                    };
                }
            }
            else
            {
                return new RestObject("400")
                {
                    Error = $"Player {name} matches {found.Count} players"
                };
            }
        }

        [Description("Get information for a user.")]
        [Route("/readplayers")]
        [Permission(RestPermissions.restuserinfo)]
        [Noun("player", true, "The player to lookup", typeof(String))]
        [Token]
        private object PlayerRead(RestRequestArgs args)
        {
            var ret = PlayerFind(args.Parameters);
            if (ret is RestObject)
            {
                return ret;
            }

            TSPlayer player = (TSPlayer)ret;

            var items = new
            {
                // NetItem has an explicitly-defined cast to convert from Item
                // Select must be used as Cast<T> doesn't support user-defined casts
                inventory = player.TPlayer.inventory.Where(i => i.active).Select(i => (NetItem)i),
                equipment = player.TPlayer.armor.Where(i => i.active).Select(i => (NetItem)i),
                dyes = player.TPlayer.dye.Where(i => i.active).Select(i => (NetItem)i),
                miscEquip = player.TPlayer.miscEquips.Where(i => i.active).Select(i => (NetItem)i),
                miscDye = player.TPlayer.miscDyes.Where(i => i.active).Select(i => (NetItem)i),
                piggy = player.TPlayer.bank.item.Where(i => i.active).Select(i => (NetItem)i),
                safe = player.TPlayer.bank2.item.Where(i => i.active).Select(i => (NetItem)i),
                trash = (NetItem)player.TPlayer.trashItem,
                forge = player.TPlayer.bank3.item.Where(i => i.active).Select(i => (NetItem)i),
                vault = player.TPlayer.bank4.item.Where(i => i.active).Select(i => (NetItem)i),
            };

            return new RestObject
            {
                {"online" , true},
                {"nickname", player.Name},
                {"username", player.Account?.Name},
                {"ip", player.IP},
                {"group", player.Group.Name},
                {"registered", player.Account?.Registered},
                {"muted", player.mute },
                {"position", player.TileX + "," + player.TileY},
                { "selectedItemSlot", player.TPlayer.selectedItem },
                {"items", items},
                {"buffs", player.TPlayer.buffType},
                {"health", player.TPlayer.statLife},
                {"maxHealth", player.TPlayer.statLifeMax},
                {"maxHealthWithBuffs", player.TPlayer.statLifeMax2},
                {"mana", player.TPlayer.statMana},
                {"maxMana", player.TPlayer.statManaMax},
                {"maxManaWithBuffs", player.TPlayer.statManaMax2},
                {"dead", player.TPlayer.dead},
                {"difficulty", player.TPlayer.difficulty},
            };
        }

        [Description("Gets an enhanced player list")]
        [Route("/darkgaming/v1/players")]
        [Permission(RestPermissions.restuserinfo)]
        [Token]
        private object PlayerList(RestRequestArgs args)
        {
            var canViewIps = !string.IsNullOrEmpty(args.TokenData.UserGroupName) && TShock.Groups.GetGroupByName(args.TokenData.UserGroupName).HasPermission(RestPermissions.viewips);
            var players = new ArrayList();

            foreach (var tsPlayer in TShock.Players.Where(p => p != null))
            {
                var entry = new Dictionary<string, object>
                {
                    { "nickname", tsPlayer.Name },
                    { "username", tsPlayer.Account == null ? "": tsPlayer.Account.Name },
                    { "group", tsPlayer.Group.Name },
                    { "active", tsPlayer.Active },
                    { "state", tsPlayer.State },
                    { "team", tsPlayer.Team },
                    { "ip", canViewIps ? tsPlayer.IP : "" },
                    { "position", tsPlayer.TileX + "," + tsPlayer.TileY },
                    { "hotbar", tsPlayer.TPlayer.inventory.Take(10).Select(i => (NetItem)i) },
                    { "mouseItem", (NetItem)tsPlayer.TPlayer.inventory[58] },
                    { "headItem", (NetItem) (tsPlayer.TPlayer.armor[10].type > 0 ? tsPlayer.TPlayer.armor[10] : tsPlayer.TPlayer.armor[0] ) },
                    { "selectedItemSlot", tsPlayer.TPlayer.selectedItem },
                    { "buffs", tsPlayer.TPlayer.buffType },
                    { "health", tsPlayer.TPlayer.statLife },
                    { "maxHealth", tsPlayer.TPlayer.statLifeMax },
                    { "maxHealthWithBuffs", tsPlayer.TPlayer.statLifeMax2 },
                    { "mana", tsPlayer.TPlayer.statMana },
                    { "maxMana", tsPlayer.TPlayer.statManaMax },
                    { "maxManaWithBuffs", tsPlayer.TPlayer.statManaMax2 },
                    { "dead", tsPlayer.TPlayer.dead },
                    { "difficulty", tsPlayer.TPlayer.difficulty },
                    { "registered", tsPlayer.Account?.Registered },
                };
                players.Add(entry);
            }


            return new RestObject
            {
                {"players" , players},
            };
        }
    }
}
