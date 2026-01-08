//FIXME: Ko còn dùng trong game
// import GeneralContract from "./Utils/GeneralContract.js";
// import * as Message from "./Utils/Message.js";
// import { getDoubleGasFeeOption, waitForReceipt } from "./Utils/NetworkUtils.js";
// import { getData } from "./Utils/Storage.js";
// import { ethers, Contract } from "ethers";
//
// export default class BirthdayEvent extends GeneralContract {
//     /**
//      * Kiểm tra user có sử dụng voucher chưa
//      * @param {string} userAddress
//      * @returns {Promise<boolean>} [true/false]
//      */
//     async isUserUsedDiscount(userAddress: string): Promise<boolean> {
//         const contract: Contract = await this.getContract();
//         const value: boolean = await contract.checkUserUsedDiscount(userAddress);
//         return value;
//     }
//
//     /**
//      * Lấy ra thời gian của Event
//      * @returns {Promise<[string, string]>} [Mảng 2 phần tử: timeStart (number), timeEnd (number)]
//      */
//     async getTimeLeft(): Promise<[string, string]> {
//         const contract: Contract = await this.getContract();
//         const timeStart = await contract.timeStart();
//         const timeEnd = await contract.timeEnd();
//         return [timeStart.toString(), timeEnd.toString()];
//     }
//
//     /**
//      * Mua hero dùng voucher
//      * @param {number} category
//      * @param {string} userAddress
//      * @returns {Promise<Message>}
//      */
//     async buyHero(category: number, userAddress: string): Promise<Message.Message> {
//         try {
//             const bheroToken = getData('hero');
//             const bheroSToken = getData('heroS');
//             const count = 15;
//             const cost = await bheroSToken.getHeroPrice(category);
//             const costBN = ethers.BigNumber.from(count).mul(ethers.BigNumber.from(cost));
//             const token = getCoinTokenByBuyHeroCategory(category);
//             await token.checkAllowance(userAddress, bheroToken._address, costBN);
//
//             const contract: Contract = await this.getContract();
//             const estimateGas: BigNumber = await contract.estimateGas.buyHeroEvent(count, category);
//             const options = getDoubleGasFeeOption(estimateGas);
//             const transaction = await contract.buyHeroEvent(count, category, options);
//             await waitForReceipt(transaction);
//             return Message.Info();
//         } catch (ex) {
//             console.error(`exception ${ex}`);
//             return Message.Error(ex.message.substring(0, 100));
//         }
//     }
// }