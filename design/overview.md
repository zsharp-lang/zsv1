# Z# Overview


Z# is a statically type-safe interpreted language that runs on the .NET runtime. It is inspired by languages
like C#, Python, TypeScript and C++.

The main purpose of the language it to allow users to define custom, domain-specific semantics without having
to manually implement the semantics by concept-mapping from high-level to language-level concepts.

## Design Documents

This directory contains sub-directories, where each sub-directory represents a system + contents.

## Project Structure

The language is implemented in C#. Every system should be implemented as its own C# project, and the
contents should also be implemented as a separate C# project(s).


The whole Z# project is split to multiple parts. We have 2 core systems, defined separately:
* Object Interaction System (OIS)
* Object Representation System (ORS)

These systems are defined in `design/ois/ois.md` and `design/ors.md`.

A super, critically important implementation requirement:
> An object may only ever participate in either OIS or ORS. **Never** both.

## Implementation

Implementation details are listed in `implementation.md`.
