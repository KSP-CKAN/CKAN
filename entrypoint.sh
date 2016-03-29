#!/bin/bash
HEAD="mono /build/CmdLine.exe"
TAIL="--asroot --headless"
$HEAD ksp add one /kspdir $TAIL
$HEAD ksp default one $TAIL
$HEAD update $TAIL
$HEAD upgrade --all $TAIL
chown --reference=/kspdir/GameData -R /kspdir
