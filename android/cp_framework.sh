#!/bin/bash
rm -rf adb
mkdir adb

copyDir() {
  if [[ -e "$1" && -r "$1" ]]
  then
    if [[ -d "$1" ]]
    then
      cp -R "$1" adb/
    else
      cp "$1" adb/
    fi
  fi
}

copyDir /system/build.prop
copyDir /system/framework
copyDir /system/system/build.prop
copyDir /system/system/framework