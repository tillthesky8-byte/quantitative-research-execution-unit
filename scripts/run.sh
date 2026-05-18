#!/bin/bash

ROOT=$(git rev-parse --show-toplevel)

dotnet run \
  --project "$ROOT/src/QREU.Application/QREU.Application.csproj" \
  -- "$@"