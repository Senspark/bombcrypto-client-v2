namespace Communicate.Encrypt {
    public interface IObfuscate {
        string Obfuscate(string base64);
        string Obfuscate2(byte[] bytes);
        string DeObfuscate(string base64);
        byte[] DeObfuscate2(string base64);
        byte[] DeObfuscateBytes(byte[] data);
    }
}