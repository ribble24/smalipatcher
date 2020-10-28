#!/bin/sh
unset LD_PRELOAD
if [ "$1" != "--no-cp" ]
then
    ./cp_framework.sh
fi
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
./smp/SmaliPatcherMin --no-cd --framework:./adb $@
mv SmaliPatcherModule* /sdcard/