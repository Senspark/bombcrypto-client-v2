using System.Threading.Tasks;

using Senspark;

namespace App {
    [Service(nameof(IEmailManager))]
    public interface IEmailManager : IService {
        EmailStatus Status { get; }
        string Email { get; }
        Task GetEmail();
        Task<bool> RegisterEmail(string email);
        Task<bool> VerifyCode(int code);
    }
}
