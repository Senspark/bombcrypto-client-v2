using System;

using App;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class NewsEventPanel : MonoBehaviour {
        [SerializeField]
        private Image newsImage;

        [SerializeField]
        private Text title;

        [SerializeField]
        private Text date;

        [SerializeField]
        private Image redDot;

        public string NewsContent { get; private set; }
        public Sprite NewsSprite { get; private set; }
        public string NewsLink { get; private set; }
        
        private Action<NewsEventPanel> _onDidLoadImage;
        private Action<NewsEventPanel> _onClickCallback;

        private int _newsId;
        private INewsManager _newsManager;

        private void Awake() {
            _newsManager = ServiceLocator.Instance.Resolve<INewsManager>();
        }

        public void InitPanel(NewsMessage newsEvent, Action<NewsEventPanel> didLoadCallback, Action<NewsEventPanel> clickCallback) {
            _newsId = newsEvent.Id;
            NewsContent = newsEvent.Content.Replace("\\n", "\n");
            NewsLink = newsEvent.NewsLink;

            _onDidLoadImage = didLoadCallback;
            _onClickCallback = clickCallback;

            title.text = newsEvent.Title.Replace("\\n", "\n");;
            var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(newsEvent.EventDate);
            date.text = dateTime.ToString("dd/MM/yyyy");
            redDot.gameObject.SetActive(!_newsManager.IsHadRead(newsEvent.Id));
            if (NewsSprite != null) {
                _onDidLoadImage?.Invoke(this);
                return;
            }
            // EE.Utils.NoAwait(async () => {
            //     NewsSprite = await App.Utils.LoadImageFromUrl(newsEvent.ImageLink);
            //     newsImage.sprite = await App.Utils.LoadImageFromUrl(newsEvent.ImageLink);
            //     _onDidLoadImage?.Invoke(this);
            // });
            NewsSprite = ConvertBase64ToSprite(newsEvent.ImageLink);
            newsImage.sprite = NewsSprite;
            _onDidLoadImage?.Invoke(this);
        }

        private Sprite ConvertBase64ToSprite(string base64) {
            var imageBytes = Convert.FromBase64String(base64);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            tex.LoadImage(imageBytes);
            var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }
        
        public void OnClicked() {
            _newsManager.SaveNewsRead(_newsId);
            redDot.gameObject.SetActive(false);
            _onClickCallback?.Invoke(this);
        }
    }
}