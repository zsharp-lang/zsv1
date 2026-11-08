#!/usr/bin/env bash
# Builds the solution and runs every test project. Each one is a program that prints
# a line per check and exits with an error if any check failed. A few only print what
# they did, for reading.

cd "$(dirname "$0")/.." || exit 1

dotnet build ZSharp.slnx --nologo -v q || exit 1

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
