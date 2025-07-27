#!/bin/bash

sudo apt-get install jq unzip -y

if [ ! -d /$RUNNER_TEMP/engine-godot ]; then

    curl https://api.github.com/repos/godotengine/godot/releases/latest > /$RUNNER_TEMP/engine-ver

    cd /$RUNNER_TEMP/
    url=$(cat /$RUNNER_TEMP/engine-ver|jq '.assets[] | select (.name | endswith("_mono_linux_x86_64.zip"))' | jq '.browser_download_url')

    echo "$url"
    echo "$url"|xargs wget -q -O "godot.zip" -c {}

    unzip /$RUNNER_TEMP/godot.zip
    mv /$RUNNER_TEMP/Godot_* /$RUNNER_TEMP/engine-godot
    mv /$RUNNER_TEMP/engine-godot/Godot_* /$RUNNER_TEMP/engine-godot/godot

    rm /$RUNNER_TEMP/engine-ver
    rm /$RUNNER_TEMP/godot.zip
fi

if [ ! -d /$RUNNER_TEMP/templates ]; then

    curl https://api.github.com/repos/godotengine/godot/releases/latest > /$RUNNER_TEMP/templates-ver

    cd /$RUNNER_TEMP/
    url=$(cat /$RUNNER_TEMP/templates-ver|jq '.assets[] | select (.name | endswith("_mono_export_templates.tpz"))' | jq '.browser_download_url')

    echo "$url"
    echo "$url"|xargs wget -q -O "templates.tpz" -c {}

    unzip /$RUNNER_TEMP/templates.tpz
    
    rm /$RUNNER_TEMP/templates-ver
    rm /$RUNNER_TEMP/templates.tpz
fi

exit 0