import { CSSProperties } from 'react';
import gameBorder from '../assets/game_border.png';
import fullratio_button from '../assets/fullratio_button.png';
import fullscreen_button from '../assets/fullscreen_button.png';

export const maxWidth = 1000;
export const maxHeight = 620;

export const getContainerStyleDefault = (zoom: number): CSSProperties => ({
  display: 'flex',
  justifyContent: 'center',
  alignItems: 'center',
  width: '100%',
  height: undefined,
  position: 'relative',
  transform: `scale(${zoom})`,
  paddingTop: '50px',
});

export const getContainerStyleFullRatio = (zoom: number): CSSProperties => ({
  display: 'flex',
  justifyContent: 'center',
  alignItems: 'center',
  height: '100%',
  position: 'fixed',
  top: '50%',
  left: '50%',
  transform: `translate(-50%, -50%) scale(${zoom})`,
});

export const iframeContainerStyleDefault: CSSProperties = {
  position: 'relative',
  width: maxWidth,
  height: maxHeight,
  justifyContent: 'center',
  alignItems: 'center',
  padding: '15px',
};

export const iframeContainerStyleFullRatio: CSSProperties = {
  position: 'relative',
  width: maxWidth,
  height: maxHeight,
  justifyContent: 'center',
  alignItems: 'center',
};

export const wrapperImg: CSSProperties = {
  backgroundImage: `url(${gameBorder})`,
  backgroundRepeat: 'no-repeat',
  backgroundSize: '100% 100%',
  backgroundPosition: 'top',
  pointerEvents: 'none',
  cursor: 'pointer',
  position: 'absolute',
  top: 0,
  left: 0,
  width: '100%',
  height: '100%',
  maxWidth: '1000px',
  maxHeight: '629px',
  zIndex: 1,
  display: 'flex',
};

export const fullRatioButton: CSSProperties = {
  width: '38px',
  height: '38px',
  backgroundImage: `url(${fullratio_button})`,
  cursor: 'pointer',
  position: 'absolute',
  top: '100%',
  left: '88%',
  backgroundRepeat: 'no-repeat',
  overflow: 'visible',
  display: 'flex',
  pointerEvents: 'auto',
  backgroundSize: 'contain',
};

export const fullScreenButton: CSSProperties = {
  width: '38px',
  height: '38px',
  backgroundImage: `url(${fullscreen_button})`,
  cursor: 'pointer',
  position: 'absolute',
  top: '100%',
  left: '93%',
  backgroundRepeat: 'no-repeat',
  overflow: 'visible',
  display: 'flex',
  pointerEvents: 'auto',
};
