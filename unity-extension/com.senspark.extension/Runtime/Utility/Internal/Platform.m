#import <Foundation/Foundation.h>
#include <string.h>

extern char* getBundleVersion() {
    NSString* bundleVersion = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleVersion"];
    return strdup([bundleVersion UTF8String]);
}