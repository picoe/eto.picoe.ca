---
title: "Versioning"
---

Eto.Forms uses [semantic versioning](https://semver.org): new features bump the minor version (2.13 → 2.14), and fixes bump the patch (2.13.0 → 2.13.1).

## Branches

All work happens on the `develop` branch, and releases are built from it. There are no separate release branches.

## Package versions

| Build | Package version | Example |
| --- | --- | --- |
| Release | The milestone's version | `2.13.0` |
| CI build of `develop` | Next version, date and build number | `2.14.0-ci-20261106.123` |
| Local build | Next version | `2.14.0-dev` |

The next version is set by `DevVersion` in [build/Common.props](https://github.com/picoe/Eto/blob/develop/build/Common.props).

## Assembly versions

The assembly version only includes the major and minor version, e.g. `2.13.0.0` for every 2.13.x release. Patch releases can then replace an earlier one without rebuilding the apps or plugins that use it. The file version adds the build date, so each build can still be told apart.

## Releasing

Releases are made from GitHub milestones:

1. Close a milestone named for the version, e.g. `2.14.0`. This builds the packages, pushes them to MyGet and creates a GitHub pre-release.
2. Test the pre-release, then mark it as a full release on GitHub. This pushes the packages to nuget.org.
3. After a minor release, a pull request is opened to move `develop` to the next version, e.g. `2.15.0-dev`.

If a release build fails, fix `develop`, then reopen and close the milestone to try again.
