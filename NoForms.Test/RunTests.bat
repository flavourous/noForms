@echo off
"C:\Program Files (x86)\NUnit 2.6.2\bin\nunit-console.exe" NoForms.Test.dll 
if errorlevel 1 "C:\Program Files (x86)\NUnit 2.6.2\bin\nunit.exe" /run NoForms.Test.dll