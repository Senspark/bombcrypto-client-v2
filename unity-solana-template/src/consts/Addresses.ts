const testAddress = "";
const prodAddress = "7aujakweAK1WFyJgqotHNSuyDAGSREwyppkaruQD4gMw";

export function getMerchantAddress(isProd: boolean): string {
    return isProd ? prodAddress : testAddress;
}

const bcoinTestAddress = "abc";
const bcoinProdAddress = "xyz";
export function getBcoinAddress(isProd: boolean): string {
    return isProd ? bcoinProdAddress : bcoinTestAddress;
}