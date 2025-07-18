/*
 * This file is responsible for building a Z# project.
 *
 * Requirements:
 * [x] FS module
 *     [x] Directory Class
 *         [x] Operator /
 *         [x] Current Directory
 *	   [x] File Class
 *     [x] Path Class
 *         [x] Operator /
 * [x] `as` keyword for safe conversion
 * [ ] Nullable types `T?`
 * [x] `is of` operator for type matching
 * [ ] custom importer for source of type `File`
 * [ ] Project compiler
 * [ ] Expose the compiler to Z# (even without members)
 * [ ] .NET platform compiler
 * [ ] Expose Mono.Cecil ModuleDefinition class (specifically, the `Write(string)` method)
*/


import {
	Directory,
	File
} from "std:fs";


// The root path of the project. This is usually where this
// script is.
let projectRoot = Directory.cwd();

// Source root is the root path of all project source files.
// Only files in this path are considered for discovery.
let sourceRoot = (projectRoot / "src") as Directory? or projectRoot;

// Custom init file so users don't have to mess with this file.
if ((projectRoot / "init.zs") is initFile of File)
	import initFile;

// Project compiler is a specialized source compiler for compiling
// projects.
import {
	Project,
	ProjectCompiler
} from "net:ZSharp.ZSProjectCompiler.dll";

// The Project object holds the state of compiling the project.
let project = Project(
	projectRoot.name, // project name
	projectRoot, // project root directory
	sourceRoot, // source files root directory
);

project.discoverZSFiles(); // populates project.sourceFiles

// Import the current compiler
import { compiler } from "core:compiler";

let projectCompiler = ProjectCompiler(compiler);
projectCompiler.compile(project);

// Create output directory if it doesn't exist
let outDir = projectRoot.createDirectory("dist", existsOk: true);

// Import the .NET platform compiler
import { 
	Compiler as NETCompiler
} from "core:platform/.net";

let netCompiler = NETCompiler();

for (let module in project.modules) { // IR
	let netModule = netCompiler.compile(module);
	netModule.write(module.name + if (module.entryPoint) ".exe" else ".dll");
}
