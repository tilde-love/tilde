rem dotnet publish --no-restore --output ../../builds/tilde --configuration Release --runtime win10-x64

dotnet tool uninstall -g tilde
dotnet pack -c Release
dotnet tool install -g --add-source ../../.package-store tilde --version 1.0.2-preview-19280.61