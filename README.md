# Home Planner Fork (Sweet Home 3D 7.5)

GPL fork of [Sweet Home 3D](https://www.sweethome3d.com/) v7.5 with custom navigation and selection improvements.

**Upstream:** Sweet Home 3D is Copyright Space Mushrooms / Emmanuel Puybaret, licensed under GNU GPL v2+. This project modifies that source and must remain under the GPL if you distribute binaries.

## Custom features

| Preference | What it does |
|------------|----------------|
| Invert horizontal 3D navigation | Flips left/right look and strafe (mouse drag, keys, nav buttons) |
| Invert vertical 3D navigation | Flips up/down look, forward/back, wheel, and elevation |
| Enhanced touch input | Larger hit targets and touch-style selection timing on laptop touchscreens |
| Cycle overlapping selection | Repeated clicks at the same plan point cycle through stacked items |

Open **Preferences** in the app to enable these (English labels in `package.properties`).

## Quick start

```powershell
.\setup.ps1    # once: download Ant + source if missing
.\build.ps1    # after any code change (~20s)
.\run.ps1      # launch your build
```

Prerequisites: **JDK 17+** (`winget install Microsoft.OpenJDK.17`)

## GitHub

This folder is a git repo. To publish (replace `YOUR_USER` and repo name):

```bash
cd /c/Users/richa/Projects/sweethome3d-fork
git add .
git commit -m "Add custom navigation, touch, and overlap-selection preferences"
gh repo create YOUR_USER/sweethome3d-fork --public --source=. --remote=origin --push
```

Or create an empty repo on github.com, then:

```bash
git remote add origin https://github.com/YOUR_USER/sweethome3d-fork.git
git push -u origin main
```

Use a **distinct app name** if you distribute builds publicly (GPL still applies; trademark "Sweet Home 3D" belongs to the original project).

## Upstream source

- ZIP: https://prdownloads.sourceforge.net/sweethome3d/SweetHome3D-7.5-src.zip
- SVN: `svn checkout https://svn.code.sf.net/p/sweethome3d/code/trunk/SweetHome3D SweetHome3D-7.5-src`
