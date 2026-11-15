import { MoonIcon, SunIcon } from "@phosphor-icons/react";
import { useTranslation } from "react-i18next";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { SidebarMenuButton, useSidebar } from "@/components/ui/sidebar";
import { useTheme } from "@/components/theme-provider";

// shadcn's ModeToggle (https://ui.shadcn.com/docs/dark-mode/vite) as a sidebar item.
export function ModeToggle() {
  const { t } = useTranslation();
  const { setTheme } = useTheme();
  const { isMobile } = useSidebar();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={<SidebarMenuButton tooltip={t("shell.theme.label")} />}
      >
        <SunIcon className="dark:hidden" />
        <MoonIcon className="hidden dark:block" />
        <span>{t("shell.theme.label")}</span>
      </DropdownMenuTrigger>
      <DropdownMenuContent
        side={isMobile ? "top" : "right"}
        align="end"
        className="min-w-40"
      >
        <DropdownMenuItem onClick={() => setTheme("light")}>
          {t("shell.theme.light")}
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => setTheme("dark")}>
          {t("shell.theme.dark")}
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => setTheme("system")}>
          {t("shell.theme.system")}
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
