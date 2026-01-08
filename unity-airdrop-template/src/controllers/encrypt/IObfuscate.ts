export default interface IObfuscate {

    /**
     * @param base64 base64 string
     * @return base64 string
     */
    obfuscate(base64: string): string;

    /**
     * @param base64 base64 string
     * @return base64 string
     */
    deobfuscate(base64: string): string;

    /**
     * @param bytes
     * @return base64 string
     */
    obfuscate2(bytes: Uint8Array): string;

    /**
     * @param base64 base64 string
     */
    deobfuscate2(base64: string): Uint8Array;
}