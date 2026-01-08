using System.Collections.Generic;
using Engine.Entities;

using Newtonsoft.Json;

namespace Engine.Strategy.Provider
{
    public class ItemCreator
    {
        public EntityType Type { get; }
        public string Name { get; }
        public float Hp { get; }
        public float Token { get; }
        public int[] MinD { get; }
        public int[] Random { get; }

        [JsonConstructor]
        public ItemCreator(string type,
                          string name,
                          float hp,
                          float token,
                          int[] min_D,
                          int[] random)
        {
            Name = name;
            Hp = hp;
            Token = token;
            MinD = min_D;
            Random = random;

            var dictionary = new Dictionary<string, EntityType> {
                { "normal", EntityType.normalBlock },
                { "jail_house", EntityType.jailHouse },
                { "wooden_chest", EntityType.woodenChest },
                { "silver_chest", EntityType.silverChest },
                { "golden_chest", EntityType.goldenChest },
                { "diamond_chest", EntityType.diamondChest },
                { "legend_chest", EntityType.legendChest },
                { "key_chest", EntityType.keyChest },
                { "bcoin_diamond_chest", EntityType.BcoinDiamondChest }

            };

            Type = dictionary[type];

        }

        public int GetRandomByD(int d)
        {
            for (var i = 0; i < MinD.Length; i++)
            {
                var minD = MinD[i];
                if (d <= minD)
                {
                    return Random[i];
                }
            }

            return Random[Random.Length - 1];
        }
    }

}
