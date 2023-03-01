using LeagueBuddy.League;
using LeagueBuddy.League.DTOs;
using LeagueBuddy.Main.DataCsses;
using Newtonsoft.Json;
using Swan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace LeagueBuddy.Main {
    internal class ItemsetRoot {
        [JsonIgnore]
        private static readonly string localSetPath = Path.Combine(Settings.DataDir, "itemsets.json");
        [JsonIgnore]
        private static readonly string localPushedPath = Path.Combine(Settings.DataDir, "pushed.json");
        [JsonIgnore]
        private static readonly string localToRemovePath = Path.Combine(Settings.DataDir, "toRemove.json");
        [JsonIgnore]
        private static readonly string clientSetPath = Path.Combine(Utils.GetLeagueClientPath(), "Config", "ItemSets.json");
        public long accountId { get; set; }
        public List<Itemset> itemSets { get; set; }
        public long timestamp { get; set; }

        [JsonConstructor]
        private ItemsetRoot(long accountId, List<Itemset> itemSets, long timestamp) {
            this.accountId = accountId;
            this.itemSets = itemSets;
            this.timestamp = timestamp;
        }

        private ItemsetRoot(long accountId) {
            this.accountId = accountId;
            this.itemSets = new List<Itemset>();
            this.timestamp = 0;
        }

        public async static void UpdateLocalSetsAndPushToClientIfEnabled() {
            //make thread async
            await Task.Delay(1);

            var setsToRemove = loadSetsToRemove();
            var setsPushedToOtherSummoners = loadPushedSets();
            var clientRoot = loadSetsFromClient();
            //preventing zombiesets that have been pushed to prevent removal
            bool removedAny = false;
            foreach (var kvp in setsToRemove) {
                foreach (var toRemove in kvp.Value) {
                    Itemset removeAfter = null;
                    foreach (var itemset in clientRoot.itemSets) {
                        if (itemset.uid != toRemove) continue;
                        removeAfter = itemset;
                        break;
                    }
                    if (removeAfter == null) continue;
                    clientRoot.itemSets.Remove(removeAfter);
                    setsPushedToOtherSummoners[LcuHandler.CurrentSummonerId].Remove(toRemove);
                    removedAny = true;
                }
            }
            //if all pushed sets have been removed delete uid from list
            if (removedAny) {
                //overwriting sets this way will only update on second start
                clientRoot.saveSetsToClient();
                foreach (var removeKvp in setsToRemove) {
                    var canBeRemoved = new List<string>();
                    foreach (var needsRemoval in removeKvp.Value) {
                        var removedAll = true;
                        foreach (var kvp in setsPushedToOtherSummoners) {
                            if (!kvp.Value.Contains(needsRemoval)) continue;
                            removedAll = false;
                            break;
                        }
                        if (!removedAll) continue;
                        canBeRemoved.Add(needsRemoval);
                    }
                    //backwards to prevent out of bounds
                    for (int i = removeKvp.Value.Count - 1; i >= 0; i--) {
                        removeKvp.Value.RemoveAt(i);
                    }
                }
            }


            var localRoots = loadSets();
            if (clientRoot != null) {
                //if no sets are saved locally insert current 
                if (localRoots == null) {
                    localRoots = new List<ItemsetRoot> { clientRoot };
                } else {
                    //updating locally saved root
                    var clientSets = clientRoot?.itemSets;
                    if (clientSets != null) {
                        var foundSelfRoot = false;
                        foreach (var clientSet in clientSets) {
                            bool setFoundInOtherRoot = false;
                            //check if itemset has been inserted from local itemsets
                            foreach (var localRoot in localRoots) {
                                if (localRoot.accountId != clientRoot.accountId) {
                                    if (localRoot.itemSets.Contains(clientSet)) {
                                        setFoundInOtherRoot = true;
                                        if (foundSelfRoot) break;
                                    }
                                } else {
                                    foundSelfRoot = true;
                                }
                            }
                            //if itemset has been inserted from local itemsets, we dont have to add it to our local[current] collection. Its already in local[otherXYZ] collection
                            if (setFoundInOtherRoot) continue;
                            if (!foundSelfRoot) {
                                var newRoot = new ItemsetRoot(clientRoot.accountId);
                                foundSelfRoot = true;
                            }
                            //appending set if not already in set (reverse iteration so its quicker if we just appended new root)
                            for (int i = localRoots.Count - 1; i >= 0; i--) {
                                if (localRoots[i].accountId != clientRoot.accountId) continue;
                                if (localRoots[i].itemSets.Contains(clientSet)) continue;
                                localRoots[i].itemSets.Add(clientSet);
                            }
                        }
                        //removing sets that arent saved in the client anymore from local version of the current root (reverse iteration so its quicker if we just appended new root)
                        for (int i = localRoots.Count - 1; i >= 0; i--) {
                            if (localRoots[i].accountId != clientRoot.accountId) continue;
                            var toRemove = new List<Itemset>();
                            foreach (var localSet in localRoots[i].itemSets) {
                                if (clientSets.Contains(localSet)) continue;
                                toRemove.Add(localSet);
                            }
                            //remove backwards so indices arent out of bounce
                            for (int rI = toRemove.Count - 1; rI >= 0; rI--) {
                                if (setsToRemove.ContainsKey(LcuHandler.CurrentSummonerId)) {
                                    setsToRemove[LcuHandler.CurrentSummonerId].Add(toRemove[rI].uid);
                                } else {
                                    setsToRemove.Add(LcuHandler.CurrentSummonerId, new List<string> { toRemove[rI].uid });
                                }
                                localRoots[i].itemSets.Remove(toRemove[rI]);
                            }

                            /*
                            var removeIndices = new List<int>();
                            foreach (var localSet in localRoots[i].itemSets) {
                                if (clientSets.Contains(localSet)) continue;
                                removeIndices.Add(i);
                            }
                            //remove backwards so indices arent out of bounce
                            for (int rI = removeIndices.Count-1; rI >= 0; rI--) {
                                localRoots[i].itemSets.RemoveAt(removeIndices[rI]);
                            }
                            */
                        }
                    }
                    if (Settings.current.IsItemsetRewritingEnabled) {
                        var pushedSets = await PushMissingSetsToClient(clientRoot, localRoots);
                        if (pushedSets.Count != 0) {
                            MainController.enqueueMessage($"Pushed {pushedSets.Count} itemsets to the client.");
                            if (setsPushedToOtherSummoners.ContainsKey(LcuHandler.CurrentSummonerId)) {
                                foreach (var pushedSet in pushedSets) {
                                    setsPushedToOtherSummoners[LcuHandler.CurrentSummonerId].Add(pushedSet.uid);
                                }
                            } else {
                                setsPushedToOtherSummoners.Add(LcuHandler.CurrentSummonerId, pushedSets.Select(x => x.uid).ToList());
                            }
                        }
                    }
                }
            } else {
                if (Settings.current.IsItemsetRewritingEnabled) {
                    clientRoot = new ItemsetRoot(LcuHandler.CurrentSummonerId);
                    var pushedSets = await PushMissingSetsToClient(clientRoot, localRoots);
                    if (pushedSets.Count != 0) {
                        MainController.enqueueMessage($"Pushed {pushedSets.Count} itemsets to the client.");
                        if (setsPushedToOtherSummoners.ContainsKey(LcuHandler.CurrentSummonerId)) {
                            foreach (var pushedSet in pushedSets) {
                                setsPushedToOtherSummoners[LcuHandler.CurrentSummonerId].Add(pushedSet.uid);
                            }
                        } else {
                            setsPushedToOtherSummoners.Add(LcuHandler.CurrentSummonerId, pushedSets.Select(x => x.uid).ToList());
                        }
                    }
                }
            }
            saveSets(localRoots);
            savePushedSets(setsPushedToOtherSummoners);
            saveToRemoveSets(setsToRemove);
        }

        private static Dictionary<long, List<string>> loadSetsToRemove() {
            if (!File.Exists(localToRemovePath)) return new Dictionary<long, List<string>>();
            try {
                return JsonConvert.DeserializeObject<Dictionary<long, List<string>>>(File.ReadAllText(localToRemovePath));
            } catch { return new Dictionary<long, List<string>>(); }
        }

        private static Dictionary<long, List<string>> loadPushedSets() {
            if (!File.Exists(localPushedPath)) return new Dictionary<long, List<string>>();
            try {
                return JsonConvert.DeserializeObject<Dictionary<long, List<string>>>(File.ReadAllText(localPushedPath));
            } catch { return new Dictionary<long, List<string>>(); }
        }

        private static async Task<List<Itemset>> PushMissingSetsToClient(ItemsetRoot clientRoot, List<ItemsetRoot> localRoots) {
            var itemSetsPushed = new List<Itemset>();
            foreach (var localRoot in localRoots) {
                var toPush = new ItemsetRoot(localRoot.accountId);
                foreach (var itemset in localRoot.itemSets) {
                    if (clientRoot.itemSets.Contains(itemset) || toPush.itemSets.Contains(itemset)) continue;
                    toPush.itemSets.Add(itemset);
                }
                if (toPush.itemSets.Count == 0) continue;
                var payload = JsonConvert.SerializeObject(toPush);
                await LcuHandler.PushItemsetToClientAsync(payload);
                itemSetsPushed.AddRange(toPush.itemSets);
            }
            return itemSetsPushed;
        }
        private static List<ItemsetRoot> loadSets() {
            if (!File.Exists(localSetPath)) return null;
            try {
                return JsonConvert.DeserializeObject<List<ItemsetRoot>>(File.ReadAllText(localSetPath));
            } catch {
                return null;
            }
        }
        private static ItemsetRoot loadSetsFromClient() {
            if (!File.Exists(clientSetPath)) return null;
            try {
                return JsonConvert.DeserializeObject<ItemsetRoot>(File.ReadAllText(clientSetPath));
            } catch {
                return null;
            }
        }
        private static bool saveSets(List<ItemsetRoot> localRoots) {
            try {
                File.WriteAllText(localSetPath, JsonConvert.SerializeObject(localRoots));
                return true;
            } catch { return false; }
        }
        private bool saveSetsToClient() {
            try {
                File.WriteAllText(clientSetPath, JsonConvert.SerializeObject(this));
                return true;
            } catch { return false; }
        }
        private static bool saveToRemoveSets(Dictionary<long, List<string>> dict) {
            try {
                File.WriteAllText(localToRemovePath, JsonConvert.SerializeObject(dict));
                return true;
            } catch { return false; }
        }
        private static bool savePushedSets(Dictionary<long, List<string>> dict) {
            try {
                File.WriteAllText(localPushedPath, JsonConvert.SerializeObject(dict));
                return true;
            } catch { return false; }
        }
    }

    public class Itemset {
        public int[] associatedChampions { get; set; }
        public int?[] associatedMaps { get; set; }
        public Block[] blocks { get; set; }
        public string map { get; set; }
        public string mode { get; set; }
        public Preferreditemslot[] preferredItemSlots { get; set; }
        public int sortrank { get; set; }
        public string startedFrom { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string uid { get; set; }

        public override bool Equals(object? other) {
            if (other == null) return false;
            if (other == this) return true;
            if (other.GetType() != typeof(Itemset)) return false;
            var otherSet = (Itemset)other;
            if (title != otherSet.title) return false;
            if (map != otherSet.map) return false;
            if (mode != otherSet.mode) return false;
            if (sortrank != otherSet.sortrank) return false;
            if (startedFrom != otherSet.startedFrom) return false;
            if (type != otherSet.type) return false;
            if (!associatedChampions.SequenceEqual(otherSet.associatedChampions)) return false;
            if (!preferredItemSlots.SequenceEqual(otherSet.preferredItemSlots)) return false;
            if (!associatedMaps.SequenceEqual(otherSet.associatedMaps)) return false;
            if (!blocks.SequenceEqual(otherSet.blocks)) return false;
            return true;
        }
    }

    public class Block {
        public string hideIfSummonerSpell { get; set; }
        public Item[] items { get; set; }
        public string showIfSummonerSpell { get; set; }
        public string type { get; set; }
        public override bool Equals(object other) {
            if (other == null) return false;
            if (other == this) return false;
            if (other.GetType() != this.GetType()) return false;
            var otherBlock = (Block)other;
            if (hideIfSummonerSpell != otherBlock.hideIfSummonerSpell) return false;
            if (showIfSummonerSpell != otherBlock.showIfSummonerSpell) return false;
            if (type != otherBlock.type) return false;
            if (!items.SequenceEqual(otherBlock.items)) return false;
            return true;
        }
    }

    public class Item {
        public int count { get; set; }
        public string id { get; set; }
        public override bool Equals(object? other) {
            if (other == null) return false;
            if (other == this) return false;
            if (other.GetType() != this.GetType()) return false;
            return id == ((Item)other).id;
        }
    }

    public class Preferreditemslot {
        public string id { get; set; }
        public int preferredItemSlot { get; set; }
        public override bool Equals(object? other) {
            if (other == null) return false;
            if (other == this) return true;
            if (other.GetType() != this.GetType()) return false;
            var otherSlot = (Preferreditemslot)other;
            return id == otherSlot.id && preferredItemSlot == otherSlot.preferredItemSlot;
        }
    }


}
