import type { ReactNode } from "react";

// Replaces the shell's top bar on a phone for routes marked `subpage` (see router.ts).
export function PhoneSubpageBar({ children }: { children: ReactNode }) {
  return (
    <header className="flex h-14 items-center gap-1 border-b px-1 md:hidden">
      {children}
    </header>
  );
}
