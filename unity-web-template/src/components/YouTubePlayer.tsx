import { useEffect, useRef, useState } from 'react';
import icon_muted from "@assets/images/icon_muted.png";
import icon_unmuted from "@assets/images/icon_unmuted.png";

// Extend the window object to include the YouTube API callback
declare global {
  interface Window {
    onYouTubeIframeAPIReady: () => void;
    YT: any;
  }
}

interface YTPlayer {
  mute: () => void;
  unMute: () => void;
  isMuted: () => boolean;
}

interface YouTubePlayerProps {
  style?: React.CSSProperties;
}

const YouTubePlayer: React.FC<YouTubePlayerProps> = ({ style }) => {
  const playerRef = useRef(null);
  const [player, setPlayer] = useState<YTPlayer | null>(null);
  const [muted, setMuted] = useState(true);

  useEffect(() => {
    // Check if script is already loaded
    if (!window.YT) {
      const existingScript = document.querySelector('script[src="https://www.youtube.com/iframe_api"]');
      if (!existingScript) {
        const tag = document.createElement('script');
        tag.src = 'https://www.youtube.com/iframe_api';
        document.body.appendChild(tag);
      }

      // This is safe to assign again — only the first one matters
      window.onYouTubeIframeAPIReady = () => {
        new window.YT.Player(playerRef.current, {
          videoId: 'FWW1P81hWME',
          playerVars: {
            autoplay: 1,
            loop: 1,
            playlist: 'FWW1P81hWME',
            controls: 0,
            mute: 1,
            modestbranding: 1,
            rel: 0,
            showinfo: 0,
          },
          events: {
            onReady: (event: any) => {
              const yt = event.target;
              yt.mute();
              yt.playVideo();
              setPlayer(yt);
            },
          },
        });
      };
    } else {
      // If YT is already available, initialize immediately
      new window.YT.Player(playerRef.current, {
        videoId: 'FWW1P81hWME',
        playerVars: {
          autoplay: 1,
          loop: 1,
          playlist: 'FWW1P81hWME',
          controls: 0,
          mute: 1,
          modestbranding: 1,
          rel: 0,
          showinfo: 0,
        },
        events: {
          onReady: (event: any) => {
            const yt = event.target;
            yt.mute();
            yt.playVideo();
            setPlayer(yt);
          },
        },
      });
    }
  }, []);


  const toggleMute = () => {
    if (!player) return;
    if (player.isMuted()) {
      player.unMute();
      setMuted(false);
    } else {
      player.mute();
      setMuted(true);
    }
  };

  return (
    <>
      <div ref={playerRef} style={style} />
      <button
        onClick={toggleMute}
        disabled={!player}
        style={{
          position: 'absolute',
          top: 10,
          right: 10,
          zIndex: 0,
          background: 'transparent',
          border: 'none',
          padding: 0,
          pointerEvents: 'auto',
        }}
      >
        <img
          src={muted ? icon_muted : icon_unmuted}
          alt={muted ? 'Muted' : 'Unmuted'}
          style={{ width: 50, height: 50 }}
        />
    </button>
    </>
  );
};

export default YouTubePlayer;
