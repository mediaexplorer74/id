//
// This file allows the IL interpreter to provide extensions to lmr for itself that aren't provided
// in the official lmr build.  In the official lmr build, this file will not get compiled.  When building
// with the IL interpreter, however, this file will get compiled an the interpreter source tree will
// substitute this blank file with code that contains the IL interpreter-specific extensions to lmr.
//