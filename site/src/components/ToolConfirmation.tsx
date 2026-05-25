import type {ReactNode} from 'react';
import Admonition from '@theme/Admonition';

export default function ToolConfirmation(): ReactNode {
  return (
    <Admonition type="warning" title="Tool execution">
      Some commands described in this section may execute third-party CLIs. Inspect what is being
      installed and run before approving any prompts in your environment.
    </Admonition>
  );
}
