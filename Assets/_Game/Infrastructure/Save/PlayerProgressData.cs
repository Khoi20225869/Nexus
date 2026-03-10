using System;
using System.Collections.Generic;

namespace Game.Infrastructure.Save
{
    [Serializable]
    public sealed class InventoryEntryData
    {
        public string ItemId;
        public int Amount;
    }

    [Serializable]
    public sealed class PlayerProgressData
    {
        public int Gold;
        public string ActiveCharacterId;
        public string CurrentLevelId;
        public List<string> UnlockedCharacterIds = new();
        public List<string> UnlockedLevelIds = new();
        public List<InventoryEntryData> InventoryEntries = new();
    }
}
