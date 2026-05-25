import type {ReactNode} from 'react';

type AsciinemaPlayerProps = {
  src: string;
  idleTimeLimit?: number;
  poster?: string;
};

export default function AsciinemaPlayer({src}: AsciinemaPlayerProps): ReactNode {
  return (
    <div
      style={{
        padding: '1rem',
        margin: '1rem 0',
        border: '1px dashed var(--ifm-color-emphasis-300)',
        borderRadius: '8px',
        fontFamily: 'var(--ifm-font-family-monospace)',
        fontSize: '0.9rem',
        color: 'var(--ifm-color-emphasis-700)',
      }}>
      Terminal recording placeholder — <code>{src}</code> not yet hosted.
    </div>
  );
}
