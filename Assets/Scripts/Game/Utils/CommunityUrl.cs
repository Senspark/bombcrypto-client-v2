using System;

using App;

namespace Utils {
    public static class CommunityUrl {
        public enum CommunityLink {
            Twitter = 0, Youtube = 1, Discord = 2, Tiktok = 3, Telegram = 4, Medium = 5
        }
        
        public enum TelegramLink {
            Vietnam = 0, Philippines = 1, China = 2, Brazil = 3, Spanish = 4, Thailand = 5, Arab = 6
        }

        public static void OpenLink(CommunityLink communityLink, NetworkType networkType) {
            var url = communityLink switch {
                CommunityLink.Twitter => "https://twitter.com/BombCryptoGame",
                CommunityLink.Youtube => "https://bit.ly/bombcrypto",
                CommunityLink.Discord => "https://discord.com/invite/bombcryptoofficial",
                CommunityLink.Tiktok => "https://www.tiktok.com/@bombcrypto_official",
                CommunityLink.Telegram => networkType switch {
                    NetworkType.Binance => "https://t.me/BombCryptoGroup",
                    _ => "https://t.me/Bombcrypto2_polygon",
                },
                CommunityLink.Medium => "https://bombcrypto.medium.com/",
                _ => null,
            };

            if (url != null) {
                WebGLUtils.OpenUrl(url);
            }
        }

        public static void OpenTelegramLink(TelegramLink link) {
            var url = link switch {
                TelegramLink.Vietnam => "https://t.me/BombcryptoVietnamgroup",
                TelegramLink.Philippines => "https://t.me/Bombcrypto_PH",
                TelegramLink.China => "https://t.me/BombCryptoGroup_CN",
                TelegramLink.Brazil => "https://t.me/BombCryptoBR",
                TelegramLink.Spanish => "https://t.me/BombCryptoGroup_SP",
                TelegramLink.Thailand => "https://t.me/bombcrypto_Thailand",
                TelegramLink.Arab => "https://t.me/BombCryptoArab",
                _ => null,
            };
            
            if(url != null) {
                WebGLUtils.OpenUrl(url);
            }
        }

        public static void OpenHome(NetworkType networkType) {
            var url = networkType switch {
                NetworkType.Binance => "https://bombcrypto.io/",
                _ => "https://polygon.bombcrypto.io/",
            };
            WebGLUtils.OpenUrl(url);
        }
    }
}