using System;
using System.Threading.Tasks;

namespace App {
    public interface IDedupServerRequester {
        Task<T> Dedup<T>(string key, Func<Task<T>> fetch);
    }
}
