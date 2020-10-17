#!/bin/sh
unset LD_PRELOAD
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
./SmaliPatcherMin --no-cd --framework:./adb