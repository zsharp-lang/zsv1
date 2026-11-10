@echo off
rem Installs the Z# toolchain this is part of:
rem   install.cmd [folder] [--no-path]    or    install.cmd --uninstall [folder]
"%~dp0bin\zs.exe" "%~dp0install.zs" %*
