REM xcopy "..\..\..\..\Resources\media\*" ".\media" /S /C /Q /Y

REM rmdir /S /Q "$(TargetDir)media"
robocopy "..\..\..\..\Resources\media" ".\media" /E /NFL /NJS /NJH /NDL /NP

REM Following is a hack to remove the error code becuase robocopy returns 1 for success
dir %TEMP% >nul