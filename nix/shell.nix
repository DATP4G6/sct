{ pkgs ? import <nixpkgs> }:
pkgs.mkShell {
  buildInputs = with pkgs; [
    antlr4
    dotnet-sdk_8
    dotnetCorePackages.runtime_8_0
    omnisharp-roslyn
    netcoredbg # Debug adapter interfacing with the nvim-dab bridge
  ];
}
