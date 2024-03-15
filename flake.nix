{
  description = "Societal Construction Tool";
  inputs = {
    nixpkgs.url ="github:NixOS/nixpkgs/nixos-23.11";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = {
    self,
    nixpkgs,
    flake-utils,
    ...
  }:
  flake-utils.lib.eachDefaultSystem (
    system: let
      pkgs = import nixpkgs { inherit system;};
      projectFile = "./SocietalConstructionTool/SocietalConstructionTool.csproj";
      testProjectFile = "./SocietalConstructionToolTests/SocietalConstructionToolTests.csproj";
      parserDir = "./SocietalConstructionTool/parser";
      parserOutDir = "${parserDir}/out";
      grammarFile = "${parserDir}/Sct.g4";
      # TODO: Find some way to put this in the shell so you can manually generate sources for LSP
      generateSources = ''"${pkgs.antlr4}/bin/antlr4" -Dlanguage=CSharp ${grammarFile} -o ${parserOutDir}'';
      pname = "societal-construction-tool";
      dotnet-sdk = pkgs.dotnet-sdk_8;
      dotnet-runtime = pkgs.dotnetCorePackages.runtime_8_0;
      version = "0.0.1";
    in {
      packages = {
        fetchDeps = let
          flags = [];
          runtimeIds = map (system: pkgs.dotnetCorePackages.systemToDotnetRid system) dotnet-sdk.meta.platforms;
        in
          pkgs.writeShellScriptBin "fetch-${pname}-deps" (builtins.readFile (pkgs.substituteAll {
            src = ./nix/fetchDeps.sh;
            pname = pname;
            binPath = pkgs.lib.makeBinPath [pkgs.coreutils dotnet-sdk (pkgs.nuget-to-nix.override {inherit dotnet-sdk;})];
            projectFiles = toString (pkgs.lib.toList projectFile);
            testProjectFiles = toString (pkgs.lib.toList testProjectFile);
            rids = pkgs.lib.concatStringsSep "\" \"" runtimeIds;
            packages = dotnet-sdk.packages;
            storeSrc = pkgs.srcOnly {
              src = ./.;
              pname = pname;
              version = version;
            };
        }));
        default = pkgs.buildDotnetModule {
          inherit version dotnet-sdk dotnet-runtime;
          projectFile = "./sct.sln";
          pname = "SocietalConstructionTool";
          src = ./.;
          doCheck = true;
          nugetDeps = ./nix/deps.nix;
          preBuild = ''
            ${generateSources}
          '';
        };
      };
      devShells.default = import ./nix/shell.nix { inherit pkgs; };
    }
  );
}
