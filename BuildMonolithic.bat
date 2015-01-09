@echo off
csc /nologo /out:Bin\WcCleaner-Monolithic.exe /optimize WcLib\WcWildcard.cs WcLib\WcParser.cs WcLib\WcPattern.cs WcLib\WcFileFilter.cs WcLib\WcLib.cs WcLib\WcLog.cs WcCleaner\WcCleaner.cs WcLib\WcSearchDirs.cs
