# Stable automatic (recommended)
Run the following command in [termux](https://termux.com/):
- `pkg install -y wget`
- `wget -q -O - https://gitlab.com/JFronny/smalipatcher/-/raw/master/android/setup.sh?inline=false | bash`

Two script will be created:
- `smalipatcher`
- `smalipatcher-shell`

# Dev automatic
This works the same as the stable automatic install, except for the fact that you will need to run `wget -q -O - https://gitlab.com/JFronny/smalipatcher/-/raw/dev/android/dev/setup.sh?inline=false | bash`

# Manual
### Initial Setup (run in the same termux instance):
- download [SmaliPatcher for android](https://gitlab.com/JFronny/smalipatcher/-/jobs/artifacts/master/download?job=android)
- build vdexExtractor (https://github.com/anestisb/vdexExtractor) by cloning it and running build.sh in termux
- `pkg install proot-distro`
- `proot-distro install ubuntu`
- `proot-distro login ubuntu`
- `unset LD_PRELOAD` <-- this is required if you have a specific other package (termux-exec)
- `apt update`
- `apt install libssl-dev openjdk-14-jre-headless adb`
- `wget -q https://gitlab.com/JFronny/smalipatcher/-/jobs/artifacts/master/download?job=android -O tmp.zip && unzip tmp.zip && rm tmp.zip`
- `chmod a+x smp/SmaliPatcherMin`
- copy the vdexExtractor (/root/vdexExtractor) binaries over to proot and `chmod a+x` them
- download [cp_framework.sh](https://gitlab.com/JFronny/smalipatcher/-/raw/master/android/cp_framework.sh?inline=false) and [smalipatcher.sh](https://gitlab.com/JFronny/smalipatcher/-/raw/master/android/smalipatcher.sh?inline=false) and `chmod a+x` them
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
