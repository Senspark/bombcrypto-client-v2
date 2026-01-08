using System;

namespace BLPvpMode.Manager.Api.Modules {
    public interface IUserModule : IDisposable {
        void Update(float delta);
    }
}