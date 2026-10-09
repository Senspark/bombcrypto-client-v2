using System;
using System.Linq;
using System.Threading.Tasks;

using App;

using Cysharp.Threading.Tasks;

using Game.UI;

using Scenes.FarmingScene.Scripts;

using Senspark;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    /// <summary>
    /// Tab UPGRADE BHERO LEVEL.
    ///
    /// Duas faixas, com regras diferentes:
    ///
    /// - Niveis 2 a 5 (LegacyMaxLevel): material do MESMO nivel do hero base, pagamento so em
    ///   native. E o comportamento do BHeroS em producao, intocado.
    /// - Niveis 6 a 10: material sempre nivel 5, pagamento em BCOIN + SEN + native, atraves do
    ///   BHeroUpgradeV2. O material fixo existe porque a regra "mesmo nivel" dobra o custo em
    ///   herois a cada degrau — chegar ao 10 exigiria 512 herois base contra 96 assim.
    /// </summary>
    public class UpgradeHeroLevelPolygon : NativeHeroActionPolygon {
        /// Ultimo nivel sob a regra antiga. Precisa bater com LEGACY_MAX_LEVEL do BHeroUpgradeV2.
        private const int LegacyMaxLevel = 5;

        /// Teto atual. Espelha BHeroDesign.getMaxLevel(); trocar aqui exige trocar la tambem.
        private const int MaxLevel = 10;

        // Layout do botao no modo empilhado (tres linhas de preco). Medido contra o prefab
        // DialogSmithyPolygon: botao 260x90.19, titulo 35 de altura ancorado no centro, label de
        // preco ancorado no rodape a 27px. Ver AdjustButtonForPrice.
        private const float StackedHeightScale = 1.45f;
        private const float PriceFontScale = 0.78f;
        private const float PriceLineSpacing = 0.95f;
        /// Folga entre o conteudo e as bordas de cima e de baixo do botao.
        private const float EdgePadding = 8f;
        /// Folga entre a base do titulo e o topo do bloco de preco.
        private const float TitleGap = 4f;
        /// Quanto o botao inteiro desce no modo empilhado, para nao encostar no slot de material.
        private const float StackedDropY = 22f;

        /// Preco dos tres tokens, lido junto com o native nos niveis altos.
        private UpgradePrice _v2Price = UpgradePrice.Zero;

        /// Altura original do botao, medida na primeira vez. Guardada para conseguir voltar ao
        /// tamanho de uma linha quando o hero selecionado for de nivel legado.
        private float _baseButtonHeight;

        /// Fonte original do preco, para restaurar nos niveis legados.
        private int _basePriceFontSize;

        /// Posicao original do label do preco, para restaurar nos niveis legados.
        private Vector2 _basePriceAnchoredPos;
        private bool _basePricePosCaptured;

        /// Label "UPGRADE" do botao e posicao original dele e do proprio botao.
        private Text _titleLbl;
        private Vector2 _baseTitlePos;
        private Vector2 _baseButtonPos;
        private bool _baseLayoutCaptured;

        [SerializeField]
        private SmithyHeroSlot materialSlot;

        [SerializeField]
        private Button chooseMaterialBtn;

        [SerializeField]
        private Text levelLbl;

        [SerializeField]
        private Text nextLevelLbl;

        private PlayerData _material;

        protected override string ActionName => "Upgrade";

        protected override bool CanProcess(PlayerData hero) {
            return hero != null
                   && hero.AccountType == HeroAccountType.Nft
                   && hero.level < MaxLevel;
        }

        /// Nivel exigido do material. Ate o legado, mesmo nivel do base; acima dele, sempre 5.
        private static int RequiredMaterialLevel(int baseLevel) {
            return baseLevel < LegacyMaxLevel ? baseLevel : LegacyMaxLevel;
        }


        // Giá tính theo bậc SẮP lên, level truyền cho contract là index 0-based.
        //
        // Nos niveis altos o preco vem do BHeroUpgradeV2 em tres moedas; guardamos as tres e
        // devolvemos so a nativa, que e o que a classe base usa para checar saldo e msg.value.
        protected override async Task<string> ReadPriceWei() {
            // TODOS os niveis passam pelo BHeroUpgradeV2, nao so os altos: ele cobre 1-10 e devolve
            // (0, 0, nativo) abaixo do nivel 5, igual ao contrato de producao. O caminho antigo lia do
            // BHeroS, que nao existe neste stack de teste  o preco voltava vazio e o botao mostrava
            // "--" em qualquer hero de nivel 1 a 4.
            _v2Price = await BlockchainManager.GetUpgradeV2Price(Hero.heroId.Id) ?? UpgradePrice.Zero;
            return _v2Price.Native;
        }

        protected override Task<HeroActionResult> SendAction(string priceWei) {
            // priceWei e a parcela nativa recem-relida pela classe base. Reaproveitar o _v2Price
            // inteiro seria arriscado se so o nativo tivesse mudado, entao ele manda.
            var price = new UpgradePrice {
                Bcoin = _v2Price.Bcoin,
                Sen = _v2Price.Sen,
                Native = priceWei
            };
            return BlockchainManager.UpgradeHeroV2(Hero.heroId.Id, _material.heroId.Id, price);
        }

        // Nos niveis altos o jogador paga tres moedas; mostrar so o BNB esconderia o custo real.
        protected override string BuildPriceText(double nativePrice) {
            var native = $"{FormatCoin(nativePrice)} {CoinSymbol}";
            // O empilhamento segue o CUSTO, nao o nivel: abaixo do 5 o contrato cobra so o nativo.
            if (!_v2Price.HasTokenCost) {
                return native;
            }
            var bcoin = FormatCoin(WeiToCoin(_v2Price.Bcoin));
            var sen = FormatCoin(WeiToCoin(_v2Price.Sen));
            // Uma moeda por linha: as tres em sequencia estouram a largura do botao e o texto
            // sai deformado. O Text do prefab tem alinhamento central, entao empilhar funciona
            // sem precisar mexer no prefab.
            return $"{bcoin} BCOIN\n{sen} SEN\n{native}";
        }

        // Nguyên liệu đang có stake BCOIN/SEN thì stake đó cháy theo hero. Cảnh báo trước khi ký,
        // cùng dialog mà luồng đốt hero ở MaterialPolygon đang dùng.
        protected override void RequestConfirmation(Action onConfirmed) {
            if (_material == null) {
                onConfirmed();
                return;
            }
            // Rele do store em vez de confiar no snapshot feito na selecao: entre escolher o
            // material e assinar, o stake pode ter mudado, e ai o aviso nao apareceria. E o que o
            // DialogFusionPolygon ja faz em CheckStakedBeforeBurn.
            var atual = PlayerStoreManager.GetPlayerDataFromId(_material.heroId) ?? _material;
            if (!atual.HaveAnyStaked()) {
                onConfirmed();
                return;
            }
            UniTask.Void(async () => {
                var confirm = await DialogConfirmBurnOrFusion.Create();
                if (!this) {
                    return;
                }
                confirm.SetInfo(1, onConfirmed, () => { });
                confirm.Show(Canvas);
            });
        }

        // Hero nguyên liệu đã bị đốt on-chain và không có push nào báo việc đó, nên phải tự gỡ
        // khỏi store, khỏi map và trừ vào sức chứa — đúng bộ ba mà luồng đốt hero ở
        // MaterialPolygon đang làm. Chạy trước khi Init() clear _material.
        protected override void OnActionSucceeded(PlayerData updated) {
            if (_material == null) {
                return;
            }
            var burned = new[] { _material.heroId };
            PlayerStoreManager.RemoveBurnHeroes(burned);
            PlayerStoreManager.AdjustTotalHeroesSize(-1);
            // Forge mở được cả ngoài map, nên phải guard Instance.
            if (LevelScene.Instance) {
                LevelScene.Instance.RemoveHeroesFromMap(burned);
            }
        }

        protected override void OnHeroChanged() {
            // Đổi hero gốc thì nguyên liệu cũ không còn chắc cùng level nữa -> clear luôn.
            SetMaterial(null);
            if (levelLbl) {
                levelLbl.text = Hero != null ? Hero.level.ToString() : string.Empty;
            }
            if (nextLevelLbl) {
                nextLevelLbl.text = Hero != null ? (Hero.level + 1).ToString() : string.Empty;
            }
            // Ô nguyên liệu chỉ xuất hiện sau khi đã chọn hero gốc, vì nó lọc theo level hero gốc.
            // Reposiciona tambem ao trocar de hero: OnQuoteApplied so roda quando o preco chega,
            // entao sem isto o botao ficava na posicao do hero anterior enquanto carrega.
            AdjustButtonForPrice();
            ShowMaterialUi(Hero != null);
        }

        private void ShowMaterialUi(bool visible) {
            if (materialSlot) {
                materialSlot.gameObject.SetActive(visible);
            }
            if (chooseMaterialBtn) {
                chooseMaterialBtn.gameObject.SetActive(visible);
            }
        }

        public async void OnChooseMaterialBtnClicked() {
            if (Hero == null) {
                return;
            }
            SoundManager.PlaySound(Audio.Tap);
            var inventory = await DialogInventoryCreator.Create();
            if (!this) {
                return;
            }
            // Exclude list được áp TRƯỚC khi phân trang, khác với FilterHeroesSuitableToUpgrade()
            // vốn chỉ lọc trong phạm vi trang hiện tại. Vẫn dùng ChooseMode.Upgrade để InventoryItem
            // bật cảnh báo hero đang stake khi chọn — nguyên liệu sẽ bị đốt.
            var exclude = PlayerStoreManager.GetPlayerDataList(HeroAccountType.Nft)
                .Where(e => !IsUsableAsMaterial(e))
                .Select(e => e.heroId)
                .ToArray();
            inventory.SetChooseHeroForUpgrade(Hero.heroId, RequiredMaterialLevel(Hero.level), exclude, heroId => {
                SetMaterial(PlayerStoreManager.GetPlayerDataFromId(heroId));
            });
            inventory.Show(Canvas);
        }

        // Ate o nivel legado o material e do MESMO nivel do base; do 5 em diante, sempre nivel 5.
        private bool IsUsableAsMaterial(PlayerData hero) {
            return hero != null
                   && hero.AccountType == HeroAccountType.Nft
                   && hero.heroId != Hero.heroId
                   && hero.level == RequiredMaterialLevel(Hero.level);
        }

        private void SetMaterial(PlayerData material) {
            _material = material;
            if (materialSlot) {
                materialSlot.Show(_material);
            }
            RefreshQuote();
        }

        // Chưa chọn nguyên liệu thì chưa cho bấm, dù giá đã đọc được.
        protected override void OnQuoteApplied(bool hasPrice) {
            AdjustButtonForPrice();
            if (hasPrice && _material == null) {
                SetInteractable(false);
            }
        }

        /// O preco dos niveis altos ocupa tres linhas e nao cabe na altura do botao, desenhada
        /// para uma linha so. Ajustar por codigo em vez de editar o prefab: prefab e asset binario,
        /// caro de revisar e de mesclar com o upstream.
        private void AdjustButtonForPrice() {
            if (!actionBtn) {
                return;
            }
            var rect = actionBtn.transform as RectTransform;
            if (rect == null) {
                return;
            }
            if (_baseButtonHeight <= 0f) {
                _baseButtonHeight = rect.sizeDelta.y;
            }
            var stacked = _v2Price.HasTokenCost;

            // O titulo do botao e o unico Text filho que nao e o priceLbl. Descobrir por busca
            // evita adicionar um campo serializado, que exigiria editar o prefab.
            if (_titleLbl == null && priceLbl) {
                foreach (var t in actionBtn.GetComponentsInChildren<Text>(true)) {
                    if (t != priceLbl) {
                        _titleLbl = t;
                        break;
                    }
                }
            }
            if (!_baseLayoutCaptured) {
                _baseButtonPos = rect.anchoredPosition;
                if (_titleLbl != null && _titleLbl.transform is RectTransform tr) {
                    _baseTitlePos = tr.anchoredPosition;
                }
                _baseLayoutCaptured = true;
            }

            // Altura empilhada e a folga interna sao os unicos numeros escolhidos a mao; titulo e
            // preco sao posicionados a partir da geometria real, nao de multiplos da altura base.
            var height = _baseButtonHeight * (stacked ? StackedHeightScale : 1f);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
            // Com preco de uma linha o botao fica exatamente onde o prefab o coloca. Empilhado ele
            // desce um pouco para abrir espaco em relacao ao slot de material acima.
            rect.anchoredPosition = new Vector2(
                _baseButtonPos.x,
                _baseButtonPos.y - (stacked ? StackedDropY : 0f));

            var half = height * 0.5f;

            // Titulo encostado no topo, virando cabecalho das linhas de preco. Ele esta ancorado no
            // CENTRO do botao, entao nao acompanha o crescimento sozinho.
            var titleBottom = _baseTitlePos.y;
            if (_titleLbl != null && _titleLbl.transform is RectTransform titleRect) {
                var titleHalf = titleRect.rect.height * 0.5f;
                var titleY = stacked ? half - titleHalf - EdgePadding : _baseTitlePos.y;
                titleRect.anchoredPosition = new Vector2(_baseTitlePos.x, titleY);
                titleBottom = titleY - titleHalf;
            }

            if (!priceLbl) {
                return;
            }
            if (_basePriceFontSize <= 0) {
                _basePriceFontSize = priceLbl.fontSize;
            }
            priceLbl.fontSize = stacked
                ? Mathf.Max(1, Mathf.RoundToInt(_basePriceFontSize * PriceFontScale))
                : _basePriceFontSize;
            priceLbl.lineSpacing = stacked ? PriceLineSpacing : 1f;
            priceLbl.alignment = TextAnchor.MiddleCenter;
            priceLbl.resizeTextForBestFit = false;
            // Overflow no horizontal: com Wrap o Unity quebrava "10415 BCOIN" em duas linhas e o
            // Truncate vertical comia SEN e BNB. Cada moeda ja vem na propria linha, entao
            // deixar transbordar um pouco na largura e melhor do que perder duas das tres linhas.
            priceLbl.horizontalOverflow = HorizontalWrapMode.Overflow;
            priceLbl.verticalOverflow = VerticalWrapMode.Overflow;

            // O label esta ancorado no RODAPE do botao (anchorMin.y = 0), entao quando o botao
            // cresce ele fica parado a 27px do chao novo e as tres linhas vazam pela borda. Recentra
            // no espaco que sobra entre a base do titulo e o rodape.
            if (priceLbl.transform is RectTransform labelRect) {
                if (!_basePricePosCaptured) {
                    _basePriceAnchoredPos = labelRect.anchoredPosition;
                    _basePricePosCaptured = true;
                }
                var y = _basePriceAnchoredPos.y;
                if (stacked) {
                    var regionTop = titleBottom - TitleGap;
                    var regionBottom = -half + EdgePadding;
                    // anchoredPosition e medida a partir do rodape; o centro do botao esta a `half`.
                    y = (regionTop + regionBottom) * 0.5f + half;
                }
                labelRect.anchoredPosition = new Vector2(_basePriceAnchoredPos.x, y);
            }
        }
    }
}
