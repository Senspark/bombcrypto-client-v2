import {EnvConfig} from "./EnvConfig.ts";

export default class SECRET {
    SIGN_SECRET = () => EnvConfig.signSecret();

    SIGN_PADDING = () => EnvConfig.signPadding();

    LOCAL_SECRET = () => EnvConfig.localSecret();

    LOCAL_IV = () => EnvConfig.localIv();

    PERMUTATION_ORDER_32 = () => EnvConfig.permutationOrder32();

    APPEND_BYTES = EnvConfig.appendBytes();
}
