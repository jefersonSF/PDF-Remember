using Models;
using Repository;

namespace Services
{
    public class PdfInfoServices
    {
        private PdfInfoRepository _pdfRepository;
        public PdfInfoServices()
        {
            _pdfRepository = new PdfInfoRepository();
        }

        public async Task deleteAsync(long id)
        {
            await _pdfRepository.Delete(id);
        }

        public async Task<PdfInfo?> FindPdfInfoByFullPath(string fileHash) {
            return await _pdfRepository.FindPdfInfoByFullPath(fileHash);
        }

        public async Task Save(PdfInfo info) {
            await _pdfRepository.Save(info);
        }
    }
}
