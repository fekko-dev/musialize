# ASP.NET Core 10 runtime base image

Research date: 2026-11-11. Sources are the `dotnet/dotnet-docker` repo (READMEs, `documentation/`, Dockerfiles, announcements), the MCR registry API, `dotnet/core` release notes and release metadata, Microsoft Learn (EF Core, Azure Linux), Npgsql's docs and issue tracker, Canonical's and Alpine's release pages, and the published images themselves (pulled and inspected with `docker run`). Points that a source does not state, or that I inferred, are marked ⚠️.

Context: Musialize ships one production image: the ASP.NET Core 10 API, the built SPA, and an EF Core migration bundle. Dokploy runs it on Docker Swarm on one Hetzner server. Swarm's health check runs `curl -fsS http://localhost:8080/health` inside the container ([deployment](../deployment.md#application)), and CI runs `./efbundle` from the same image in a one-off container ([ADR-0009](../adr/0009-migration-bundles-as-a-separate-deploy-step.md)). Images are built once and promoted by tag ([ADR-0004](../adr/0004-promote-images-from-main-by-release-tag.md)). Issue #14 asked for "the Debian ASP.NET 10 runtime image". There is no such image, so the question is which `mcr.microsoft.com/dotnet/aspnet:10.0-*` image to use instead.

## 1. Summary and recommendation

**Use `mcr.microsoft.com/dotnet/aspnet:10.0-noble` (Ubuntu 24.04), add `curl` with apt, and switch to `USER $APP_UID`.** It is the image `10.0` already points to, but name the OS explicitly so the base can't change under us. Reasons:

1. **Its support covers .NET 10 completely.** .NET 10 ends on 2028-11-14 ([release metadata](https://builds.dotnet.microsoft.com/dotnet/release-metadata/releases-index.json)). Ubuntu 24.04 standard security maintenance runs until May 2029 ([Ubuntu release cycle](https://ubuntu.com/about/release-cycle)). The base OS never changes during .NET 10's life. The .NET team says it will not move `10.0` to Ubuntu 26.04 for exactly this reason ([dotnet-docker #7286](https://github.com/dotnet/dotnet-docker/discussions/7286)). Alpine and Azure Linux 3.0 both reach end of life before .NET 10 does (§3.3).
2. **Nothing to work around.** It uses glibc, ships ICU and tzdata, and the default SDK image (`sdk:10.0`, also noble) builds a `linux-x64` bundle that runs on it as is. Alpine needs invariant globalization (or ICU added), tzdata added, and a `linux-musl-x64` bundle (§3.5, §3.6).
3. **It is the default.** It is the `10.0` tag, so it is the image most .NET 10 users run and the one Microsoft's docs and samples assume ([dotnet-docker #6801](https://github.com/dotnet/dotnet-docker/discussions/6801)). That is the "boring" choice for a one-person project.
4. **Adding curl is one apt line and about 6.5 MB** (§3.4). The image has no curl or wget by default, which I checked.

What it costs: it is the largest of the full images apart from resolute, at about 98 MB compressed against 56 MB for Alpine (§3.2). On one server that pulls each tag once, this hardly matters.

**Runner-up: `10.0-alpine`.** It is about 40 MB smaller and has about 18 OS packages against noble's 97, so scanners report fewer CVEs. It loses on the extra steps (musl bundle, tzdata, invariant globalization) and on its moving OS version: the floating tag moves to a new Alpine release about every six months (§3.3). Choose it if image size or scanner noise becomes a real concern.

**Not recommended:** `10.0-resolute` (bigger, newer, gives nothing during .NET 10's life), `10.0-azurelinux3.0` (comes with curl, but its OS reaches end of life around summer 2027 ⚠️, so we would have to switch OS mid-life), and every chiseled/distroless variant (no shell, no package manager, no curl; §3.4).

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
# … restore, publish the API and the SPA …
# The default RID on this stage is linux-x64, which matches the runtime image.
RUN dotnet ef migrations bundle --target-runtime linux-x64 --output /app/efbundle # plus --project/--startup-project

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS final
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "Musialize.Api.dll"]
```

The files stay owned by root, which is fine: the `app` user only needs to read them. Port 8080 is already the default (`ASPNETCORE_HTTP_PORTS=8080`), so no `EXPOSE` or `ASPNETCORE_URLS` is needed (§3.1).

## 2. Comparison

Legend: ✅ yes, documented or checked · 🟡 partial / caveat · ❌ no

|                                      | `10.0-noble`              | `10.0-resolute`           | `10.0-alpine`                                    | `10.0-azurelinux3.0`      |
| ------------------------------------ | ------------------------- | ------------------------- | ------------------------------------------------ | ------------------------- |
| OS                                   | Ubuntu 24.04              | Ubuntu 26.04              | Alpine 3.24 (moves)                              | Azure Linux 3.0           |
| `10.0` default tag                   | ✅                        | ❌ opt-in                 | ❌                                               | ❌                        |
| OS supported until .NET 10 EOL       | ✅ May 2029               | ✅ May 2031               | 🟡 3.24 ends 2028-06-01; tag moves to new Alpine | ❌ ~summer 2027 ⚠️        |
| Compressed size, amd64               | 97.7 MB                   | 111.0 MB                  | 55.7 MB                                          | 103.3 MB                  |
| OS packages in image                 | 97                        | 92                        | 18                                               | 83                        |
| `curl` present                       | ❌ add with apt (+6.5 MB) | ❌ add with apt           | ❌ add with apk (+5.4 MB); busybox `wget` exists | ✅                        |
| libc / bundle RID                    | glibc / `linux-x64`       | glibc / `linux-x64`       | musl / `linux-musl-x64`                          | glibc / `linux-x64`       |
| ICU (culture-aware text)             | ✅ libicu74               | ✅ libicu78               | ❌ invariant mode                                | ✅                        |
| tzdata (`TimeZoneInfo`)              | ✅                        | ✅                        | ❌                                               | ✅                        |
| `libgssapi_krb5` (Npgsql 10 default) | ❌                        | ❌ ⚠️ not checked         | ❌                                               | ✅                        |
| Non-root `app` user, `APP_UID=1654`  | ✅ (not the default user) | ✅ (not the default user) | ✅ (not the default user)                        | ✅ (not the default user) |

Sizes are the sum of the compressed layers in each tag's amd64 manifest on MCR, read on 2026-11-11 (.NET 10.0.15). Package counts, curl, ICU, tzdata and libgssapi were checked in the pulled images. The curl increase is the uncompressed layer size from `docker history`.

## 3. Findings

### 3.1 What every candidate has in common

- **Tags.** For .NET 10 GA, `aspnet` has `noble`, `resolute`, `alpine` (with `alpine3.22`, `3.23` and `3.24` still listed) and `azurelinux3.0`, each with chiseled/distroless, `-extra` and `-composite` variants. Debian appears only as `10.0-preview-trixie-slim` and `10.0.0-preview.{3,4,5}-trixie-slim` ([MCR tag list](https://mcr.microsoft.com/v2/dotnet/aspnet/tags/list), [README.aspnet.md](https://github.com/dotnet/dotnet-docker/blob/main/README.aspnet.md)). `10.0` and `10.0.15` point to `noble` (same README). .NET 11 is out now, and its `latest`/`11.0` tags point to `resolute`.
- **Same .NET layer on every OS.** Each `aspnet` Dockerfile extracts the same `aspnetcore-runtime-10.0.15` tarball (the `musl` build for Alpine) on top of `runtime` ([noble](https://github.com/dotnet/dotnet-docker/blob/main/src/aspnet/10.0/noble/amd64/Dockerfile), [alpine3.24](https://github.com/dotnet/dotnet-docker/blob/main/src/aspnet/10.0/alpine3.24/amd64/Dockerfile), [azurelinux3.0](https://github.com/dotnet/dotnet-docker/blob/main/src/aspnet/10.0/azurelinux3.0/amd64/Dockerfile), [resolute](https://github.com/dotnet/dotnet-docker/blob/main/src/aspnet/10.0/resolute/amd64/Dockerfile)).
- **User and port.** Each `runtime-deps` image sets `APP_UID=1654` ("UID of the non-root user 'app'") and `ASPNETCORE_HTTP_PORTS=8080`, and creates the `app` user and group ([runtime-deps noble](https://github.com/dotnet/dotnet-docker/blob/main/src/runtime-deps/10.0/noble/amd64/Dockerfile)). None of them sets `USER`: `docker run … id` prints `uid=0(root)` on all four. So `USER $APP_UID` in our Dockerfile is required, not optional. Only the distroless images run as non-root by default ([distroless.md](https://github.com/dotnet/dotnet-docker/blob/main/documentation/distroless.md)).
- **Patching.** MCR images are rebuilt "within 12 hours of any updates to their base images", on every .NET release, for critical CVEs, and monthly, "typically on the second Tuesday" ([README, Image Update Policy](https://github.com/dotnet/dotnet-docker/blob/main/README.md)). All four tags were rebuilt on 2026-11-10. Because we promote one built image ([ADR-0004](../adr/0004-promote-images-from-main-by-release-tag.md)), base-image fixes only reach production when CI builds a new image. ⚠️ If `main` is quiet for weeks, production runs an old base. A scheduled CI build, or Dependabot's `docker` ecosystem to bump a digest-pinned `FROM`, would fix that. This is the same for every candidate. The repo has no `.github/dependabot.yml` yet.

### 3.2 Why there is no Debian image, and why Ubuntu is the default

- In 2024 the .NET team proposed moving the convenience tags to Ubuntu because Debian and .NET release cycles "align poorly". A new Debian ships only months before a .NET LTS, and its three years of full support end before that .NET LTS does. Ubuntu LTS ships about 1.5 years earlier and has five years of standard support ([dotnet-docker #5709](https://github.com/dotnet/dotnet-docker/discussions/5709)). That proposal still planned `10.0-trixie` tags.
- In July 2025 Microsoft went further: "Consider dropping Debian images for .NET 10+ … most users wanting a glibc image will be well-served by Ubuntu" ([dotnet-docker #6526](https://github.com/dotnet/dotnet-docker/issues/6526), closed 2025-07-14). The platform list for .NET 10 has only Ubuntu 24.04, Alpine 3.22 and Azure Linux 3.0 ([#6539](https://github.com/dotnet/dotnet-docker/discussions/6539)). The GA announcement says: "Debian container images will not be shipped for .NET 10" ([#6801](https://github.com/dotnet/dotnet-docker/discussions/6801)).
- Policy now: "Existing .NET Debian images will continue to receive base image updates, but new .NET images will not be added for future Debian versions" ([supported-platforms.md](https://github.com/dotnet/dotnet-docker/blob/main/documentation/supported-platforms.md)).
- .NET 10 itself still supports Debian 13 as a host OS ([supported-os.md](https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md)). Only the container images were dropped. A self-built Debian image is possible but would make us maintain the .NET layer ourselves (YAGNI).

### 3.3 OS lifetime against .NET 10 (ends 2028-11-14)

- **Ubuntu 24.04 (noble):** standard security maintenance until May 2029 ([Ubuntu release cycle](https://ubuntu.com/about/release-cycle)). "There are no plans to move the `10.0` tags from Ubuntu 24.04 (Noble) to 26.04 (Resolute). Resolute is opt-in", partly because 24.04 "is fully supported by Canonical for the entire duration of .NET 10's support lifecycle" ([#7286](https://github.com/dotnet/dotnet-docker/discussions/7286), .NET team member).
- **Ubuntu 26.04 (resolute):** standard support until May 2031 (same Ubuntu page). Added to .NET 10 under the rule "new images will be added to the latest .NET version and latest LTS .NET version" ([supported-platforms.md](https://github.com/dotnet/dotnet-docker/blob/main/documentation/supported-platforms.md)). The extra years only matter after .NET 10 has ended.
- **Alpine:** the main repository "is typically supported for 2 years". 3.24 was branched on 2026-06-09 and ends 2028-06-01; 3.23 ends 2027-11-01 ([Alpine releases](https://alpinelinux.org/releases/)). .NET publishes only for the latest Alpine and keeps the previous one for 3 months ([supported-platforms.md](https://github.com/dotnet/dotnet-docker/blob/main/documentation/supported-platforms.md)). The floating `10.0-alpine` tag moves to the new Alpine a month after its images appear ([supported-tags.md](https://github.com/dotnet/dotnet-docker/blob/main/documentation/supported-tags.md)). In 2026 that happened for 3.23 on 2026-01-14 and for 3.24 on 2026-08-11 ([#6929](https://github.com/dotnet/dotnet-docker/discussions/6929), [#7265](https://github.com/dotnet/dotnet-docker/discussions/7265)). So the OS is always in support, but it changes about twice a year. Pinned tags such as `10.0-alpine3.23` stop getting updates ([#7415](https://github.com/dotnet/dotnet-docker/discussions/7415)).
- **Azure Linux 3.0:** current Microsoft Learn pages give no end date. The lifecycle page says "Specific support durations and update policies will be published ahead of General Availability" of 4.0, which is in preview ([release cadence](https://learn.microsoft.com/en-us/azure/azure-linux/release-cadence-lifecycle)). A June 2025 Microsoft post said each major version is supported for 3 years after GA and that "Azure Linux 3.0 will reach EOL in Summer 2027" ([Tech Community](https://techcommunity.microsoft.com/blog/azuretoolsblog/-/4419582)). ⚠️ That date comes from a blog post, not a lifecycle page. Either way it falls before .NET 10's end. .NET 11 already has `azurelinux4.0` tags; .NET 10 doesn't yet ([README.aspnet.md](https://github.com/dotnet/dotnet-docker/blob/main/README.aspnet.md)). ⚠️ The policy implies `10.0-azurelinux4.0` will be added, but then we would have to change tags mid-life.

### 3.4 Getting curl into the image

- **noble / resolute:** `ubuntu:noble` has neither curl nor wget (checked). `apt-get install -y --no-install-recommends curl` installs curl 8.5.0 and adds 6.5 MB. Package fixes come from Ubuntu's security pocket, and the MCR base is rebuilt when `ubuntu:noble` changes (§3.1). ⚠️ Our own `apt-get install` only picks up new curl fixes when our image is rebuilt.
- **Alpine:** `RUN apk add --no-cache curl` installs curl 8.23.0 and adds 5.4 MB (checked). BusyBox `wget` already exists, but Dokploy's health check is set to curl ([deployment](../deployment.md#application)).
- **Azure Linux 3.0:** curl is already there (`/usr/bin/curl`, checked). The `aspnet` Dockerfile even uses it in its installer stage without installing it ([azurelinux3.0 Dockerfile](https://github.com/dotnet/dotnet-docker/blob/main/src/aspnet/10.0/azurelinux3.0/amd64/Dockerfile)). Other packages would be `tdnf install -y <pkg> && tdnf clean all`, as in [runtime-deps azurelinux3.0](https://github.com/dotnet/dotnet-docker/blob/main/src/runtime-deps/10.0/azurelinux3.0/amd64/Dockerfile).
- **Chiseled / distroless:** no shell, no package manager ([distroless.md](https://github.com/dotnet/dotnet-docker/blob/main/documentation/distroless.md)). Options, all worse for us:
  - Ubuntu Chiseled: since .NET 10 the images carry a Chisel manifest, and slices can be added with `chisel` in a build stage, then copied into `FROM scratch` ([ubuntu-chiseled.md](https://github.com/dotnet/dotnet-docker/blob/main/documentation/ubuntu-chiseled.md)). A `curl_bins` slice exists for 24.04 ([chisel-releases `curl.yaml`](https://github.com/canonical/chisel-releases/blob/ubuntu-24.04/slices/curl.yaml)). It works, but adds a stage, two pinned downloaded tools, and a custom root filesystem to maintain.
  - Azure Linux distroless: `tdnf --installroot` into a copy of the image, with an `fdupes` clean-up step ([azurelinux.md](https://github.com/dotnet/dotnet-docker/blob/main/documentation/azurelinux.md)). Same objection.
  - Copying a static curl binary in from a third-party image: one more supply-chain source to trust and patch. ⚠️ Not evaluated further.
  - A .NET health probe (for example `["CMD", "dotnet", "Musialize.Api.dll", "--health"]`): starts a whole runtime every 10 s and is extra code. ⚠️ Our idea, not a documented pattern. The requirement is curl.

### 3.5 Migration bundle

- `dotnet ef migrations bundle` has `--self-contained` ("Also bundle the .NET runtime …") and `--target-runtime`/`-r` ("The target runtime to bundle for") ([EF Core tools reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)). Without `-r`, "the output is an executable suitable for your target operating system", that is, the build machine ([Applying migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)).
- Framework-dependent is enough. The `aspnet` image has both the .NET and ASP.NET Core shared frameworks, and the API project is the startup project. Self-contained would only add size.
- **noble:** build in `sdk:10.0-noble` with `-r linux-x64`. It is the same libc and the same OS as the runtime image.
- **Alpine:** the bundle's native host must be built for `linux-musl-x64` (the .NET runtime itself ships a separate `linux-musl-x64` build; [aspnet alpine3.24 Dockerfile](https://github.com/dotnet/dotnet-docker/blob/main/src/aspnet/10.0/alpine3.24/amd64/Dockerfile)). ⚠️ I did not build a glibc bundle and run it on Alpine to see the exact failure; that it fails is inferred from the libc difference.
- The same docs say to set `ASPNETCORE_ENVIRONMENT=Production` both when building and when running the bundle, so it doesn't fall back to `Development` ([Applying migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)). This is the same on every image.

### 3.6 Globalization, time zones and Npgsql

- **ICU:** Ubuntu and Azure Linux images include ICU. Alpine `aspnet` and Ubuntu chiseled images don't, and enable invariant mode ([enable-globalization.md](https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md)). The Alpine `runtime-deps` Dockerfile sets `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true` ([runtime-deps alpine3.24](https://github.com/dotnet/dotnet-docker/blob/main/src/runtime-deps/10.0/alpine3.24/amd64/Dockerfile)). In invariant mode, culture-aware sorting and formatting (for example Dutch names or dates in a sign-in email) behave like the invariant culture. Adding ICU on Alpine means `apk add icu-data-full icu-libs` and turning invariant mode off (same sample), which takes back much of the size gain. ⚠️ I found no Npgsql doc that says it needs ICU. Microsoft only says `Microsoft.Data.SqlClient` does (same sample).
- **tzdata:** without it, `TimeZoneInfo.Local` is UTC and `FindSystemTimeZoneById` throws ([enable-globalization.md](https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md)). noble, resolute and Azure Linux install `tzdata` ([runtime-deps noble](https://github.com/dotnet/dotnet-docker/blob/main/src/runtime-deps/10.0/noble/amd64/Dockerfile)). Alpine doesn't: `/usr/share/zoneinfo/Europe/Amsterdam` is missing (checked). Npgsql's `Timezone` setting defaults to the `PGTZ` environment variable ([connection string parameters](https://www.npgsql.org/doc/connection-string-parameters.html)), so Npgsql doesn't need tzdata just to connect.
- **GSS encryption (all images except Azure Linux):** Npgsql 10 tries GSS session encryption by default (`GSS Encryption Mode=Prefer`). Without `libgssapi_krb5.so.2` it logs an error, which the docs call "harmless", and falls back. `GSS Encryption Mode=Disable` stops the attempt ([Npgsql security](https://www.npgsql.org/doc/security.html)). Since .NET 8, Microsoft's images no longer include Kerberos libraries ([npgsql #6360](https://github.com/npgsql/npgsql/issues/6360)). noble and Alpine lack it (checked). Open Npgsql issues report exception storms and hangs in this path on containers without Kerberos ([#6416](https://github.com/npgsql/npgsql/issues/6416), [#6524](https://github.com/npgsql/npgsql/issues/6524)). We use Npgsql EF Core 10.0.3. ⚠️ So add `GSS Encryption Mode=Disable` to both connection strings (app and migrator). PostgreSQL on `dokploy-network` doesn't use Kerberos. This doesn't depend on which image we pick, and installing `libgssapi-krb5-2` instead would only add packages we don't use.
- ⚠️ musl DNS: musl's resolver behaves differently from glibc's (for example with search domains). I did not test Alpine on Swarm's `dokploy-network`. It is one more reason not to pick Alpine without a test.

## 4. What would change the recommendation

- **Image size or scanner noise matters** (for example a registry or bandwidth limit, or a security review that counts CVEs): go to `10.0-alpine`, with `apk add curl tzdata`, the bundle built with `-r linux-musl-x64`, and a check that invariant globalization is acceptable.
- **We move to .NET 11 or 12:** .NET 11's default is already Ubuntu 26.04 ([README.aspnet.md](https://github.com/dotnet/dotnet-docker/blob/main/README.aspnet.md)). Switch the explicit `-noble` to `-resolute` then.
- **We want distroless hardening:** `10.0-noble-chiseled-extra` plus a `curl_bins` slice (§3.4), or a Dokploy health check that doesn't need curl. Not worth it now.

## 5. Flagged uncertainties

- ⚠️ Azure Linux 3.0's end date (summer 2027) comes from a 2025 Microsoft blog post. The lifecycle page on Learn gives no date.
- ⚠️ Whether and when `10.0-azurelinux4.0` will be published is inferred from policy, not announced.
- ⚠️ I did not run a glibc bundle on Alpine or test musl DNS on Swarm.
- ⚠️ `GSS Encryption Mode=Disable` is my recommendation based on Npgsql's docs and open issues. Npgsql doesn't advise it for containers in general.
- ⚠️ Base-image staleness between deploys follows from ADR-0004 plus MCR's rebuild policy. It isn't stated by any one source.
- Sizes and package counts are amd64 measurements of the 2026-11-10 builds. They change with every rebuild.

## 6. Sources

All checked 2026-11-11. GitHub data via `gh api`; images pulled from MCR and inspected with `docker run`.

- dotnet-docker: [README.md](https://github.com/dotnet/dotnet-docker/blob/main/README.md) · [README.aspnet.md](https://github.com/dotnet/dotnet-docker/blob/main/README.aspnet.md) · [supported-platforms.md](https://github.com/dotnet/dotnet-docker/blob/main/documentation/supported-platforms.md) · [supported-tags.md](https://github.com/dotnet/dotnet-docker/blob/main/documentation/supported-tags.md) · [distroless.md](https://github.com/dotnet/dotnet-docker/blob/main/documentation/distroless.md) · [ubuntu-chiseled.md](https://github.com/dotnet/dotnet-docker/blob/main/documentation/ubuntu-chiseled.md) · [azurelinux.md](https://github.com/dotnet/dotnet-docker/blob/main/documentation/azurelinux.md) · [enable-globalization.md](https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md) · Dockerfiles under `src/aspnet/10.0/` and `src/runtime-deps/10.0/` (`noble`, `resolute`, `alpine3.24`, `azurelinux3.0`) · discussions [#5709](https://github.com/dotnet/dotnet-docker/discussions/5709), [#6539](https://github.com/dotnet/dotnet-docker/discussions/6539), [#6801](https://github.com/dotnet/dotnet-docker/discussions/6801), [#6929](https://github.com/dotnet/dotnet-docker/discussions/6929), [#7265](https://github.com/dotnet/dotnet-docker/discussions/7265), [#7286](https://github.com/dotnet/dotnet-docker/discussions/7286), [#7415](https://github.com/dotnet/dotnet-docker/discussions/7415) · issue [#6526](https://github.com/dotnet/dotnet-docker/issues/6526)
- MCR: https://mcr.microsoft.com/v2/dotnet/aspnet/tags/list · `https://mcr.microsoft.com/v2/dotnet/aspnet/manifests/<tag>`
- .NET: [supported-os.md (10.0)](https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md) · [releases-index.json](https://builds.dotnet.microsoft.com/dotnet/release-metadata/releases-index.json)
- EF Core: [Applying migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying) · [EF Core tools reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- Npgsql: [security](https://www.npgsql.org/doc/security.html) · [connection string parameters](https://www.npgsql.org/doc/connection-string-parameters.html) · issues [#6360](https://github.com/npgsql/npgsql/issues/6360), [#6416](https://github.com/npgsql/npgsql/issues/6416), [#6467](https://github.com/npgsql/npgsql/issues/6467), [#6524](https://github.com/npgsql/npgsql/issues/6524)
- Distros: [Ubuntu release cycle](https://ubuntu.com/about/release-cycle) · [Alpine releases](https://alpinelinux.org/releases/) · [Azure Linux release cadence](https://learn.microsoft.com/en-us/azure/azure-linux/release-cadence-lifecycle) · [Azure Linux AKS support cycle](https://learn.microsoft.com/en-us/azure/azure-linux/aks-support-cycle) · [Microsoft Tech Community, Azure Linux 3.0 EOL](https://techcommunity.microsoft.com/blog/azuretoolsblog/-/4419582) · [canonical/chisel-releases `curl.yaml` (24.04)](https://github.com/canonical/chisel-releases/blob/ubuntu-24.04/slices/curl.yaml)
