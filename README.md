# SmaliPatcher.JF
A "fork" of the decompiled code of [SmaliPatcher by fOmey](https://forum.xda-developers.com/apps/magisk/module-smali-patcher-0-7-t3680053)\
Supports Linux (maybe android and macOS, although they are not tested and not in CI) devices.\
The functionality should basically be the same as SmaliPatcher 6.9

## Download
You can download the latest CI builds for [Windows](https://gitlab.com/JFronny/smalipatcher/-/jobs/artifacts/master/download?job=windows) and [Linux](https://gitlab.com/JFronny/smalipatcher/-/jobs/artifacts/master/download?job=linux) or build it yourself (see [here](https://gitlab.com/JFronny/smalipatcher/-/blob/master/.gitlab-ci.yml))\
(Also: yes, the linux zip contains .dll files. This is the extension for .NET Core)

## What are the projects?
- "Original" is the decompiled source of SmaliPatcher 6.9 with just enough modifications for it to start. It is here for reference and slightly broken
- "SmaliLib" is the core functionality of SmaliPatcher repackaged into a .NET Standard library with cross-platform support and some modifications for readability
- "SmaliPatcher" is a recreation of the original UI code using MaterialSkin.2 and NETFX48 (because MaterialSkin doesn't support Core as of writing)
- "SmaliPatcherMin" is a custom text-based interface for SmaliLib and is cross-platform (WinForms is only partially supported cross-platform in Mono)
Everything else is just decoration