using Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class PreferenceServices
    {
        private PreferenceRepository _preferenceRepository;
        public PreferenceServices()
        {
            _preferenceRepository = new PreferenceRepository();
        }

        public async Task<ICollection<Preference>> List()
        {
            return await _preferenceRepository.List();
        }

        public async Task Save(Preference preference )
        {
            await _preferenceRepository.Save(preference);
        }
    }
}
