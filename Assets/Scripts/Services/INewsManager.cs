using System.Collections.Generic;
using System.Threading.Tasks;

using Senspark;

namespace App {
    [Service(nameof(INewsManager))]
    public interface INewsManager : IService {
        Task SyncData();
        List<NewsMessage> GetNews();
        List<AnnouncementsMessage> GetAnnouncements();
        public void SaveNewsRead(int newsId);
        public bool IsHadRead(int newsId);
    }
}