namespace PvpMode.Utils {
    public static class Ellipsis {
        public static string EllipsisAddress(string address) {
            if (address == null) {
                return "";
            }
            if (address.Length <= 10) {
                return address;
            }
            var pre = address[..4];
            var suf = address.Substring(address.Length - 4, 4);
            return pre + "..." + suf;
        }
    }
}