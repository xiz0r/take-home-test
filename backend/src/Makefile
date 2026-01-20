SHELL := /bin/bash

.PHONY: restore build test test-watch dev clean coverage

restore:
	dotnet restore

build:
	dotnet build -c Debug

test:
	dotnet test -c Debug

test-watch:
	dotnet watch test

dev:
	ASPNETCORE_ENVIRONMENT=Development dotnet watch run --project Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj

clean:
	dotnet clean
	rm -rf **/bin **/obj

coverage:
	rm -rf Fundo.Services.Tests/TestResults
	dotnet test Fundo.Services.Tests/Fundo.Services.Tests.csproj --collect:"XPlat Code Coverage"
	reportgenerator \
		-reports:./Fundo.Services.Tests/TestResults/**/coverage.cobertura.xml \
		-targetdir:./Fundo.Services.Tests/TestResults/coverage-report \
		-reporttypes:Html