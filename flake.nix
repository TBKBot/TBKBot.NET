{
  description = "Description for the project";

  inputs = {
    flake-parts.url = "github:hercules-ci/flake-parts";
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  };

  outputs = inputs @ {flake-parts, ...}:
    flake-parts.lib.mkFlake {inherit inputs;} {
      systems = ["x86_64-linux" "aarch64-linux"];
      perSystem = {
        self',
        pkgs,
        ...
      }: {
        packages.default = pkgs.buildDotnetModule {
          pname = "TBKBot.NET";
          version = "0.1.0-rolling.2";

          src = ./.;

          dotnet-sdk = pkgs.dotnetCorePackages.sdk_8_0;
          dotnet-runtime = pkgs.dotnetCorePackages.runtime_8_0;

          projectFile = "TBKBot.csproj";
          nugetDeps = ./Nix/deps.nix;
        };
      };
      flake = {
        nixosModules = {};
      };
    };
}
