# The Z# documentation site

Serves the documentation in `../docs` as a website, with Astro and Starlight. The pages
stay as they are, since the toolchain's tests run their samples. `scripts/sync.mjs`
copies them into `src/content/docs`, with each page's title from its first heading, its
code blocks as the site shows them and its links to the site's pages. Every page is in
`../docs`, so the documentation can be read without the site. The navigation and the
theme are in `astro.config.mjs`. Z# is highlighted by
`src/zsharp.tmLanguage.json`.

From this folder:

```text
npm install       once, and when package.json changes
npm run dev       serves it, reloading as the site changes; npm run sync copies pages again
npm run build     writes it into dist/
```

`npm run dev` serves it on every interface, so another device on the network reaches it
at this machine's address, as `http://10.0.0.50:4321`.

A page added to `../docs` goes in the `sidebar` in `astro.config.mjs`, to be in the
navigation.
