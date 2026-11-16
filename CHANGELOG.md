# Changelog

## [0.2.0](https://github.com/fekko-dev/musialize/compare/v0.1.0...v0.2.0) (2026-11-16)


### Features

* list the Suggestions on the Overview ([a6f4404](https://github.com/fekko-dev/musialize/commit/a6f4404c475c32659765259a8f92ba7584518266))
* seed a demo Group with three Suggestions ([1cf16c8](https://github.com/fekko-dev/musialize/commit/1cf16c8bd2b19018cd4d027a096437a053ea1311))
* serve the Group's Suggestions from /api/songs ([6c437a1](https://github.com/fekko-dev/musialize/commit/6c437a1317c1e633dc411dd6073cce41144e2404))
* store Groups and their Songs in PostgreSQL ([c8a6d7a](https://github.com/fekko-dev/musialize/commit/c8a6d7a2ee780b26833a3500892f699cf731b96e))
* **web:** follow the device's dark mode, with a theme menu ([c8d44d5](https://github.com/fekko-dev/musialize/commit/c8d44d54bebd7035d6649be019008216671ea712))
* **web:** show the app in a sidebar shell ([32c628f](https://github.com/fekko-dev/musialize/commit/32c628f4d7c9fb3047bf44e60e5e076408bd0855)), closes [#54](https://github.com/fekko-dev/musialize/issues/54)
* **web:** show the app in a sidebar shell, with dark mode ([ebd0b50](https://github.com/fekko-dev/musialize/commit/ebd0b50e2211bf027b5382e9cb73a04479bee161))

## 0.1.0 (2026-11-14)


### Features

* /health checks the database ([07615b5](https://github.com/fekko-dev/musialize/commit/07615b5f6c82b950bfb409aa4de15c9d06fbdd5f))
* /health reports the latest migration and is unhealthy while the database isn't ready ([0f22657](https://github.com/fekko-dev/musialize/commit/0f2265718b25faff073b88bb988f79e74d9fae67)), closes [#11](https://github.com/fekko-dev/musialize/issues/11)
* add EF Core persistence and a local PostgreSQL that pnpm dev migrates ([68b8665](https://github.com/fekko-dev/musialize/commit/68b8665529413835378fe6d66e8b0dc89e2cd0f2)), closes [#11](https://github.com/fekko-dev/musialize/issues/11)
* add the database role setup for the migrator and the app ([31924fc](https://github.com/fekko-dev/musialize/commit/31924fc009e18cd27e661f248d9c1d2e1e1c1b1a))
* add the forced-command script that runs the migration step ([0847e49](https://github.com/fekko-dev/musialize/commit/0847e491723a1d3c5c5b1213979c64f2d6ddd797))
* add the forced-command script that runs the migration step ([d7e694b](https://github.com/fekko-dev/musialize/commit/d7e694b0ffc840d9c2f3dcf5ab64913b8a25fa7d))
* answer every API error with ProblemDetails ([9e2e262](https://github.com/fekko-dev/musialize/commit/9e2e2624eb99b6995cd2fff932a58f442e50facc))
* **api:** add the backend projects and a typed GET /health ([1d59fe7](https://github.com/fekko-dev/musialize/commit/1d59fe764479f2f28c1e8e516541bb1a80d30c7d)), closes [#7](https://github.com/fekko-dev/musialize/issues/7)
* branding and installing ([e053af8](https://github.com/fekko-dev/musialize/commit/e053af80f3a309883f4624d5f8d5a6d78cd8fc80))
* build the web app manifest from the Branding settings ([c328061](https://github.com/fekko-dev/musialize/commit/c328061c5d2ea57b1d1d210a06c284ecd72c05ed))
* errors, validation and logging ([e9c190d](https://github.com/fekko-dev/musialize/commit/e9c190d3d509a8b0390c6e7775e89f163ba66474))
* log a crash while the app starts or runs ([4570a1c](https://github.com/fekko-dev/musialize/commit/4570a1ceb503c83ebaa9edb67bee995b7fa222f8))
* production image with end-to-end tests ([e6b045b](https://github.com/fekko-dev/musialize/commit/e6b045b8fa4cb5fd8fe1d9e1d7821def6d3e81f9))
* refuse empty branding names and invalid theme colours ([f7bd699](https://github.com/fekko-dev/musialize/commit/f7bd69923af2f734c170f8c78dd3381a3d3bf0cc))
* reject invalid input before an endpoint runs ([873039f](https://github.com/fekko-dev/musialize/commit/873039f541a1c633c76522df09b73cf6bfb42952))
* serve the Group's name from the Branding settings ([dfcb827](https://github.com/fekko-dev/musialize/commit/dfcb827bf0323dc8043abc8faa34ab9208ea9171))
* serve the logo and icons from the branding folder ([334bee8](https://github.com/fekko-dev/musialize/commit/334bee8b6f05673634d63a2dfcb595b8f1d0e2bc))
* show the Group's logo and name in the header and the tab ([6f84b50](https://github.com/fekko-dev/musialize/commit/6f84b50a17ebddee902eef5c0649be76fcdbcb08))
* the API serves the built SPA and falls back to index.html ([5447ae2](https://github.com/fekko-dev/musialize/commit/5447ae24867178ea4d8b29b38ff90eb85f3718cd))
* the health card shows the migration and an unhealthy server ([cbf40c0](https://github.com/fekko-dev/musialize/commit/cbf40c0fed69c8866e1c3f6b0853677db77a56e4)), closes [#11](https://github.com/fekko-dev/musialize/issues/11)
* the health page runs locally ([8db46cb](https://github.com/fekko-dev/musialize/commit/8db46cbdc564b55839a2578b8d7ee9319a8d4f2a))
* **web:** add typed translations with react-i18next ([60b5130](https://github.com/fekko-dev/musialize/commit/60b51308a6ea676ac9761369282164e4149d0745))
* **web:** component kit, theming, i18n and web structure ([49e49e1](https://github.com/fekko-dev/musialize/commit/49e49e17766af260dbd23a11edf12dc0e36cce16))
* **web:** rebuild the health card with the shadcn Card and translation keys ([89ec6bd](https://github.com/fekko-dev/musialize/commit/89ec6bd90488831eb1286e8b3c567aaea5eb8c4a)), closes [#13](https://github.com/fekko-dev/musialize/issues/13)
* **web:** show the health status and version on the home page ([92e6c6c](https://github.com/fekko-dev/musialize/commit/92e6c6cfe9c784aa3d5b5fae56685999f8ededd0)), closes [#7](https://github.com/fekko-dev/musialize/issues/7)
* write logs through Serilog ([3e4747a](https://github.com/fekko-dev/musialize/commit/3e4747aa0e7ba23d7a47b6500f4719c4a93092b7))


### Bug Fixes

* **ci:** keep the Dokploy host out of a failed deploy's log ([75ee9b5](https://github.com/fekko-dev/musialize/commit/75ee9b5202d8c0af0ae5dfc1fefbfbe3c7ed84d2)), closes [#16](https://github.com/fekko-dev/musialize/issues/16)
* **ci:** release only after CI for the newest commit on main ([5f559bf](https://github.com/fekko-dev/musialize/commit/5f559bfff3580bb993ff0f3555abc678bb8d76f2))
* run the migration bundle as Production in the image ([05fd2ff](https://github.com/fekko-dev/musialize/commit/05fd2ff1198ccc68a1018f235a2e46eba043c8ec))
* say the theme colour must be a six-digit hex colour ([33fe570](https://github.com/fekko-dev/musialize/commit/33fe570d83292ab6795a522739404e375ef7c911))
