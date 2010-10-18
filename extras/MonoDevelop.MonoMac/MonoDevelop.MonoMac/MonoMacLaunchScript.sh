#!/bin/sh
# This file was auto-generated by MonoDevelop.MonoMac

##### Determine app name and locations #####

MACOS_DIR=$(cd "$(dirname "$0")"; pwd)
APP_ROOT=${MACOS_DIR%%/Contents/MacOS}
 
CONTENTS_DIR="$APP_ROOT/Contents"
RESOURCES_PATH="$CONTENTS_DIR/Resources"

APP_NAME=`echo $0 | awk -F"/" '{ printf("%s", $NF); }'`
ASSEMBLY=`echo $0 | awk -F"/" '{ printf("%s.exe", $NF); }'`
 
##### Environment setup #####

MONO_FRAMEWORK_PATH=/Library/Frameworks/Mono.framework/Versions/Current
export DYLD_FALLBACK_LIBRARY_PATH="$MONO_FRAMEWORK_PATH/lib:$DYLD_FALLBACK_LIBRARY_PATH"
export PATH="$MONO_FRAMEWORK_PATH/bin:$PATH"
export DYLD_LIBRARY_PATH="$RESOURCES_PATH:$DYLD_LIBRARY_PATH"

##### Mono check #####

"$MACOS_DIR/mono-version-check" "$APP_NAME" 2 6 7

##### Run the exe using Mono #####

# Work around a limitation in 'exec' in older versions of macosx
OSX_VERSION=$(uname -r | cut -f1 -d.)
if [ $OSX_VERSION -lt 9 ]; then  # If OSX version is 10.4
	EXEC="exec "
else
	EXEC="exec -a \"$APP_NAME\""
fi

$EXEC "$APP_ROOT/$APP_NAME" $MONO_OPTIONS "$RESOURCES_PATH"/"$ASSEMBLY"
