Unity Receipt (Android) Sample:
```json
{"Payload":"{\"json\":\"{\\\"orderId\\\":\\\"GPA.3347-2066-7600-94817\\\",\\\"packageName\\\":\\\"com.senspark.android.phom\\\",\\\"productId\\\":\\\"com.senspark.android.phom.iap.coins_pack_1\\\",\\\"purchaseTime\\\":1709029671487,\\\"purchaseState\\\":0,\\\"purchaseToken\\\":\\\"nnobkgignahicfdjjpdbohlb.AO-J1Oy66C_oT808YucKi8BdeMgI8C59SWuaG4U583E5H7AnuyFfwdEZD9cD07RBIS9HOa10TjiTKJMPhdmf76Xkr34M8k7Un9wxbpokasxInbEFU_Hfv08\\\",\\\"obfuscatedAccountId\\\":\\\"86b8de0a-018b-4712-8745-e7a05683f126\\\",\\\"quantity\\\":1,\\\"acknowledged\\\":false}\",\"signature\":\"Wobd+mhM2b/wIkrTA86J18Bo1+UP1Vc04Bu+2zuoyQ9QwELjUb3cbT+yiEMD7v/jtyG2r0OIE2zpEOVDv6bujdpN2VfiI8KNbLElv6uGoOCJZWyd3h/TsuE7TJ6k3W3Ksn+F0PWDLH9SZLL0Gsj+vCWD8Vu7U+X68lj5XGWlEqR0b/Go2ZX58RWagpc0T4haFu25TpfQ45jYTJhFWodADqJULyBxZpoecFe0cecXHkHqYdl9SflQAX9OcZ4m6txLvnsC21drbtT5ZD5eVG/ocy+KPtY+p6nnFAwc8ObgKWYruC1mkHA9lZavKyCnU5qkLID68bsBz0nRG91jE+5jDQ==\",\"skuDetails\":[\"{\\\"productId\\\":\\\"com.senspark.android.phom.iap.coins_pack_1\\\",\\\"type\\\":\\\"inapp\\\",\\\"title\\\":\\\"G\\u00f3i Xu 1 (Phom, Ta la)\\\",\\\"name\\\":\\\"G\\u00f3i Xu 1\\\",\\\"iconUrl\\\":\\\"https:\\\\/\\\\/lh3.googleusercontent.com\\\\/5yDKOUlMdb8jM8s4xR6bba1poWS0aVyLLEbjo3Xgqxe9OBy0l9KxAr3d9Ni3Mf-nHFo\\\",\\\"description\\\":\\\"G\\u00f3i xu 1\\\",\\\"price\\\":\\\"\\u20ab22,000\\\",\\\"price_amount_micros\\\":22000000000,\\\"price_currency_code\\\":\\\"VND\\\",\\\"skuDetailsToken\\\":\\\"AEuhp4K6jC0-0nwexzFSN-0eK7HnUBAih4VDxFQlj5deuoHXg_Gv2K6oCYh1agSmaY2M\\\"}\"]}","Store":"GooglePlay","TransactionID":"nnobkgignahicfdjjpdbohlb.AO-J1Oy66C_oT808YucKi8BdeMgI8C59SWuaG4U583E5H7AnuyFfwdEZD9cD07RBIS9HOa10TjiTKJMPhdmf76Xkr34M8k7Un9wxbpokasxInbEFU_Hfv08"}
```

Parsed:
```cs
public class Root
{
    public string Payload { get; set; }
    public string Store { get; set; }
    public string TransactionID { get; set; }
}

public class Payload
{
    public string json { get; set; }
    public string signature { get; set; }
    public List<string> skuDetails { get; set; }
}

public class Json
{
    public string orderId { get; set; }
    public string packageName { get; set; }
    public string productId { get; set; }
    public long purchaseTime { get; set; }
    public int purchaseState { get; set; }
    public string purchaseToken { get; set; }
    public string obfuscatedAccountId { get; set; }
    public int quantity { get; set; }
    public bool acknowledged { get; set; }
}

public class SkuDetails
{
    public string productId { get; set; }
    public string type { get; set; }
    public string title { get; set; }
    public string name { get; set; }
    public string iconUrl { get; set; }
    public string description { get; set; }
    public string price { get; set; }
    public long price_amount_micros { get; set; }
    public string price_currency_code { get; set; }
    public string skuDetailsToken { get; set; }
}
```

```cs
using Newtonsoft.Json;

// Parse the root JSON string
var root = JsonConvert.DeserializeObject<Root>(jsonString);

// Parse the 'Payload' JSON string
var payload = JsonConvert.DeserializeObject<Payload>(root.Payload);

// Parse the 'json' JSON string
var json = JsonConvert.DeserializeObject<Json>(payload.json);

// Parse each 'skuDetails' JSON string
var skuDetailsList = new List<SkuDetails>();
foreach (var skuDetailsString in payload.skuDetails)
{
    var skuDetails = JsonConvert.DeserializeObject<SkuDetails>(skuDetailsString);
    skuDetailsList.Add(skuDetails);
}
```