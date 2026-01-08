using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App;

using Data;

using Senspark;

using Services.Server.Exceptions;

namespace Services {
    public class SkinManager : ISkinManager {
        private IProductManager _productManager;
        private readonly IServerManager _serverManager;

        public SkinManager(IServerManager serverManager) {
            _serverManager = serverManager;
        }

        public void Destroy() {
        }

        public async Task EquipSkinAsync(int itemType, IEnumerable<(int, long)> itemList) {
            await _serverManager.Pvp.Equip(itemType, itemList);
        }

        public async Task<IEnumerable<ISkinManager.Skin>> GetSkinsAsync(int skinType) {
            await ServiceLocator.Instance.Resolve<IProductItemManager>().InitializeAsync();
            _productManager ??= ServiceLocator.Instance.Resolve<IProductManager>();
            var result = await _serverManager.Pvp.GetEquipment();
            return result.Equipments.Where(it => it.ItemType == skinType).Select(it => new ISkinManager.Skin(
                it.Ids,
                it.IsForever,
                it.ExpirationAfter,
                it.Equipped,
                it.Expire,
                it.Quantity,
                it.ItemId,
                _productManager.GetProduct(it.ItemId).ProductName,
                it.Used,
                it.ItemType,
                it.Stats
            ));
        }
        
        public async Task<IEnumerable<ISkinManager.Skin>> GetSkinsEquipped() {
            await ServiceLocator.Instance.Resolve<IProductItemManager>().InitializeAsync();
            _productManager ??= ServiceLocator.Instance.Resolve<IProductManager>();
            var result = await _serverManager.Pvp.GetEquipment();
            return result.Equipments.Where(it => it.Equipped).Select(it => new ISkinManager.Skin(
                it.Ids,
                it.IsForever,
                it.ExpirationAfter,
                it.Equipped,
                it.Expire,
                it.Quantity,
                it.ItemId,
                _productManager.GetProduct(it.ItemId).ProductName,
                it.Used,
                it.ItemType,
                it.Stats
            ));
        }
        
        public async Task<IEnumerable<ISkinManager.Skin>> GetSkinsEquipped(EquipmentData[] equipments) {
            await ServiceLocator.Instance.Resolve<IProductItemManager>().InitializeAsync();
            _productManager ??= ServiceLocator.Instance.Resolve<IProductManager>();
            return equipments.Where(it => it.Equipped).Select(it => new ISkinManager.Skin(
                it.Ids,
                it.IsForever,
                it.ExpirationAfter,
                it.Equipped,
                it.Expire,
                it.Quantity,
                it.ItemId,
                _productManager.GetProduct(it.ItemId).ProductName,
                it.Used,
                it.ItemType,
                it.Stats
            ));
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }
    }
}