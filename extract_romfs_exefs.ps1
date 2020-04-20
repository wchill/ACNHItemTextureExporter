param(
    [string]$tempdir="workdir",
    [string]$xcifile="game.xci",
    [string]$nspfile="Update.nsp",
    [string]$hactool=".\hactool.exe",
    [string]$outputdir="output",
    [switch]$skipxci
)

$ErrorActionPreference = "Stop"

function GetNameOfLargestFile($dir) {
    pushd
    cd $dir
    $name = gci -r | sort Length -desc | select fullname -f 1
    popd
    return $name.FullName
}

md -Force $tempdir | Out-Null

$xcidir = "$tempdir\extracted_xci"
$nspdir = "$tempdir\extracted_nsp"

if (-NOT $skipxci) {
    Write-Host "Extracting base XCI file"
    & $hactool --intype=xci --securedir=$xcidir -x $xcifile | Out-Null
} else {
    Write-Host "Skipping extracting XCI file"
}
$basenca = GetNameOfLargestFile($xcidir)

Write-Host "Extracting NSP update file"
& $hactool -x --intype=pfs0 --pfs0dir=$nspdir $nspfile | Out-Null
$updatenca = GetNameOfLargestFile($nspdir)

md -Force $outputdir | Out-Null

& $hactool -x --romfsdir="$outputdir\romfs" --exefsdir="$outputdir\exefs" --basenca=$basenca $updatenca