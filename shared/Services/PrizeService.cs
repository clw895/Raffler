using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using shared.Models;

namespace shared.Services
{
    public class PrizeService : IPrizeService
    {
        private readonly IPrizeStorageService _storageUpdater;
        private IDictionary<int, RafflePrize> _prizes;
        private readonly ILogger _logger;

        public PrizeService(ILogger<PrizeService> logger)
        {
            _logger = logger;
        }

        public bool IsInitialized { get; set; }

        public PrizeService(IPrizeStorageService storageUpdater) => _storageUpdater = storageUpdater;

        public async Task InitializeService()
        {
            if (!_storageUpdater.IsInitialized)
            {
                await _storageUpdater.InitializeService();
            }

            if (_prizes == null)
            {
                _prizes = await _storageUpdater.GetItems() ?? new Dictionary<int, RafflePrize>();
            }
            IsInitialized = true;
        }

        public async Task AddRafflePrize(RafflePrize prize)
        {

            if (_prizes.Values.Any(result => string.Equals(prize.Name, result.Name, StringComparison.CurrentCultureIgnoreCase)))
            {
                var index = _prizes.First(result => string.Equals(result.Value.Name, prize.Name, StringComparison.CurrentCultureIgnoreCase)).Key;

                _prizes[index].Quantity += prize.Quantity;
                await _storageUpdater.UpdateRepository(index, prize);
            }
            else
            {
                var index = await _storageUpdater.AddItemToRepository(prize);
                _prizes.Add(index, prize);
            }
        }

        public Task<RafflePrize> GetCurrentPrize()
        {
            throw new NotImplementedException();
        }

        public async Task<RafflePrize> SelectPrize()
        {
            _logger.LogInformation("Selecting a prize from the list of eligible prizes");
            var eligiblePrizes = _prizes.Where(entry => !entry.Value.IsSelectedPrize && entry.Value.Quantity > 0).ToList();

            var index = new Random().Next(0, eligiblePrizes.Count);
            var selected = eligiblePrizes[index];

            selected.Value.IsSelectedPrize = true;

            await _storageUpdater.UpdateRepository(selected.Key, selected.Value);

            _logger.LogInformation("Selected a prize from the list of eligible prizes: {0}", selected.Value);

            return selected.Value;
        }

        public async Task<IList<RafflePrize>> GetRafflePrizes()
        {
            if (_prizes == null)
            {
                _prizes = await _storageUpdater.GetItems();
            }
            return _prizes.Values.ToList();
        }
    }
}