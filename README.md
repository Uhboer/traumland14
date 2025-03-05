# Finster

Underground RolePlay expirience in unique lore and content. Mostly based on Space Station 14. Made in abyss.

## Building

Follow [Space Wizards guide](https://docs.spacestation14.com/en/general-development/setup/setting-up-a-development-environment.html) on setting up a working environment, but keep in mind that the repository is different and some things may also be different.
EE make some scripts shown below to make work easier.

### Dependencies

> - Git
> - .NET SDK 9.0.101

### Windows

> 1. Clone this repository
> 2. Run `git submodule update --init --recursive` at the command line to download the game engine
> 3. Run `Tools/bat/buildAllDebug.bat` after any changes to the project code
> 4. Run `Tools/bat/runQuickAll.bat` to start the client and server
> 5. Connect to the local server and play

### Linux

> 1. Clone this repository.
> 2. Run `git submodule update --init --recursive` at the command line to download the game engine
> 3. Run `Tools/sh/buildAllDebug.sh` after any changes to the project code
> 4. Run `Tools/sh/runQuickAll.sh` to start the client and server
> 5. Connect to the local server and play

### MacOS

> Same as on Linux. Otherwise try use `dotnet build` to build the project.

## License

👉 [LEGAL.md](./LEGAL.md)
