echo Copying DB file to output folder

echo * ProjectDir %1
echo * OutDir %2

xcopy "%1Database\APITESTData.db" %2 /y

