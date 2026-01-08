using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using App;

using Senspark;

using UnityEngine;

namespace Analytics {
    public class BaseFirebaseAnalytics : IAnalytics {
        private static readonly string KeyTrackedEvent = $"{nameof(BaseFirebaseAnalytics)}_tracked";
        private static readonly string KeyActiveUserEvent = $"{nameof(BaseFirebaseAnalytics)}_active_user";

        private readonly Dictionary<ConversionType, string> _tags = new() {
            {ConversionType.ConnectWalletBnb, "conversion_connect_wallet_bnb"},
            {ConversionType.ConnectWalletPolygon, "conversion_connect_wallet_polygon"},
            {ConversionType.ConnectAccount, "conversion_connect_account"},
            {ConversionType.ConnectAccountBnb, "conversion_connect_account_bnb"},
            {ConversionType.ConnectAccountPolygon, "conversion_connect_account_polygon"},
            {ConversionType.ConnectGuest, "conversion_guest"},
            {ConversionType.ConnectFacebook, "conversion_connect_facebook"},
            {ConversionType.ConnectApple, "conversion_connect_apple"},
            {ConversionType.ConnectTelegram, "conversion_connect_telegram"},
            {ConversionType.ConnectSolana, "conversion_connect_solana"},
            {ConversionType.ChooseBnb, "conversion_choose_bnb"},
            {ConversionType.ChoosePolygon, "conversion_choose_polygon"},
            {ConversionType.NewUser, "conversion_nru"},
            {ConversionType.FirstOpen, "conversion_first_open"},
            {ConversionType.BuyHeroFi, "conversion_buy_hero"},
            {ConversionType.BuyHeroFiFail, "conversion_buy_hero_fail"},
            {ConversionType.BuyHouse, "conversion_buy_house"},
            {ConversionType.BuyHouseFail, "conversion_buy_house_fail"},
            {ConversionType.BuyIap, "conversion_iap"},
            {ConversionType.BuyIapFail, "conversion_iap_fail"},
            {ConversionType.BuySoftCurrency, "conversion_soft_currency"},
            {ConversionType.BuySoftCurrencyFail, "conversion_soft_currency_fail"},
            {ConversionType.BuyGachaChest, "conversion_buy_gacha_chest"},
            {ConversionType.BuyGachaChestFail, "conversion_buy_gacha_chest_fail"},
            {ConversionType.OpenChestUseGem, "conversion_open_chest_use_gem"},
            {ConversionType.OpenChestUseGemFail, "conversion_open_chest_use_gem_fail"},
            {ConversionType.OpenChestWatchAds, "conversion_open_chest_watch_ads"},
            {ConversionType.OpenChest, "conversion_open_chest"},
            {ConversionType.BuyMarketHeroTr, "conversion_buy_hero_market"},
            {ConversionType.BuyMarketHeroTrFail, "conversion_buy_hero_market_fail"},
            {ConversionType.BuyMarketItem, "conversion_buy_item_market"},
            {ConversionType.BuyMarketItemFail, "conversion_buy_item_market_fail"},
            {ConversionType.BuyHeroTrFail, "conversion_buy_hero_fail"},
            {ConversionType.GetHeroTrFree, "conversion_get_hero_free"},
            {ConversionType.X2GoldPveWatchAds, "conversion_x2_gold_pve_watch_ads"},
            {ConversionType.UsedBoosterBombPve, "conversion_bomb_pve"},
            {ConversionType.UsedBoosterRangePve, "conversion_range_pve"},
            {ConversionType.UsedBoosterSpeedPve, "conversion_speed_pve"},
            {ConversionType.UsedBoosterShieldPve, "conversion_shield_pve"},
            {ConversionType.X2GoldPvpWatchAds, "conversion_x2_gold_pvp_watch_ads"},
            {ConversionType.UsedBoosterBombPvp, "conversion_bomb_pvp"},
            {ConversionType.UsedBoosterRangePvp, "conversion_range_pvp"},
            {ConversionType.UsedBoosterSpeedPvp, "conversion_speed_pvp"},
            {ConversionType.UsedBoosterKeyPvp, "conversion_key_pvp"},
            {ConversionType.UsedBoosterShieldPvp, "conversion_shield_pvp"},
            {ConversionType.UsedBoosterFullConquestCardPvp, "conversion_full_cup_card_pvp"},
            {ConversionType.UsedBoosterFullRankGuardianPvp, "conversion_full_rank_gua_pvp"},
            {ConversionType.UsedBoosterConquestCardPvp, "conversion_cup_card_pvp"},
            {ConversionType.UsedBoosterRankGuardianPvp, "conversion_rank_gua_pvp"},
            {ConversionType.ClickSwapGem, null},
            {ConversionType.SwapGem, null},
            {ConversionType.UseAutoMine, "conversion_use_auto_mine"},
            {ConversionType.UserFiClickMarket, "conversion_userfi_click_market"},
            {ConversionType.UserTrClickMarket, "conversion_usertr_click_market"},
            {ConversionType.PlayAdventure, "conversion_play_pve"},
            {ConversionType.UseUpBoosterPvp, "conversion_used_up_booster_pvp"},
            {ConversionType.UseUpBoosterAdventure, "conversion_used_up_booster_pve"},
            {ConversionType.ClickActiveChest, "track_click_active_chest"},
            {ConversionType.WatchAds, "conversion_watch_ads"},
            {ConversionType.RevivePveByAds, "conversion_revive_by_ads"},
            {ConversionType.RevivePveByGem, "conversion_revive_by_gem"},
            {ConversionType.LuckyWheelPveWatchAds, "conversion_lucky_wheel_pve_watch_ads"},
            {ConversionType.LuckyWheelPvpWatchAds, "conversion_lucky_wheel_pvp_watch_ads"},
            {ConversionType.OverwriteAccount, "conversion_overwrite_account"},
            {ConversionType.VisitGemShop, "conversion_visit_gem_shop"},
            {ConversionType.ShowStarterPack, "conversion_show_starter_pack"},
            {ConversionType.TapBtnBuyStarterPack, "conversion_tap_button_in_starter_pack"},
            {ConversionType.ShowPremiumPack, "conversion_show_premium_pack"},
            {ConversionType.TapBtnBuyPremiumPack, "conversion_tap_button_in_premium_pack"},
            {ConversionType.ShowHeroPack, "conversion_show_hero_pack"},
            {ConversionType.TapBtnBuyHeroPack, "conversion_tap_button_in_hero_pack"},
        };

        private readonly Dictionary<SceneType, string> _sceneTags = new() {
            {SceneType.VisitMarketplace, "scene_marketplace"},
            {SceneType.VisitRanking, "scene_ranking"},
            {SceneType.VisitAdventure, "scene_pve"},
            {SceneType.VisitPvp, "scene_pvp"},
            {SceneType.VisitTreasureHunt, "scene_treasure_hunt"},
            {SceneType.VisitShop, "scene_shop"},
            {SceneType.VisitChest, "scene_chest"},
            {SceneType.VisitDailyGift, "scene_daily_gift"},

            {SceneType.VisitHome, "scene_home"},
            {SceneType.PlayPve, "scene_play_pve"},
            {SceneType.PlayPvp, "scene_play_pvp"},
            {SceneType.Rating, "scene_rating"},

            {SceneType.WelcomeTutorial, "scene_welcome_tutorial"},
            {SceneType.SkipTutorialBot, "scene_skip_tutorial_bot"},
            {SceneType.PlayTutorialBot, "scene_play_tutorial_bot"},
            {SceneType.ControlGuideTutorial, "scene_control_guide_tutorial"},
            {SceneType.FTUEConversion, "conversion_ftue"},
            {SceneType.FullPowerTutorial, "scene_full_power_tutorial"},
            {SceneType.FeatureGuideTutorial, "scene_feature_guide_tutorial"},
            {SceneType.RewardTutorial, "scene_reward_tutorial"},
            {SceneType.ChooseLoginScene, "scene_choose_login"}
        };
        
        private readonly Dictionary<int, string> _retentionTags = new() {
            {1, "retention_D1"},
            {3, "retention_D3"},
            {5, "retention_D5"},
            {10, "retention_D10"},
        };

        private readonly Dictionary<int, string> _pvpMatchTags = new() {
            {1, "conversion_play_1_matches"},
            {2, "conversion_play_2_matches"},
            {3, "conversion_play_3_matches"},
            {4, "conversion_play_4_matches"},
            {5, "conversion_play_5_matches"},
            {10, "conversion_play_10_matches"},
            {20, "conversion_play_20_matches"},
            {30, "conversion_play_30_matches"},
        };

        private readonly Dictionary<int, string> _pvpWinMatchTags = new() {
            {20, "conversion_win_20_matches"}
        };

        private readonly Dictionary<(int level, int stage), string> _pveLevelStageTags = new() {
            {(1, 1), "conversion_complete_pve_stage1_level_1"},
            {(2, 1), "conversion_complete_pve_stage1_level_2"},
            {(3, 1), "conversion_complete_pve_stage1_level_3"},
            {(4, 1), "conversion_complete_pve_stage1_level_4"},
            {(5, 1), "conversion_complete_stage_1_pve"},
            {(5, 2), "conversion_complete_stage_2_pve"},
            {(5, 3), "conversion_complete_stage_3_pve"},
        };

        private readonly IAnalyticsBridge _bridge;
        private readonly List<string> _tracked;
        private IAnalyticsDependency _analyticsDependency;
        
        public BaseFirebaseAnalytics(
            bool production,
            ILogManager logManager
        ) {
            var enableLog = production;
            var platform = Application.platform;
            _bridge = platform switch {
                RuntimePlatform.WebGLPlayer => new WebGLFirebaseAnalyticsBridge(enableLog, AppConfig.FirebaseAppId,
                    AppConfig.FirebaseMeasurementId),
                RuntimePlatform.Android or RuntimePlatform.IPhonePlayer => new MobileFirebaseAnalyticsBridge(logManager,
                    enableLog),
                _ => new NullAnalyticsBridge(logManager, nameof(BaseFirebaseAnalytics))
            };

            _tracked = AnalyticsUtils.LoadTrackedEvent(KeyTrackedEvent);
        }

        public Task<bool> Initialize() {
            return _bridge.Initialize();
        }

        public void Destroy() { }

        public void SetDependencies(IAnalyticsDependency dependency) {
            _analyticsDependency = dependency;
        }

        #region CONVERSION

        public void TrackConversion(ConversionType type) {
#if UNITY_EDITOR
            if (!_tags.ContainsKey(type)) {
                throw new Exception($"Invalid Firebase Analytics key: {type}");
            }
#endif
            if (_tags.TryGetValue(type, out var tag)) {
                TrackConversion(tag);
            }
        }

        public void TrackConversionRetention() {
            var retentionDay = AnalyticsUtils.GetRetentionDays();
            if (_retentionTags.TryGetValue(retentionDay, out var tag)) {
                TrackConversion(tag);
            }
        }

        public void TrackConversionPvpPlay(int matchCount, int winMatch) {
            // track conversion played match
            if (_pvpMatchTags.TryGetValue(matchCount, out var tag)) {
                TrackConversion(tag);
            }
            
            // track conversion win match
            if (_pvpWinMatchTags.TryGetValue(winMatch, out var matchTag)) {
                TrackConversion(matchTag);
            }
        }

        public void TrackConversionAdventurePlay(int level, int stage) {
            var d = (level, stage);
            if (_pveLevelStageTags.TryGetValue(d, out var tag)) {
                TrackConversion(tag);
            }
        }
        
        public void TrackConversionActiveUser() {
            if (!AnalyticsUtils.CanTrackActiveUserToday(KeyActiveUserEvent)) {
                return;
            }
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
            };
            _bridge.LogEvent("active_user", parameters);
        }

        private void TrackConversion(string evName) {
            if (string.IsNullOrWhiteSpace(evName) || _tracked.Contains(evName)) {
                return;
            }
            _tracked.Add(evName);
            AnalyticsUtils.SaveTrackedEvent(KeyTrackedEvent, _tracked);
            _bridge.LogEvent(evName);
        }

        public void TrackScene(SceneType type) {
#if UNITY_EDITOR
            if (!_sceneTags.ContainsKey(type)) {
                throw new Exception($"Invalid Firebase Analytics key: {type}");
            }
#endif
            if (!_sceneTags.ContainsKey(type)) {
                return;
            }
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId)
            };
            var s = _sceneTags[type];
            _bridge.LogEvent(s, parameters);
            _bridge.LogScene(s);
        }
        
        public void TrackSceneAndSub(SceneType type, string sub) {
#if UNITY_EDITOR
            if (!_sceneTags.ContainsKey(type)) {
                throw new Exception($"Invalid Firebase Analytics key: {type}");
            }
#endif
            if (!_sceneTags.ContainsKey(type)) {
                return;
            }
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId)
            };
            _bridge.LogEvent($"{_sceneTags[type]}_{sub}", parameters);
        }

        public void TrackData(string name, Dictionary<string, object> parameters) {
            _bridge.LogEvent(name, parameters);
        }

        #endregion

        public void TrackRate(int pointRating) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("point_rating", pointRating)
            };
            _bridge.LogEvent("track_rating", parameters);
        }

        public void TrackClickPlayPvp(int heroId) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("hero_id", heroId)
            };
            _bridge.LogEvent("track_click_play_pvp", parameters);
        }

        public void TrackClickPlayPve(int heroId, int level, int stage) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("hero_id", heroId),
                new("level", level),
                new("stage", stage)
            };
            _bridge.LogEvent("track_click_play_pve", parameters);
        }

        public void TrackOfflineReward(int timeOffline, string type, string[] names, string[] values) {
            var userId = _analyticsDependency.GetUserId();
            var rewardName = string.Join("-", names);
            var rewardAmount = string.Join("-", values);
            Parameter[] parameters = {
                new("user_id", userId),
                new("time_offline", timeOffline),
                new("type", type),
                new("reward_name", rewardName),
                new("reward_amount", rewardAmount)
            };
            _bridge.LogEvent("track_offline_reward", parameters);
        }

        public void TrackClaimRewardTutorial() {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId)
            };
            _bridge.LogEvent("track_claim_reward_tutorial", parameters);
        }

        public void TrackGetHeroTrFree(string heroName, int heroId) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("hero_status", "hero_free"),
                new("hero_name", heroName),
                new("hero_id", heroId),
                new("hero_amount", 1),
            };
            _bridge.LogEvent("track_hero", parameters);
        }

        public void TreasureHunt_TrackCompleteMap() {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("game_mode", "treasure_hunt"),
            };
            _bridge.LogEvent("track_complete_map", parameters);
        }

        public void TreasureHunt_TrackPlayingTime(float seconds) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("game_mode", "treasure_hunt"),
                new("playing_time", seconds),
            };
            _bridge.LogEvent("track_playing_time", parameters);
        }

        public void Iap_TrackBuyIap(string transactionId, string productId, int value, TrackResult result) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("id", transactionId ?? string.Empty),
                new("sink_type", "sink_money"),
                new("source_type", "source_gem_lock"),
                new("iap_name", productId),
                new("value", value),
                new("reason", productId),
                new("result", TrackResultToString(result)),
            };
            _bridge.LogEvent("track_iap", parameters);
        }

        public void Iap_TrackBuyGold(int productId, int gemLockSpending, int gemSpending, int goldReceiving, TrackResult result) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", "sink_gem_lock"),
                new("source_type", "source_gold"),
                new("sink_value", $"{gemLockSpending}-{gemSpending}"),
                new("source_value", goldReceiving),
                new("reason", $"buy_{productId}_gold"),
                new("result", TrackResultToString(result)),
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }
        public void Iap_TrackGetGoldFree(string transactionId, string productId, int value, TrackResult result) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("id", transactionId ?? string.Empty),
                new("sink_type", "sink_ads"),
                new("source_type", "source_gold"),
                new("sink_value", 1),
                new("source_value", value),
                new("reason", $"buy_{productId}_gem"),
                new("result", TrackResultToString(result)),
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }
        public void Iap_TrackBuyGachaChest(int productId, string productName, string sinkType, float coinSpending, int chestReceiving,
            string[] items, string[] values, TrackResult result) {
            var userId = _analyticsDependency.GetUserId();
            var itemType = string.Join("-", items);
            var itemAmount = string.Join("-", values);
            Parameter[] parameters = {
                new("user_id", userId),
                new("id", productId),
                new("sink_type", sinkType),
                new("source_type", "source_gacha_chest"),
                new("chest_name", productName),
                new("sink_value", coinSpending),
                new("source_value", chestReceiving),
                new("item_type", itemType),
                new("item_amount", itemAmount),
                new("reason", $"buy_{productId}"),
                new("result", TrackResultToString(result)),
            };
            _bridge.LogEvent("buy_gacha_chest", parameters);
        }

        public void Iap_TrackSoftCurrencyBuyGachaChest(string sinkType, float coinSpending, TrackResult result) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", sinkType),
                new("source_type", "source_open_chest"),
                new("sink_value", coinSpending),
                new("source_value", 1),
                new("reason", "use_coin_to_open_the_chest"),
                new("result", TrackResultToString(result)),
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }

        public void Iap_TrackSoftCurrencyOpenGachaChestByGem(int gemLockSpending, int gemSpending, TrackResult result) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", "sink_gem"),
                new("source_type", "source_open_chest"),
                new("sink_value", $"{gemLockSpending}-{gemSpending}"),
                new("source_value", 1),
                new("reason", "use_gem_to_open_the_chest"),
                new("result", TrackResultToString(result)),
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }

        public void Iap_TrackSoftCurrencyBuySlotChestByGem(int gemLockSpending, int gemSpending, TrackResult result) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", "sink_gem"),
                new("source_type", "source_slot_chest"),
                new("sink_value", $"{gemLockSpending}-{gemSpending}"),
                new("source_value", 1),
                new("reason", "use_gem_to_buy_slot_chest"),
                new("result", TrackResultToString(result)),
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }
        
        public void Iap_TrackOpenSwapGem() {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
            };
            _bridge.LogEvent("track_click_swap_gem", parameters);
        }

        public void Iap_TrackSwapGem(float gemSpending, float tokenReceiving, TrackResult result) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", "sink_gem_unlock"),
                new("source_type", "source_sen"),
                new("sink_value", gemSpending),
                new("source_value", tokenReceiving),
                new("reason", "swap_token"),
                new("result", TrackResultToString(result)),
            };
            _bridge.LogEvent("track_swap_gem", parameters);
        }

        public void Pvp_TrackActiveBooster(string boosterName) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("booster_name", boosterName),
                new("booster_result", "use_pvp"),
                new("value", 1),
            };
            _bridge.LogEvent("track_pvp_booster", parameters);
        }

        public void Pvp_TrackPlay(int winUserId, int loseUserId, int totalTime, TrackPvpMatchResult result,
            TrackPvpLoseReason reason) {
            var matchResult = result switch {
                TrackPvpMatchResult.Win => "complete",
                TrackPvpMatchResult.Lose => "complete",
                TrackPvpMatchResult.Draw => "draw",
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
            var loseResult = reason switch {
                TrackPvpLoseReason.BlockDrop => "block_drop",
                TrackPvpLoseReason.Prison => "prison",
                TrackPvpLoseReason.Quit => "quit",
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
            };
            Parameter[] parameters = {
                new("user_id_win", winUserId),
                new("user_id_lose", loseUserId),
                new("match_result", matchResult),
                new("reason_lose", loseResult),
                new("total_time", totalTime),
            };
            _bridge.LogEvent("track_play_pvp", parameters);
        }

        public void Pvp_TrackCollectItems(Dictionary<TrackPvpCollectItemType, int> collected,
            TrackPvpMatchResult result) {
            var userId = _analyticsDependency.GetUserId();
            var matchResult = result switch {
                TrackPvpMatchResult.Win => "win",
                TrackPvpMatchResult.Lose => "lose",
                TrackPvpMatchResult.Draw => "draw",
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
            var array = (TrackPvpCollectItemType[]) Enum.GetValues(typeof(TrackPvpCollectItemType));
            foreach (var it in array) {
                collected.TryAdd(it, 0);
            }
            Parameter[] parameters = {
                new("user_id", userId),
                new("skull_head", collected[TrackPvpCollectItemType.SkullHead]),
                new("power", collected[TrackPvpCollectItemType.FireUp]),
                new("boots", collected[TrackPvpCollectItemType.Boots]),
                new("count", collected[TrackPvpCollectItemType.BombUp]),
                new("shield", collected[TrackPvpCollectItemType.Armor]),
                new("gold_1", collected[TrackPvpCollectItemType.Gold1]),
                new("gold_5", collected[TrackPvpCollectItemType.Gold5]),
                new("silver_chest", collected[TrackPvpCollectItemType.SilverChest]),
                new("bronze_chest", collected[TrackPvpCollectItemType.BronzeChest]),
                new("match_result", matchResult),
            };
            _bridge.LogEvent("track_collect_item_pvp", parameters);
        }

        public void Pvp_TrackFullInventory(Dictionary<TrackPvpRejectChest, int> rejected) {
            var userId = _analyticsDependency.GetUserId();
            var array = (TrackPvpRejectChest[]) Enum.GetValues(typeof(TrackPvpRejectChest));
            foreach (var it in array) {
                rejected.TryAdd(it, 0);
            }
            Parameter[] parameters = {
                new("user_id", userId),
                new ("mode", "pvp"),
                new("silver_chest", rejected[TrackPvpRejectChest.SilverChest]),
                new("bronze_chest", rejected[TrackPvpRejectChest.BronzeChest]),
            };
            _bridge.LogEvent("track_full_inventory", parameters);
        }

        public void Pve_TrackFullInventory(Dictionary<TrackPvpRejectChest, int> rejected) {
            var userId = _analyticsDependency.GetUserId();
            var array = (TrackPvpRejectChest[]) Enum.GetValues(typeof(TrackPvpRejectChest));
            foreach (var it in array) {
                rejected.TryAdd(it, 0);
            }
            Parameter[] parameters = {
                new("user_id", userId),
                new ("mode", "pve"),
                new("silver_chest", rejected[TrackPvpRejectChest.SilverChest]),
                new("bronze_chest", rejected[TrackPvpRejectChest.BronzeChest]),
            };
            _bridge.LogEvent("track_full_inventory", parameters);
        }
        
        public void Pvp_TrackInPrison(int amount) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("value", amount),
            };
            _bridge.LogEvent("track_prison", parameters);
        }

        public void Pvp_TrackSoftCurrencyByWin(float value) {
            var userId = _analyticsDependency.GetUserId();
            var reason = $"win_pvp";
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", "sink_pvp"),
                new("source_type", "source_gold"),
                new("sink_value", 1),
                new("source_value", value),
                new("reason", reason)
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }

        public void Pvp_TrackSoftCurrencyByX2Gold(float value) {
            var userId = _analyticsDependency.GetUserId();
            var reason = $"x2_gold_pvp";
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", "sink_pvp_ad"),
                new("source_type", "source_gold"),
                new("sink_value", 1),
                new("source_value", value),
                new("reason", reason)
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }

        public void MarketPlace_TrackProduct(string productName, float price, int amount, MarketPlaceResult result) {
            var userId = _analyticsDependency.GetUserId();
            var marketResult = result switch {
                MarketPlaceResult.BeginSell => "item_list",
                MarketPlaceResult.CancelSell => "item_cancel",
                MarketPlaceResult.Sold => "item_sell_done",
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
            Parameter[] parameters = {
                new("user_id", userId),
                new("product_name", productName),
                new("product_amount", amount),
                new("price", price),
                new("market_result", marketResult),
            };
            _bridge.LogEvent("track_marketplace", parameters);
        }

        public void MarketPlace_TrackSoftCurrency(
            string productName,
            float gemLockSpending,
            float gemSpending,
            float itemReceiving,
            TrackResult result
        ) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", "sink_gem"),
                new("source_type", "source_item"),
                new("sink_value", $"{gemLockSpending}-{gemSpending}"),
                new("source_value", itemReceiving),
                new("reason", $"{productName}"),
                new("result", TrackResultToString(result)),
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }

        public void MarketPlace_TrackBuyHeroTr(string heroName, int[] heroIds) {
            var userId = _analyticsDependency.GetUserId();
            var heroIdStr = string.Join("-", heroIds);
            Parameter[] parameters = {
                new("user_id", userId),
                new("hero_status", "hero_buy"),
                new("hero_name", heroName),
                new("hero_id", heroIdStr),
                new("hero_amount", heroIds.Length),
            };
            _bridge.LogEvent("track_hero", parameters);
        }

        public void Inventory_TrackOpenChestByGem(
            string chestType,
            int chestId,
            string[] receiveProductName,
            int[] receiveProductQuantity
        ) {
            var userId = _analyticsDependency.GetUserId();
            var received = string.Join("-", receiveProductName);
            Parameter[] parameters = {
                new("user_id", userId),
                new("chest_type", chestType),
                new("id_chest", chestId),
                new("product_name", received),
                new("amount", string.Join("-", receiveProductQuantity))
            };
            _bridge.LogEvent("track_open_chest", parameters);
        }

        public void Inventory_TrackEquipItem(int itemType, string itemName, int itemId) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("item_type", itemType),
                new("item_name", itemName),
                new("item_id", itemId),
            };
            _bridge.LogEvent("track_equip_item", parameters);
        }

        public void Inventory_TrackOpenChestGetHeroTr(string[] heroesNames, int[] heroesIds) {
            var userId = _analyticsDependency.GetUserId();
            var heroesNameStr = string.Join("-", heroesNames);
            var heroesIdStr = string.Join("-", heroesIds);
            Parameter[] parameters = {
                new("user_id", userId),
                new("hero_status", "open_chest"),
                new("hero_name", heroesNameStr),
                new("hero_id", heroesIdStr),
                new("hero_amount", heroesIds.Length),
            };
            _bridge.LogEvent("track_hero", parameters);
        }

        public void Adventure_TrackPlay(int heroId, int level, int stage, float totalTime, TrackPvpMatchResult result) {
            var userId = _analyticsDependency.GetUserId();
            var matchResult = result == TrackPvpMatchResult.Win ? "win" : "lose";
            Parameter[] parameters = {
                new("user_id", userId),
                new("hero_id", heroId),
                new("level", level),
                new("stage", stage),
                new("match_result", matchResult),
                new("total_time", totalTime),
            };
            _bridge.LogEvent("track_play_pve", parameters);
        }

        public void Adventure_TrackUseBooster(int level, int stage, string boosterName) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("level", level),
                new("stage", stage),
                new("booster_name", boosterName),
                new("booster_result", "use_pvp"),
                new("value", 1),
            };
            _bridge.LogEvent("track_pve_booster", parameters);
        }

        public void Adventure_TrackCollectItems(int level, int stage,
            Dictionary<TrackPvpCollectItemType, int> collected, TrackPvpMatchResult result) {
            var userId = _analyticsDependency.GetUserId();
            var matchResult = result switch {
                TrackPvpMatchResult.Win => "win",
                TrackPvpMatchResult.Lose => "lose",
                TrackPvpMatchResult.Draw => "draw",
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
            var array = (TrackPvpCollectItemType[]) Enum.GetValues(typeof(TrackPvpCollectItemType));
            foreach (var it in array) {
                collected.TryAdd(it, 0);
            }
            Parameter[] parameters = {
                new("user_id", userId),
                new("level", level),
                new("stage", stage),
                new("skull_head", collected[TrackPvpCollectItemType.SkullHead]),
                new("power", collected[TrackPvpCollectItemType.FireUp]),
                new("boots", collected[TrackPvpCollectItemType.Boots]),
                new("count", collected[TrackPvpCollectItemType.BombUp]),
                new("shield", collected[TrackPvpCollectItemType.Armor]),
                new("gold_1", collected[TrackPvpCollectItemType.Gold1]),
                new("gold_5", collected[TrackPvpCollectItemType.Gold5]),
                new("silver_chest", collected[TrackPvpCollectItemType.SilverChest]),
                new("bronze_chest", collected[TrackPvpCollectItemType.BronzeChest]),
                new("match_result", matchResult),
            };
            _bridge.LogEvent("track_collect_item_pve", parameters);
        }

        public void Adventure_TrackSoftCurrencyByWin(int level, int stage, float value) {
            var userId = _analyticsDependency.GetUserId();
            var reason = $"win_pve_level_{level}_stage_{stage}";
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", "sink_pve"),
                new("source_type", "source_gold"),
                new("sink_value", 1),
                new("source_value", value),
                new("reason", reason)
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }

        public void Adventure_TrackSoftCurrencyByX2Gold(int level, int stage, float value) {
            var userId = _analyticsDependency.GetUserId();
            var reason = $"x2_gold_pve_level_{level}_stage_{stage}";
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", "sink_pve_ad"),
                new("source_type", "source_gold"),
                new("sink_value", 1),
                new("source_value", value),
                new("reason", reason)
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }

        public void Adventure_TrackSoftCurrencyReviveByAds(int level, int stage, int reviveTimes) {
            var userId = _analyticsDependency.GetUserId();
            var reason = $"revive_by_ad_pve_level_{level}_stage_{stage}";
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", "sink_pve_ad"),
                new("source_type", "source_revive"),
                new("revive_times", reviveTimes),
                new("sink_value", 1),
                new("source_value", 1),
                new("reason", reason)
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }

        public void Adventure_TrackSoftCurrencyReviveByGem(int level, int stage, float valueLock, float valueUnlock, int reviveTimes) {
            var userId = _analyticsDependency.GetUserId();
            var reason = $"revive_by_gem_pve_level_{level}_stage_{stage}";
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", "sink_gem"),
                new("source_type", "source_revive"),
                new("revive_times", reviveTimes),
                new("sink_value", $"{valueLock}-{valueUnlock}"),
                new("source_value", 1),
                new("reason", reason)
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }
        
        public void TrackAds(AdsCategory adsCategory, TrackAdsResult result) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("category", AdsCategoryToString(adsCategory)),
                new("ad_format", "rewarded_ads"),
                new("ad_result", TrackAdsResultToString(result))
            };
            _bridge.LogEvent("track_ads", parameters);
        }

        public void TrackAds(string  category, TrackAdsResult result) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("category", category),
                new("ad_format", "rewarded_ads"),
                new("ad_result", TrackAdsResultToString(result))
            };
            _bridge.LogEvent("track_ads", parameters);
        }
        
        public void TrackInterstitial(AdsCategory adsCategory, TrackAdsResult result) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("category", AdsCategoryToString(adsCategory)),
                new("ad_format", "interstitial"),
                new("ad_result", TrackAdsResultToString(result))
            };
            _bridge.LogEvent("track_ads", parameters);
        }

        public void TrackLuckyWheel(string type, string mode, string[] names, string[] values, string matchResult, int level,
            int stage) {
            var userId = _analyticsDependency.GetUserId();
            var rewardName = string.Join("-", names);
            var rewardAmount = string.Join("-", values);
            Parameter[] parameters = {
                new("user_id", userId),
                new("type", type),
                new("mode", mode),
                new("reward_name", rewardName),
                new("reward_amount", rewardAmount),
                new("match_result", matchResult),
                new("level", level),
                new("stage", stage),
            };
            _bridge.LogEvent("track_lucky_wheel", parameters);
        }
        
        public void DailyGift_TrackCollectItems(int level) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("level_reward", level)
            };
            _bridge.LogEvent("track_collect_item_daily_gift", parameters);
        }

        private static string GetId() {
            var id = DateTime.Now.ToString("yyMMddHHmmssfff", CultureInfo.InvariantCulture);
            return id;
        }

        private static string TrackResultToString(TrackResult result) {
            return result switch {
                TrackResult.Error => "error",
                TrackResult.Cancel => "cancel",
                TrackResult.Done => "done",
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
        }

        private static string AdsCategoryToString(AdsCategory category) {
            return category switch {
                AdsCategory.ShortenTimeBronzeChest => "shorten_time_bronze_chest",
                AdsCategory.ShortenTimeSilverChest => "shorten_time_silver_chest",
                AdsCategory.X2GoldPve =>  "x2_gold_pve",
                AdsCategory.PlayButtonPve => "play_button_pve",
                AdsCategory.CompleteMatchPve => "complete_match_pve",
                AdsCategory.CompleteMatchPvp => "complete_match_pvp",
                AdsCategory.QuitMatchPve => "quit_match_pve",
                AdsCategory.X2GoldPvp => "x2_gold_pvp",
                AdsCategory.GetGemToAds => "get_gem_to_ads",
                AdsCategory.GetGoldToAds => "get_gold_to_ads",
                AdsCategory.ClaimDailyGiftLevel => "claim_daily_gift_level",
                _=> throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };
        }
        
        private static string TrackAdsResultToString(TrackAdsResult result) {
            return result switch {
                TrackAdsResult.Start => "start",
                TrackAdsResult.Cancel => "cancel",
                TrackAdsResult.Complete => "complete",
                TrackAdsResult.Error => "error",
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
        }
        
        public void ShopBuyCostumeUseGem_TrackSoftCurrency(
            string productName,
            float gemLockSpending,
            float gemSpending,
            float itemReceiving,
            string productType,
            string duration,
            TrackResult result
        ) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", "sink_gem"),
                new("source_type", $"{productName}"),
                new("sink_value", $"{gemLockSpending}-{gemSpending}"),
                new("source_value", itemReceiving),
                new("reason", "buy_item_in_shop"),
                new("product_type", $"{productType}"),
                new("duration", $"{duration}"),
                new("result", TrackResultToString(result)),
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }

        public void ShopBuyCostumeUseGold_TrackSoftCurrency(
            string productName,
            float goldSpending,
            float itemReceiving,
            string productType,
            string duration,
            TrackResult result
        ) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("sink_type", "sink_gold"),
                new("source_type", $"{productName}"),
                new("sink_value", $"{goldSpending}"),
                new("source_value", itemReceiving),
                new("reason", "buy_item_in_shop"),
                new("product_type", $"{productType}"),
                new("duration", $"{duration}"),
                new("result", TrackResultToString(result)),
            };
            _bridge.LogEvent("track_soft_currency", parameters);
        }
        
        #region UPRADE HERO

        public void TrackCreateCrystal(string heroName, int heroAmount,
            int crap, int lesser, int rough, int pure, int perfect,
            int gold) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("hero_name", heroName),
                new("hero_amount", heroAmount),
                new("crap", crap),
                new("lesser_crystal", lesser),
                new("rough_crystal", rough),
                new("pure_crystal", pure),
                new("perfect_crystal", perfect),
                new("gold_cost", gold)
            };
            _bridge.LogEvent("track_create_crystal", parameters);
        }

        public void TrackUpgradeCrystal(string rawCrystal, int rawAmount,
            string targetCrystal, int targetAmount,
            int gold, int gemLock, int gemUnlock) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("raw_crystal", rawCrystal),
                new("raw_amount", rawAmount),
                new("target_crystal", targetCrystal),
                new("target_amount", targetAmount),
                new("gold_cost", gold),
                new("gem_cost", $"{gemLock} - {gemUnlock}")
            };
            _bridge.LogEvent("track_upgrade_crystal", parameters);
        }

        public void TrackUpgradeBaseIndex(int heroId, string heroName, string index, int level,
            int lesser, int rough, int pure, int perfect,
            int gold, int gemLock, int gemUnlock) {
            var userId = _analyticsDependency.GetUserId();
            Parameter[] parameters = {
                new("user_id", userId),
                new("hero_id", heroId),
                new("hero_name", heroName),
                new("index", index),
                new("level", level),
                new("lesser_crystal", lesser),
                new("rough_crystal", rough),
                new("pure_crystal", pure),
                new("perfect_crystal", perfect),
                new("gold_cost", gold),
                new("gem_cost", $"{gemLock}-{gemUnlock}")
            };
            _bridge.LogEvent("track_upgrade_base_index", parameters);
        }

        public void TrackUpgradeMaxIndex(int heroId, string heroName, string index, int times,
            int lesser, int rough, int pure, int perfect,
            int gold, int gemLock, int gemUnlock) {
            var userId = _analyticsDependency.GetUserId();
            var parameters = new List<Parameter>() {
                new("user_id", userId),
                new("hero_id", heroId),
                new("hero_name", heroName),
                new("index", index),
                new("times_upgrade", times),
                new("lesser_crystal", lesser),
                new("rough_crystal", rough),
                new("pure_crystal", pure),
                new("perfect_crystal", perfect),
                new("gold_cost", gold),
            };
            if (gemLock + gemUnlock > 0) {
                parameters.Add(new Parameter("gem_cost", $"{gemLock}-{gemUnlock}"));
            }
            
            _bridge.LogEvent("track_upgrade_max_index", parameters.ToArray());
        }

        public void PushGameLevel(int levelNo, string levelMode) {
        }

        public void PopGameLevel(bool winGame) {
        }

        public void LogAdRevenue(AdNetwork mediationNetwork, string monetizationNetwork, double revenue, string currencyCode,
            AdFormat format, string adUnit) {
            _bridge.LogAdRevenue(mediationNetwork, monetizationNetwork, revenue, currencyCode, format, adUnit, null);
        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue, string currencyIso,
            string receipt) {
            _bridge.LogIapRevenue(eventName, packageName, orderId, priceValue, currencyIso, receipt);
        }

        #endregion        
    }
}