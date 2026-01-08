using System.Threading.Tasks;

namespace Senspark.Internal {
    public static class FirebaseInitializer {
        private static Task<bool> _initializer;

        public static Task<bool> Initialize() => _initializer ??= InitializeImpl();

        private static async Task<bool> InitializeImpl() {
#if !UNITY_WEBGL
            try {
                // https://firebase.google.com/docs/unity/setup
                var status = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
                return status == Firebase.DependencyStatus.Available;
            } catch {
                return false;
            }
#else
            return await Task.FromResult(true);
#endif
        }
    }
}