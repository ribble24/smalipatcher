### Initial Setup (run in the same termux instance):
- `apt update`
- `apt install libssl-dev openjdk-14-jre-headless adb`
- run `dotnet publish -r android.21-arm64 -c Release --self-contained true` on your pc (in SmaliPatcherMin)
- build vdexExtractor (https://github.com/anestisb/vdexExtractor) by cloning it and running build.sh in termux
- `pkg install proot-distro`
- `proot-distro install ubuntu`
- `proot-distro login ubuntu`
- `unset LD_PRELOAD` <-- this is required if you have a specific other package (termux-exec)
- copy the SmaliPatcherMin (/root/smp/*) and vdexExtractor (/root/vdexExtractor) binaries over to proot and `chmod a+x` them
- download cp_framework.sh and smalipatcher.sh and `chmod a+x` them
- `./cp_framework.sh`
- `adb connect 127.0.0.1` with adb-over-network and confirm the popup
- `./smalipatcher.sh` <-- If this runs you are done - The next step is only if you want to use this again
- You can now append `--no-download` to the `SmaliPatcherMin` command in `smalipatcher.sh`

### Re-entering smali-patcher:
- `proot-distro login ubuntu`
- `unset LD_PRELOAD` <-- same as above
- `adb devices` <-- if not connected run `adb connect 127.0.0.1` as above
- `./smalipatcher.sh`

The module will end up in the proots `/root`. You can copy it to you internal storage with `cp SmaliPatcherModule* /sdcard/`

### This sounds way too complicated
I do plan to create a script that automates this process (or maybe even a full app that packages SmaliLib)
but for now this is enough for me to prove functionality