@echo off
echo start > testloop.log
for /L %%n in (1,1,100) do (
echo TestLoop %%n >> testloop.log
timeout /t 1 >nul
)
echo end > testloop.log
