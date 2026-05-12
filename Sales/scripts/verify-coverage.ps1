#!/usr/bin/env pwsh
# Verifica testes unitários e cobertura mínima de linhas (81%) em Domain + Application.
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root
dotnet test "tests\Ambev.DeveloperEvaluation.Unit\Ambev.DeveloperEvaluation.Unit.csproj" -c Release -p:RunCoverageAnalysis=true
exit $LASTEXITCODE
