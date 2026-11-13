#!/usr/bin/env bash
# Builds the solution and runs every test project. Each one is a program that prints
# a line per check and exits with an error if any check failed. A few only print what
# they did, for reading.

cd "$(dirname "$0")/.." || exit 1

dotnet build ZSharp.slnx --nologo -v q || exit 1

# The toolchain's Z# projects are built from their sources, and the Cecil platform must
# build itself the same, so the tests run with what a bootstrap builds.
bootstrap=$(dotnet src/CLI/ZSharp.CLI/bin/Debug/net10.0/zs.dll --no-prelude bootstrap.zs 2>&1) || { echo "$bootstrap"; exit 1; }
echo "$bootstrap" | grep -v '^wrote '

status=0

for project in tests/ZSharp.Tests.*/; do
    name=$(basename "$project")
    output=$(dotnet "$project/bin/Debug/net10.0/$name.dll" examples/hello.zs 2>&1)
    code=$?
    passed=$(grep -c '^pass' <<< "$output")
    failed=$(grep -c '^FAIL' <<< "$output")

    if [ "$passed" -eq 0 ] && [ "$failed" -eq 0 ]; then
        echo "$name: prints only, nothing is checked"
    else
        echo "$name: $passed passed, $failed failed"
        grep '^FAIL' <<< "$output"
    fi

    if [ "$code" -ne 0 ]; then
        echo "$name exited with $code"
        status=1
    fi
done

exit $status
