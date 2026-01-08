import { ethers } from 'ethers';

export class EtherMessageUtils {
    /**
     * Encode a string message to hex format for transaction data
     * @param message - Plain text message (e.g., "DEP001")
     * @returns Hex-encoded string (e.g., "0x444550303031")
     */
    static encodeMessage(message: string): string {
        if (!message || message.length === 0) {
            return "0x";
        }
        
        // Convert string to bytes and then to hex
        const bytes = ethers.toUtf8Bytes(message);
        return ethers.hexlify(bytes);
    }

    /**
     * Decode hex data back to string message
     * @param hexData - Hex-encoded string (e.g., "0x444550303031")
     * @returns Plain text message (e.g., "DEP001")
     */
    static decodeMessage(hexData: string): string {
        if (!hexData || hexData === "0x" || hexData.length === 0) {
            return "";
        }
        
        try {
            // Convert hex to UTF-8 string
            return ethers.toUtf8String(hexData);
        } catch {
            // If decoding fails, return empty string
            return "";
        }
    }

    /**
     * Validate if a message is a valid invoice code format
     * @param message - Message to validate
     * @returns true if valid invoice format
     */
    static isValidInvoiceCode(message: string): boolean {
        if (!message || message.length === 0) {
            return false;
        }
        
        // Check if it matches common invoice patterns (DEP, BCD, etc.)
        const invoicePattern = /^[A-Z]{3}\d{3,6}$/;
        return invoicePattern.test(message);
    }

    /**
     * Sanitize message for safe encoding
     * @param message - Raw message
     * @returns Sanitized message
     */
    static sanitizeMessage(message: string): string {
        if (!message) {
            return "";
        }
        
        // Remove null bytes and control characters
        // eslint-disable-next-line no-control-regex
        return message.replace(/[\x00-\x1F\x7F]/g, "").trim();
    }
}

export default EtherMessageUtils;