@ECHO OFF
CLS

RD /S /Q ".\MimeTool\bin"
RD /S /Q ".\MimeTool\obj"
RD /S /Q ".\TnefTool\bin"
RD /S /Q ".\TnefTool\obj"
RD /S /Q ".\Tester\bin"
RD /S /Q ".\Tester\obj"

DEL /Q ".\bin\*.pdb"
DEL /Q ".\bin\*.vshost.exe"
DEL /Q ".\bin\*.vshost.exe.config"
DEL /Q ".\bin\*.vshost.exe.manifest"
