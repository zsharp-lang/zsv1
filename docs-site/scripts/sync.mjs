// Copies the toolchain's pages into src/content/docs, which Starlight serves. The pages
// stay as they are, in ../docs, because the toolchain's tests run their samples; what a
// page needs only on the site is added here:
//
// - its title, from its first heading, as Starlight's frontmatter;
// - its code blocks as the site shows them: `zs fragment`, which the tests don't run, is
//   Z# like any other, and `output`, what the script before it prints, is titled as one;
// - its links, from files to the pages the site has of them.
//
// Pages that live with what they describe are served too, each as it is: the design
// document, in ../design, under the internals, and each standard library's README, in
// ../std, under the standard libraries.
import { existsSync, mkdirSync, readdirSync, readFileSync, rmSync, statSync, writeFileSync } from 'node:fs';
import { dirname, join, relative, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const site = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const root = resolve(site, '..');
const out = join(site, 'src', 'content', 'docs');

const posix = (path) => path.split(sep).join('/');

function* files(folder, accept) {
	for (const name of readdirSync(folder)) {
		const path = join(folder, name);
		if (statSync(path).isDirectory()) {
			if (name !== 'build' && name !== 'node_modules') yield* files(path, accept);
		} else if (accept(name)) yield path;
	}
}

// Each page served, from the file it is to where the site has it.
const served = new Map();
for (const path of files(join(root, 'docs'), (name) => name.endsWith('.md')))
	served.set(path, posix(relative(join(root, 'docs'), path)));

// The design document, in the order its README lists its pages.
const design = join(root, 'design');
const order = [...readFileSync(join(design, 'README.md'), 'utf8').matchAll(/\]\(([\w-]+\.md)\)/g)].map((match) => match[1]);
for (const path of files(design, (name) => name.endsWith('.md')))
	served.set(path, 'internals/architecture/' + (path.endsWith('README.md') ? 'index.md' : posix(relative(design, path))));

const libraries = join(root, 'std');
for (const path of files(libraries, (name) => name === 'README.md'))
	served.set(path, 'reference/std/libraries/' + posix(relative(libraries, dirname(path))) + '.md');

// Where the site serves a page: its path, lowercased as Starlight's slugs are, with an
// index being its folder.
function route(page) {
	const path = page.replace(/\.md$/, '').toLowerCase();
	if (path === 'index') return '/';
	return '/' + path.replace(/\/index$/, '') + '/';
}

let unresolved = 0;

function page(source, target) {
	let text = readFileSync(source, 'utf8').replace(/\r\n/g, '\n');

	const heading = text.match(/^# (.+)\n+/);
	const title = heading ? heading[1].trim() : target.replace(/\.md$/, '');
	if (heading) text = text.slice(heading.index + heading[0].length);

	text = text.replace(/^(\s*)```zs fragment\s*$/gm, '$1```zs');
	text = text.replace(/^(\s*)```output\s*$/gm, '$1```text title="Output"');

	text = text.replace(/\]\(([^)\s]+)\)/g, (link, address) => {
		if (/^[a-z]+:/i.test(address) || address.startsWith('#')) return link;
		const [file, anchor] = address.split('#');
		// Only a link to a page or a folder is one to resolve: `](x)` in code isn't one.
		if (!file.endsWith('.md') && !file.endsWith('/')) return link;
		const linked = resolve(dirname(source), file);
		const found = served.get(linked) ?? (existsSync(join(linked, 'README.md')) ? served.get(join(linked, 'README.md')) : undefined);
		if (found === undefined) {
			unresolved++;
			console.warn(`${posix(relative(root, source))}: no page for ${address}`);
			return link;
		}
		return '](' + route(found) + (anchor ? '#' + anchor : '') + ')';
	});

	const front = ['---', `title: ${JSON.stringify(title)}`];
	const position = order.indexOf(posix(relative(design, source)));
	if (source.startsWith(design) && position >= 0) front.push('sidebar:', `  order: ${position + 1}`);
	if (target === 'index.md') {
		const tagline = text.match(/^([^\n#][^]*?)\n\n/);
		front.push(
			'template: splash',
			'hero:',
			`  tagline: ${JSON.stringify(tagline ? tagline[1].replace(/\n/g, ' ') : '')}`,
			'  actions:',
			'    - text: Get started',
			'      link: /learn/getting-started/installation/',
			'      icon: right-arrow',
			'    - text: The language',
			'      link: /learn/basics/basics/',
			'      variant: minimal',
		);
		if (tagline) text = text.slice(tagline[0].length);
	}
	front.push('---', '');

	const written = join(out, target);
	mkdirSync(dirname(written), { recursive: true });
	writeFileSync(written, front.join('\n') + '\n' + text);
}

rmSync(out, { recursive: true, force: true });
for (const [source, target] of served) page(source, target);
console.log(`synced ${served.size} pages${unresolved ? `, ${unresolved} links with no page` : ''}`);
