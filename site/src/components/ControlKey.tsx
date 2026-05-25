import {useEffect, useState, type ReactNode} from 'react';

export default function ControlKey(): ReactNode {
  const [isMac, setIsMac] = useState(false);

  useEffect(() => {
    if (typeof navigator !== 'undefined') {
      setIsMac(/Mac|iPhone|iPad|iPod/i.test(navigator.platform));
    }
  }, []);

  return <kbd>{isMac ? '⌘' : 'Ctrl'}</kbd>;
}
