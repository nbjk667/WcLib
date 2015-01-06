
; This is a commentary.
; To write a commentary make semicolon the first non-whitespace character in the line.

; If you have worked with Git and have seen .gitignore files
; you may find the format of this file somewhat familiar.
; This is not a coincidence. :)

;
; SIMPLE PATTERNS
;

; Select all files with the extension "a" in ALL directories.
*.a
; Select all files with the extension "c1" in the ROOT directory.
/*.c1
; Select all files with the extension "c2" in the "sub2".
/sub2/*.c2
; Select all files in the "rec" but NOT in its subdirectories.
/rec/*
; Select all files in the "rec2" and its subdirectories.
/rec2/**
; Select "qfile42.q" and "qfileAB.q" in the "qfiles", but not "qfile451.q".
qfile??.q
; Select all files in ANY "blah" folder.
**/blah/*

;
; SELECTION AND UNSELECTION
;

; Use ! to unselect a subset of previously selected files.
; Uselected files can be selected again using a normal selection pattern.

; Select all files with the extension "42",
; but unselect those in the "safe42",
; but then select those whose name starts with "zz".
*.42
!/safe42/*.42
/safe42/zz*.42

;
; LOCKING
;

; You can LOCK a subset of files using !!
; Locking is similar to unselection but locked files
; can never be selected again.

; In this example "loc" files in the "locked" will not be selected.
!!/locked/**
*.loc

;
; PREPROCESSOR
;

; Use #include to insert patterns from other files.
; Here we include the "Base.w" file which contains a
; patterns that selects all files with the extension
; "base".

#include "Base.w"

; Then we unselect some of the selected "base" files.
!/unbase/*.base

; You can use #if, #else and #endif to conditionally
; activate and deactivate sections of the file.

#if UNDEFINED

; Since "UNDEFINED" is not defined,
; this pattern will not be included.

*.unselected

#else

; This pattern will be included and the file with
; the extension "selected" will be added to set of
; selected files.

*.selected

#endif

; You can use ! to invert condition, like in the example below.

#if !UNDEFINED_CONSTANT

; Since the constant "UNDEFINED_CONSTANT" is not defined,
; this will be active.

*.undefined

#endif

; If you run the program using "TestCleaner.bat",
; the constant "DEFINED_CONSTANT" will be defined.

#if DEFINED_CONSTANT
*.defined
#endif

