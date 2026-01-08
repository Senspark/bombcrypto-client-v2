#import "AppsFlyerPurchaseiOSWrapper.h"

static void unityCallBack(NSString* objectName, const char* method, const char* msg) {
    if(objectName){
        UnitySendMessage([objectName UTF8String], method, msg);
    }
}

extern "C" {

    const void _startObservingTransactions() {
        [[PurchaseConnector shared] startObservingTransactions];
    }

    const void _stopObservingTransactions() {
        [[PurchaseConnector shared] stopObservingTransactions];
    }

    const void _setIsSandbox(bool isSandBox) {
        [[PurchaseConnector shared] setIsSandbox:isSandBox];
    }

    const void _setPurchaseRevenueDelegate() {
        if (_AppsFlyerPurchasedelegate == nil) {
            _AppsFlyerPurchasedelegate = [[AppsFlyerPurchaseiOSWrapper alloc] init];
               }
        [[PurchaseConnector shared] setPurchaseRevenueDelegate:_AppsFlyerPurchasedelegate];
    }

    const void _setAutoLogPurchaseRevenue(int option) {
           [[PurchaseConnector shared] setAutoLogPurchaseRevenue:option];

    }

    const void _init(const char* objectName) {
        if (_AppsFlyerPurchasedelegate == nil) {
            _AppsFlyerPurchasedelegate = [[AppsFlyerPurchaseiOSWrapper alloc] init];
        }
        onPurchaseValidationObjectName = stringFromChar(objectName);
    }
}

@implementation AppsFlyerPurchaseiOSWrapper

- (void)didReceivePurchaseRevenueValidationInfo:(NSDictionary *)validationInfo error:(NSError *)error {
    if (error != nil) {
        unityCallBack(onPurchaseValidationObjectName, PURCHASE_REVENUE_ERROR_CALLBACK, [[error localizedDescription] UTF8String]);
    } else {
        unityCallBack(onPurchaseValidationObjectName, PURCHASE_REVENUE_VALIDATION_CALLBACK, stringFromdictionary(validationInfo));
    }
}


@end
