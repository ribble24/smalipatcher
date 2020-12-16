#!/bin/bash
rm -rf adb
mkdir adb

copyDir() {
  if [[ -e "$1" && -r "$1" ]]
  then
    cp --no-preserve=all -R "$1" adb/
  fi
}

copyDir /system/build.prop
copyDir /system/framework
copyDir /system/system/build.prop
copyDir /system/system/framework
