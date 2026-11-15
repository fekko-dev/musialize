import {
  CaretDoubleLeftIcon,
  CaretDoubleRightIcon,
  ListIcon,
  SparkleIcon,
} from "@phosphor-icons/react";
import { Link, useMatchRoute } from "@tanstack/react-router";
import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarRail,
  useSidebar,
} from "@/components/ui/sidebar";
import { TooltipProvider } from "@/components/ui/tooltip";

const navItems = [
  { to: "/", label: "shell.suggestions", icon: SparkleIcon },
] as const;

type AppLayoutProps = {
  // The deployment's App name, while it loads undefined.
  appName: string | undefined;
  children: ReactNode;
};

// The layout sets no width for the page: each page picks its own.
export function AppLayout({ appName, children }: AppLayoutProps) {
  return (
    <TooltipProvider>
      <SidebarProvider>
        <AppSidebar appName={appName} />
        <SidebarInset>
          <PhoneTopBar appName={appName} />
          {children}
        </SidebarInset>
      </SidebarProvider>
    </TooltipProvider>
  );
}

function AppSidebar({ appName }: { appName: string | undefined }) {
  const { t } = useTranslation();
  const matchRoute = useMatchRoute();
  const { isMobile, setOpenMobile } = useSidebar();

  return (
    <Sidebar variant="inset" collapsible="icon">
      <SidebarHeader>
        <Branding appName={appName} />
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <nav aria-label={t("shell.navigation")}>
            <SidebarMenu>
              {navItems.map((item) => (
                <SidebarMenuItem key={item.to}>
                  <SidebarMenuButton
                    render={
                      <Link
                        to={item.to}
                        // Closes the drawer. On the Link, not the button: Link prevents the
                        // default click, which skips the handlers merged in after it.
                        onClick={() => isMobile && setOpenMobile(false)}
                      />
                    }
                    isActive={!!matchRoute({ to: item.to })}
                    tooltip={t(item.label)}
                  >
                    <item.icon />
                    <span>{t(item.label)}</span>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </nav>
        </SidebarGroup>
      </SidebarContent>
      <SidebarFooter>
        <SidebarMenu>
          <CollapseButton />
        </SidebarMenu>
      </SidebarFooter>
      <SidebarRail />
    </Sidebar>
  );
}

function Branding({ appName }: { appName: string | undefined }) {
  return (
    <div className="flex h-12 items-center gap-2 px-2 group-data-[collapsible=icon]:px-0">
      {/* Decorative: the name next to it says the same. */}
      <img src="/branding/logo.svg" alt="" className="size-8 shrink-0" />
      {appName && (
        <span className="truncate font-heading text-base font-semibold group-data-[collapsible=icon]:hidden">
          {appName}
        </span>
      )}
    </div>
  );
}

// Desktop only: the phone's drawer closes by tapping beside it.
function CollapseButton() {
  const { t } = useTranslation();
  const { state, toggleSidebar, isMobile } = useSidebar();

  if (isMobile) {
    return null;
  }

  const collapsed = state === "collapsed";
  return (
    <SidebarMenuItem>
      <SidebarMenuButton onClick={toggleSidebar} tooltip={t("shell.expand")}>
        {collapsed ? <CaretDoubleRightIcon /> : <CaretDoubleLeftIcon />}
        <span>{collapsed ? t("shell.expand") : t("shell.collapse")}</span>
      </SidebarMenuButton>
    </SidebarMenuItem>
  );
}

function PhoneTopBar({ appName }: { appName: string | undefined }) {
  const { t } = useTranslation();
  const { toggleSidebar } = useSidebar();

  return (
    <header className="flex h-14 items-center gap-2 border-b px-2 md:hidden">
      <Button
        variant="ghost"
        size="icon"
        aria-label={t("shell.openMenu")}
        onClick={toggleSidebar}
      >
        <ListIcon className="size-5" />
      </Button>
      {/* Decorative: the name next to it says the same. */}
      <img src="/branding/logo.svg" alt="" className="size-7 shrink-0" />
      {appName && (
        <span className="min-w-0 truncate font-heading text-lg font-semibold">
          {appName}
        </span>
      )}
    </header>
  );
}
