import { useState, useCallback, useEffect } from 'react';

interface UseResizeConfig {
  maxWidth: number;
  maxHeight: number;
  onResetZoom?: () => void;
}

interface UseResizeResult {
  zoom: number;
  isFullRatioMode: boolean;
  handleFullRatioClick: () => void;
  resetZoom: () => void;
}

/**
 * Custom hook to handle resizing logic for fullscreen and full ratio modes
 */
export function useResize({ maxWidth, maxHeight, onResetZoom }: UseResizeConfig): UseResizeResult {
  const [zoom, setZoom] = useState(1);
  const [isFullRatioMode, setIsFullRatioMode] = useState(false);

  // Create a debounce utility function
  const debounce = <T extends (...args: unknown[]) => void>(
    func: T, 
    wait: number
  ): (...args: Parameters<T>) => void => {
    let timeout: NodeJS.Timeout;
    return (...args: Parameters<T>) => {
      clearTimeout(timeout);
      timeout = setTimeout(() => func(...args), wait);
    };
  };
  
  // Handle window resize
  const handleResize = useCallback(() => {
    if (isFullRatioMode) {
      const containerWidth = maxWidth;
      const containerHeight = maxHeight;
      // Use innerWidth instead of outerWidth for more accurate resizing
      const windowWidth = window.innerWidth;
      const windowHeight = window.innerHeight;

      const zoomWidth = windowWidth / containerWidth;
      const zoomHeight = windowHeight / containerHeight;
      const zoomFactor = Math.min(zoomWidth, zoomHeight);
      setZoom(zoomFactor);
    }
  }, [isFullRatioMode, maxWidth, maxHeight]);

  const debouncedHandleResize = useCallback(
    debounce(handleResize, 100), 
    [handleResize]
  );
  
  // Reset zoom to default
  const resetZoom = useCallback(() => {
    setZoom(1);
    setIsFullRatioMode(false);
    if (onResetZoom) {
      onResetZoom();
    }
  }, [onResetZoom]);
  
  // Set full ratio mode
  const handleFullRatioClick = useCallback(() => {
    const containerWidth = maxWidth;
    const containerHeight = maxHeight;
    const windowWidth = window.innerWidth;
    const windowHeight = window.innerHeight;

    const zoomWidth = windowWidth / containerWidth;
    const zoomHeight = windowHeight / containerHeight;
    const zoomFactor = Math.min(zoomWidth, zoomHeight);
    
    setZoom(zoomFactor);
    setIsFullRatioMode(true);
  }, [maxWidth, maxHeight]);

  // Set up window resize listener
  useEffect(() => {
    if (isFullRatioMode) {
      window.addEventListener('resize', debouncedHandleResize);
    } else {
      window.removeEventListener('resize', debouncedHandleResize);
    }
    
    return () => {
      window.removeEventListener('resize', debouncedHandleResize);
    };
  }, [isFullRatioMode, debouncedHandleResize]);

  // Add escape key listener to exit full ratio mode
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && isFullRatioMode) {
        resetZoom();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [isFullRatioMode, resetZoom]);

  return {
    zoom,
    isFullRatioMode,
    handleFullRatioClick,
    resetZoom,
  };
}
