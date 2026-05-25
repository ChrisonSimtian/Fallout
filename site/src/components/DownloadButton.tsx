import type {ReactNode} from 'react';
import Link from '@docusaurus/Link';

type DownloadButtonProps = {
  url: string | null | undefined;
};

export default function DownloadButton({url}: DownloadButtonProps): ReactNode {
  if (!url) {
    return (
      <span className="button button--secondary button--disabled" aria-disabled="true">
        Download (coming soon)
      </span>
    );
  }

  return (
    <Link className="button button--primary" to={url}>
      Download
    </Link>
  );
}
