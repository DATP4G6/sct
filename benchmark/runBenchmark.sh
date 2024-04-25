#!/usr/bin/env sh
python3 ./generateBenchmarkFiles.py "$1"
pushd ..
hyperfine --prepare "dotnet build" --parameter-scan size 0 "$1" 'dotnet run --no-build --project SocietalConstructionTool -- "benchmark/benchmark_files/bm{size}.sct"'
popd
