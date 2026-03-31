import { useEffect, useRef } from 'react';
import { BrowserMultiFormatReader } from '@zxing/browser';

interface QrScannerProps {
  onResult: (text: string) => void;
  onError?: (error: Error) => void;
}

export function QrScanner({ onResult, onError }: QrScannerProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const readerRef = useRef<BrowserMultiFormatReader | null>(null);

  useEffect(() => {
    const reader = new BrowserMultiFormatReader();
    readerRef.current = reader;

    reader
      .decodeFromVideoDevice(undefined, videoRef.current!, (result, error) => {
        if (result) {
          onResult(result.getText());
        } else if (error && onError) {
          onError(error as Error);
        }
      })
      .catch((err) => onError?.(err));

    return () => {
      BrowserMultiFormatReader.releaseAllStreams();
    };
  }, [onResult, onError]);

  return (
    <video
      ref={videoRef}
      style={{ width: '100%', maxWidth: 400, borderRadius: 8 }}
      autoPlay
      muted
      playsInline
    />
  );
}
