using Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class RecentServices
    {
        private RecentRepository _recentRepository;
        public RecentServices()
        {
            _recentRepository = new RecentRepository();
        }
        public DateTime DthOpened { get; set; }
        public string FilePath { get; set; } = null!;

        public async Task<ICollection<Recents>> List()
        {
            return await _recentRepository.List();
        }

        public async Task Save(Recents recent )
        {
            await _recentRepository.Save(recent);
        }

        public async Task Delete(int id) {
            await _recentRepository.Delete(id);
        }
    }
}
