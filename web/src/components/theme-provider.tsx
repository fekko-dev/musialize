import { createContext, useContext, useEffect, useState } from "react";

// shadcn's ThemeProvider for Vite (https://ui.shadcn.com/docs/dark-mode/vite), plus following
// the device while the page is open and surviving blocked storage or a bad stored value.
// index.html applies the same rules before first paint.
const themes = ["dark", "light", "system"] as const;
type Theme = (typeof themes)[number];

type ThemeProviderProps = {
  children: React.ReactNode;
  defaultTheme?: Theme;
  storageKey?: string;
};

type ThemeProviderState = {
  theme: Theme;
  setTheme: (theme: Theme) => void;
};

const ThemeProviderContext = createContext<ThemeProviderState>({
  theme: "system",
  setTheme: () => null,
});

export function ThemeProvider({
  children,
  defaultTheme = "system",
  storageKey = "vite-ui-theme",
}: ThemeProviderProps) {
  const [theme, setTheme] = useState<Theme>(() => {
    const stored = readStorage(storageKey);
    return themes.find((known) => known === stored) ?? defaultTheme;
  });

  useEffect(() => {
    const root = window.document.documentElement;
    const apply = (resolved: "dark" | "light") => {
      root.classList.remove("light", "dark");
      root.classList.add(resolved);
    };

    if (theme !== "system") {
      apply(theme);
      return;
    }

    const device = window.matchMedia("(prefers-color-scheme: dark)");
    const applyDevice = () => apply(device.matches ? "dark" : "light");
    applyDevice();
    // Not in the guide, which only reads the device's setting once.
    device.addEventListener("change", applyDevice);
    return () => device.removeEventListener("change", applyDevice);
  }, [theme]);

  return (
    <ThemeProviderContext
      value={{
        theme,
        setTheme: (theme: Theme) => {
          writeStorage(storageKey, theme);
          setTheme(theme);
        },
      }}
    >
      {children}
    </ThemeProviderContext>
  );
}

export function useTheme() {
  return useContext(ThemeProviderContext);
}

// Some private modes throw on any storage access; the theme then lasts for this visit only.
function readStorage(key: string) {
  try {
    return localStorage.getItem(key);
  } catch {
    return null;
  }
}

function writeStorage(key: string, value: string) {
  try {
    localStorage.setItem(key, value);
  } catch {
    // See readStorage.
  }
}
