import { useEffect, useCallback } from 'react';
import { useUnityContext } from 'react-unity-webgl';
import { UnityInstance } from 'react-unity-webgl/declarations/unity-instance';
import { EnvConfig } from '../services/EnvConfig';
import { unityService } from '../services/GlobalServices';

interface UnitySetupConfig {
  unityFolder: string;
}

export interface UnitySetupResult {
  unityProvider: ReturnType<typeof useUnityContext>['unityProvider'];
  UNSAFE__unityInstance: UnityInstance | undefined;
  getUnityUrls: () => {
    loaderUrl: string;
    dataUrl: string;
    frameworkUrl: string;
    codeUrl: string;
    streamingAssetsUrl: string;
  };
}

/**
 * Custom hook for setting up Unity context and handling Unity related functionality
 */
export function useUnitySetup({ unityFolder }: UnitySetupConfig): UnitySetupResult {
  const getDataFile = () => {
    // Get the data file URL extension based on browser capabilities
    return EnvConfig.dataExtension() || '/webgl.data.br';
    
    /* Commented out implementation for potential future use
    // Detect if browser supports ASTC texture compression
    try {
      const canvas = document.createElement('canvas');
      const gl = canvas.getContext('webgl');
      const gl2 = canvas.getContext('webgl2');
      if (
        (gl && gl.getExtension('WEBGL_compressed_texture_astc')) || 
        (gl2 && gl2.getExtension('WEBGL_compressed_texture_astc'))
      ) {
        return EnvConfig.mobileDataExtension() || '/mobile.data.br';
      }
      return EnvConfig.dataExtension() || '/webgl.data.br';
    } catch (e) {
      console.error('Error checking for compressed texture: ', e);
      return EnvConfig.dataExtension() || '/webgl.data.br';
    }
    */
  };

  const getUnityUrls = useCallback(() => {
    const loaderUrlExtension = EnvConfig.loaderExtension() || '/webgl.loader.js';
    const dataUrlExtension = getDataFile();
    const frameworkUrlExtension = EnvConfig.frameworkExtension() || '/webgl.js.br';
    const codeUrlExtension = EnvConfig.codeExtension() || '/webgl.wasm.br';

    return {
      loaderUrl: `${unityFolder}${loaderUrlExtension}`,
      dataUrl: `${unityFolder}${dataUrlExtension}`,
      frameworkUrl: `${unityFolder}${frameworkUrlExtension}`,
      codeUrl: `${unityFolder}${codeUrlExtension}`,
      streamingAssetsUrl: `${unityFolder}/StreamingAssets`,
    };
  }, [unityFolder]);

  const unityUrls = getUnityUrls();
  
  const { unityProvider, UNSAFE__unityInstance: rawUnityInstance } = useUnityContext({
      loaderUrl: unityUrls.loaderUrl,
      dataUrl: unityUrls.dataUrl,
      frameworkUrl: unityUrls.frameworkUrl,
      codeUrl: unityUrls.codeUrl,
      streamingAssetsUrl: unityUrls.streamingAssetsUrl,
  });

  const UNSAFE__unityInstance: UnityInstance | undefined = rawUnityInstance || undefined;

  useEffect(() => {
    if (UNSAFE__unityInstance) {
      window.unityInstance = UNSAFE__unityInstance;
      unityService.setUnityInstance(UNSAFE__unityInstance);
    }
  }, [UNSAFE__unityInstance]);

  return {
    unityProvider,
    UNSAFE__unityInstance,
    getUnityUrls,
  };
}
