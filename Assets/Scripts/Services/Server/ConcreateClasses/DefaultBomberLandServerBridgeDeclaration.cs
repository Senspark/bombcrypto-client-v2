using System;
using System.Collections.Generic;

using Engine.Utils;

using Newtonsoft.Json;

using Server.Models;

using Services.IapAds;

using Sfs2X.Entities.Data;

namespace App.BomberLand {
    public partial class DefaultBomberLandServerBridge {
        private class SyncHeroResponse : ISyncHeroResponse {
            public IHeroDetails[] Details { get; }
            public int[] NewIds { get; }

            public SyncHeroResponse(ISFSObject data) {
                Details = HeroDetails.ParseArray(data);
                if (data.ContainsKey(SFSDefine.SFSField.NewBombers)) {
                    var newIdsArray = data.GetSFSArray(SFSDefine.SFSField.NewBombers);
                    var newIdsJson = newIdsArray.ToJson();
                    NewIds = JsonConvert.DeserializeObject<int[]>(newIdsJson);    
                } else {
                    NewIds = Array.Empty<int>();
                }
            }
        }

        private class OfferPacksResult : IOfferPacksResult {
            public List<IOfferPacksResult.IOffer> Offers { get; }

            public OfferPacksResult(ISFSObject data) {
                Offers = new List<IOfferPacksResult.IOffer>();
                var arr = data.GetSFSArray("data");
                for (var i = 0; i < arr.Count; i++) {
                    var pack = new Offer(arr.GetSFSObject(i));
                    Offers.Add(pack);
                }
            }

            public class Offer : IOfferPacksResult.IOffer {
                public IOfferPacksResult.OfferType Type { get; }
                public string Name { get; }
                public DateTime SaleEnd { get; }
                public bool WillRemoveAds { get; }
                public List<IOfferPacksResult.IItem> Items { get; }
                public bool IsExpired => (SaleEnd - DateTime.Now).TotalSeconds <= 0;

                public Offer(ISFSObject data) {
                    var secondsEpoch = data.GetLong("sale_end_date");
                    SaleEnd = Epoch.FromEpochMillisecondsToLocal(secondsEpoch * 1000);
                    Name = data.GetUtfString("product_id");
                    Type = Name switch {
                        IapConfig.STARTER_OFFER => IOfferPacksResult.OfferType.Starter,
                        IapConfig.HERO_OFFER => IOfferPacksResult.OfferType.Hero,
                        IapConfig.PREMIUM_OFFER => IOfferPacksResult.OfferType.Premium,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    WillRemoveAds = data.GetBool("is_remove_ads");
                    Items = new List<IOfferPacksResult.IItem>();
                    var arr = data.GetSFSArray("items");
                    for (var i = 0; i < arr.Count; i++) {
                        var item = new Item(arr.GetSFSObject(i));
                        Items.Add(item);
                    }
                }
            }

            public class Item : IOfferPacksResult.IItem {
                public int ItemId { get; }
                public int ItemQuantity { get; }
                public TimeSpan ExpiresAfter { get; }
                public bool IsNeverExpired => ExpiresAfter == TimeSpan.Zero;

                public Item(ISFSObject data) {
                    ItemId = data.GetInt("item_id");
                    ItemQuantity = data.GetInt("quantity");
                    var expires = data.GetLong("expiration_after");
                    ExpiresAfter = TimeSpan.FromMilliseconds(expires);
                }
            }
        }
    }
}