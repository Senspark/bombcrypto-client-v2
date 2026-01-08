//  AppsFlyerPurchaseiOSWarpper.h
//  Unity-iPhone


#import "../../../Plugins/iOS/AFUnityUtils.mm"
#import "UnityAppController.h"
#if __has_include(<PurchaseConnector/PurchaseConnector.h>)
#import  <PurchaseConnector/PurchaseConnector.h>
#else
#import "PurchaseConnector.h"
#endif



@interface AppsFlyerPurchaseiOSWrapper : NSObject <AppsFlyerPurchaseRevenueDelegate>
@end

static AppsFlyerPurchaseiOSWrapper *_AppsFlyerPurchasedelegate;

static const char* PURCHASE_REVENUE_VALIDATION_CALLBACK = "didReceivePurchaseRevenueValidationInfo";
static const char* PURCHASE_REVENUE_ERROR_CALLBACK = "didReceivePurchaseRevenueError";

static NSString* onPurchaseValidationObjectName = @"";
