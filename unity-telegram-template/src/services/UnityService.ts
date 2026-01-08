import {UnityInstance} from "react-unity-webgl/declarations/unity-instance";

export default class UnityService {
    private _unity: UnityInstance | null = null;

    setUnityInstance(unity: UnityInstance) {
        this._unity = unity;
    }

    quitUnity() {
        this._unity?.Quit();
        this._unity = null;
    }
    
    sendMessage(gameObjectName: string, methodName: string, parameter?: unknown) {
        this._unity?.SendMessage(gameObjectName, methodName, parameter);
    }
}