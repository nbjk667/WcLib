@echo off
csc /nologo /target:library /out:Bin\WcLib.dll /optimize WcLib\WcWildcard.cs WcLib\WcParser.cs WcLib\WcPattern.cs WcLib\WcFileFilter.cs WcLib\WcLib.cs WcLib\WcLog.cs WcLib\WcSearchDirs.cs
csc /nologo /reference:Bin\WcLib.dll /out:Bin\WcCleaner.exe /optimize WcCleaner\WcCleaner.cs
