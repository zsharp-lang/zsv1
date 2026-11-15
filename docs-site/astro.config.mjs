// The Z# documentation as a website, with Astro and Starlight. The pages are the
// toolchain's, in ../docs, which scripts/sync.mjs copies into src/content/docs as they
// are, with what Starlight needs of a page: what the site adds is here and there.
import { readFileSync } from 'node:fs';
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

const zsharp = JSON.parse(readFileSync(new URL('./src/zsharp.tmLanguage.json', import.meta.url), 'utf8'));

// A group of pages, collapsed unless it is one a reader starts with.
const group = (label, items, collapsed = true) => ({ label, collapsed, items });

export default defineConfig({
	integrations: [
		starlight({
			title: 'Z#',
			description: 'The Z# language and toolchain.',
			customCss: ['./src/styles/zsharp.css'],
			expressiveCode: {
				shiki: { langs: [zsharp] },
				themes: ['github-dark-default', 'github-light-default'],
			},
			sidebar: [
				group('Learn', [
					group('Getting started', ['learn/getting-started/installation', 'learn/getting-started/hello-world'], false),
					group('Basics', [
						'learn/basics/basics',
						'learn/basics/control-flow',
						'learn/basics/functions',
						'learn/basics/classes',
						'learn/basics/interfaces',
						'learn/basics/modules',
						'learn/basics/dotnet',
						'learn/basics/projects',
						'learn/basics/not-yet',
					]),
					group('Advanced', [
						'learn/advanced/overview',
						group('Build scripts', [
							'learn/advanced/build-scripts/arguments',
							'learn/advanced/build-scripts/compiling-projects',
							'learn/advanced/build-scripts/toolchain-fork',
						]),
						group('Code generation', [
							'learn/advanced/code-generation/declarative',
							'learn/advanced/code-generation/programmatic',
						]),
						'learn/advanced/decorators',
						'learn/advanced/reflection',
						'learn/advanced/submodules',
						'learn/advanced/core-namespace',
						'learn/advanced/prelude',
					]),
					group('Custom syntax and definitions', [
						'learn/custom-syntax/syntax',
						'learn/custom-syntax/meta-types',
						'learn/custom-syntax/meta-compilers',
					]),
					group('Custom importers', [
						'learn/custom-importers/how-imports-work',
						'learn/custom-importers/file-suffix',
						'learn/custom-importers/prefix',
						'learn/custom-importers/source',
						'learn/custom-importers/parameters',
						'learn/custom-importers/nodes-and-loaders',
					]),
					group('Custom semantics', [
						'learn/custom-semantics/hir-to-lir',
						'learn/custom-semantics/compiler-objects',
						'learn/custom-semantics/common-reis',
					]),
					group('Custom dispatchers', [
						'learn/custom-dispatchers/reis',
						'learn/custom-dispatchers/when',
						'learn/custom-dispatchers/toolchain-reis',
					]),
					group('Custom platforms', [
						'learn/custom-platforms/overview',
						'learn/custom-platforms/lir',
						'learn/custom-platforms/capabilities',
						'learn/custom-platforms/dialects',
						'learn/custom-platforms/source-maps',
					]),
					group('The Z# CLI', [
						'learn/cli/arguments',
						'learn/cli/pipeline',
						'learn/cli/document-compiler',
						'learn/cli/runtime',
					]),
				], false),
				group('Reference', [
					'reference',
					group('Standard libraries', ['reference/std', { autogenerate: { directory: 'reference/std/libraries' } }]),
					group('Standard packages', [
						'reference/packages',
						'reference/packages/project-compiler',
						'reference/packages/package-manager',
						'reference/packages/cecil',
						'reference/packages/clr',
						'reference/packages/zsharp',
					]),
					group('Core', ['reference/core', 'reference/core/hir', 'reference/core/mir', 'reference/core/lir']),
					group('The Z# language', ['reference/language/syntax', 'reference/language/semantics']),
					'reference/runtime',
				]),
				group('Internals', [
					'internals/principles',
					'internals/bootstrap',
					group('The architecture', [{ autogenerate: { directory: 'internals/architecture' } }]),
				]),
			],
		}),
	],
});
